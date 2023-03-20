using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Bedrock.Framework
{
    public class SocketConnection : ConnectionContext
    {
        private readonly Socket _socket;
        private volatile bool _aborted;
        private AddressFamily _addressFamily;
        private string _host;
        private int _port;
        private IDuplexPipe _application;
        private readonly SocketSender _sender;
        private readonly SocketReceiver _receiver;
        private readonly CancellationTokenSource _connectionClosedTokenSource = new CancellationTokenSource();
        private readonly TaskCompletionSource<bool> _waitForConnectionClosedTcs = new TaskCompletionSource<bool>();

        public SocketConnection(AddressFamily family, string host, int port)
        {
            this._addressFamily = family;
            this._host = host;
            this._port = port;

            _socket = new Socket(family, SocketType.Stream, ProtocolType.Tcp);

            _sender = new SocketSender(_socket, PipeScheduler.ThreadPool);
            _receiver = new SocketReceiver(_socket, PipeScheduler.ThreadPool);

            ConnectionClosed = _connectionClosedTokenSource.Token;
        }

        public override IDuplexPipe Transport { get; set; }
        public override async ValueTask DisposeAsync()
        {
            if (Transport != null)
            {
                await Transport.Output.CompleteAsync().ConfigureAwait(false);
                await Transport.Input.CompleteAsync().ConfigureAwait(false);
            }

            // Completing these loops will cause ExecuteAsync to Dispose the socket.
        }

        public async ValueTask<ConnectionContext> StartAsync(int timeOut = 5000)
        {
            var task = _socket.ConnectAsync(_host, _port);
            var tokenSource = new CancellationTokenSource();
            var completeTask = await Task.WhenAny(task, Task.Delay(timeOut, tokenSource.Token));
            if (completeTask != task)
            {
                try
                {
                    _socket.Close();
                    _socket.Dispose();
                }
                catch (Exception)
                {

                }
                return null;
            }
            else
            {
                tokenSource.Cancel();
                await task;
            }

            var pair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);

            LocalEndPoint = _socket.LocalEndPoint;
            RemoteEndPoint = _socket.RemoteEndPoint;

            Transport = pair.Transport;
            _application = pair.Application;

            _ = ExecuteAsync();

            return this;
        }

        private async Task ExecuteAsync()
        {
            Exception sendError = null;
            try
            {
                // Spawn send and receive logic
                var receiveTask = DoReceive();
                var sendTask = DoSend();

                // If the sending task completes then close the receive
                // We don't need to do this in the other direction because the kestrel
                // will trigger the output closing once the input is complete.
                if (await Task.WhenAny(receiveTask, sendTask).ConfigureAwait(false) == sendTask)
                {
                    // Tell the reader it's being aborted
                    _socket.Dispose();
                }

                // Now wait for both to complete
                await receiveTask;
                sendError = await sendTask;

                // Dispose the socket(should noop if already called)
                _socket.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected exception in {nameof(SocketConnection)}.{nameof(StartAsync)}: " + ex);
            }
            finally
            {
                // Complete the output after disposing the socket
                _application.Input.Complete(sendError);
            }
        }

        private async Task DoReceive()
        {
            Exception error = null;

            try
            {
                await ProcessReceives().ConfigureAwait(false);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
            {
                error = new ConnectionResetException(ex.Message, ex);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted ||
                                             ex.SocketErrorCode == SocketError.ConnectionAborted ||
                                             ex.SocketErrorCode == SocketError.Interrupted ||
                                             ex.SocketErrorCode == SocketError.InvalidArgument)
            {
                if (!_aborted)
                {
                    // Calling Dispose after ReceiveAsync can cause an "InvalidArgument" error on *nix.
                    error = new ConnectionAbortedException();
                }
            }
            catch (ObjectDisposedException)
            {
                if (!_aborted)
                {
                    error = new ConnectionAbortedException();
                }
            }
            catch (IOException ex)
            {
                error = ex;
            }
            catch (Exception ex)
            {
                error = new IOException(ex.Message, ex);
            }
            finally
            {
                if (_aborted)
                {
                    //error ??= new ConnectionAbortedException();
                    if (error == null)
                        error = new ConnectionAbortedException();
                }

                await _application.Output.CompleteAsync(error).ConfigureAwait(false);
                FireConnectionClosed();
                await _waitForConnectionClosedTcs.Task;
            }
        }

        private bool _connectionClosed;
        private void FireConnectionClosed()
        {
            //Console.WriteLine($"{ConnectionId} closed");
            //Guard against scheduling this multiple times
            if (_connectionClosed)
            {
                return;
            }
            _connectionClosed = true;
            ThreadPool.UnsafeQueueUserWorkItem(OnConnectionClosed, this);
        }

        private void OnConnectionClosed(object state)
        {
            if (state is SocketConnection sc)
            {
                sc.CancelConnectionClosedToken();
                sc._waitForConnectionClosedTcs.TrySetResult(true);
            }
        }

        public override CancellationToken ConnectionClosed { get; set; }
        public override IFeatureCollection Features => throw new NotImplementedException();
        public override string ConnectionId { get; set; } = Guid.NewGuid().ToString();
        public override IDictionary<object, object> Items { get; set; } = new ConcurrentDictionary<object, object>();

        private void CancelConnectionClosedToken()
        {
            try
            {
                _connectionClosedTokenSource.Cancel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected exception in {nameof(SocketConnection)}.{nameof(CancelConnectionClosedToken)}.{ex}");
            }
        }

        private async Task ProcessReceives()
        {
            while (true)
            {
                // Ensure we have some reasonable amount of buffer space
                var buffer = _application.Output.GetMemory();

                var bytesReceived = await _receiver.ReceiveAsync(buffer);

                if (bytesReceived == 0)
                {
                    // FIN
                    break;
                }

                _application.Output.Advance(bytesReceived);

                var flushTask = _application.Output.FlushAsync();

                if (!flushTask.IsCompleted)
                {
                    await flushTask.ConfigureAwait(false);
                }

                var result = flushTask.Result;
                if (result.IsCompleted)
                {
                    // Pipe consumer is shut down, do we stop writing
                    break;
                }
            }
        }

        private async Task<Exception> DoSend()
        {
            Exception error = null;

            try
            {
                await ProcessSends().ConfigureAwait(false);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
                error = null;
            }
            catch (ObjectDisposedException)
            {
                error = null;
            }
            catch (IOException ex)
            {
                error = ex;
            }
            catch (Exception ex)
            {
                error = new IOException(ex.Message, ex);
            }
            finally
            {
                _aborted = true;
                _socket.Shutdown(SocketShutdown.Both);
            }

            return error;
        }

        private async Task ProcessSends()
        {
            while (true)
            {
                // Wait for data to write from the pipe producer
                var result = await _application.Input.ReadAsync().ConfigureAwait(false);
                var buffer = result.Buffer;

                if (result.IsCanceled)
                {
                    break;
                }

                var end = buffer.End;
                var isCompleted = result.IsCompleted;
                if (!buffer.IsEmpty)
                {
                    await _sender.SendAsync(buffer);
                }

                _application.Input.AdvanceTo(end);

                if (isCompleted)
                {
                    break;
                }
            }
        }

    }
}
