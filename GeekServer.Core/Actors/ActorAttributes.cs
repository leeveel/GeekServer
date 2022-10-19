using System;

namespace Geek.Server
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AsyncApi : Attribute
    {
        private bool isAwait;
        protected bool threadsafe;
        protected int timeout;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isAwait">是否等待执行</param>
        /// <param name="threadsafe">是否线程安全</param>
        /// <param name="timeout">超时时间</param>
        public AsyncApi(bool isAwait=true, bool threadsafe=true, int timeout=Actor.TIME_OUT)
        {
            this.isAwait = isAwait;
            this.threadsafe = threadsafe;   
            this.timeout = timeout; 
        }
    }

}
