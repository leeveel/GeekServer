namespace LiteDB.Async
{
    /// <summary>
    /// An exception was raised by the background thread. Throws this, the original exception is the InnerException
    /// </summary>
    public class LiteAsyncException : System.Exception
    {
        public LiteAsyncException() : base() { }
        public LiteAsyncException(string message) : base(message) { }
        public LiteAsyncException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected LiteAsyncException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}
