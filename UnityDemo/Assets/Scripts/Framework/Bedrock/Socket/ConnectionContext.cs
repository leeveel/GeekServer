using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bedrock.Framework
{
    public abstract class ConnectionContext 
    {
        /// <summary>
        /// Gets or sets the <see cref="IDuplexPipe"/> that can be used to read or write data on this connection.
        /// </summary>
        public abstract IDuplexPipe Transport { get; set; }


        /// <summary>
        /// Gets or sets a unique identifier to represent this connection in trace logs.
        /// </summary>
        public abstract string ConnectionId { get; set; }

        /// <summary>
        /// Gets the collection of features provided by the server and middleware available on this connection.
        /// </summary>
        //public abstract IFeatureCollection Features { get; }

        /// <summary>
        /// Gets or sets a key/value collection that can be used to share data within the scope of this connection.
        /// </summary>
        public abstract IDictionary<object, object> Items { get; set; }

        /// <summary>
        /// Triggered when the client connection is closed.
        /// </summary>
        public virtual CancellationToken ConnectionClosed { get; set; }

        /// <summary>
        /// Gets or sets the local endpoint for this connection.
        /// </summary>
        public virtual EndPoint LocalEndPoint { get; set; }

        /// <summary>
        /// Gets or sets the remote endpoint for this connection.
        /// </summary>
        public virtual EndPoint RemoteEndPoint { get; set; }

        /// <summary>
        /// Releases resources for the underlying connection.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> that completes when resources have been released.</returns>
        public virtual ValueTask DisposeAsync()
        {
            return default;
        }

        /// <summary>
        /// Aborts the underlying connection.
        /// </summary>
        public virtual ValueTask Abort()
        {
            return DisposeAsync();
        }
    }
}
