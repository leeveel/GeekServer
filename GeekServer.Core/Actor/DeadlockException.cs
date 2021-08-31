using System;

namespace Geek.Server
{
    public class DeadlockException : Exception
    {
        public DeadlockException() { }
        public DeadlockException(string msg) : base(msg) { }
    }
}
