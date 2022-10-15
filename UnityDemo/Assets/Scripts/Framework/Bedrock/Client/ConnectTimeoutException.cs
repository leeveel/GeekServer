using System;

namespace Bedrock.Framework
{
    public class ConnectTimeoutException : Exception
    {
        public ConnectTimeoutException(string message)
            : base(message)
        {
        }

        public ConnectTimeoutException()
        {
        }
    }
}
