namespace Geek.Server.Core.Net.Bedrock.Client
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
