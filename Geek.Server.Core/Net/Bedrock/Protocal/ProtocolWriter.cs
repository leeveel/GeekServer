using Microsoft.AspNetCore.Connections;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Bedrock.Framework.Protocols
{

    public static class ProtocolWriterCreator
    {
        public static ProtocolWriter CreateWriter(this ConnectionContext connection)
            => new ProtocolWriter(connection.Transport.Output);

        public static ProtocolWriter CreateWriter(this IDuplexPipe pipe)
            => new ProtocolWriter(pipe.Output);
    }

    public class ProtocolWriter //: IAsyncDisposable
    {
        private readonly PipeWriter _writer;
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        public ProtocolWriter(PipeWriter writer)
            : this(writer, new SemaphoreSlim(1))
        {
        }

        public ProtocolWriter(PipeWriter writer, SemaphoreSlim semaphore)
        {
            _writer = writer;
            _semaphore = semaphore;
        }

        public async ValueTask WriteAsync<TMessage>(IProtocal<TMessage> protocal, TMessage protocolMessage, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (_disposed)
                {
                    return;
                }

                protocal.WriteMessage(protocolMessage, _writer);

                var result = await _writer.FlushAsync(cancellationToken).ConfigureAwait(false);

                if (result.IsCanceled)
                {
                    throw new OperationCanceledException();
                }

                if (result.IsCompleted)
                {
                    _disposed = true;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }


        public async ValueTask DisposeAsync()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
