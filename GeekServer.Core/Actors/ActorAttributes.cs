using System;

namespace Geek.Server
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AsyncApi : Attribute
    {
        private bool isAwait;
        protected int timeout;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isAwait">是否等待执行</param>
        /// <param name="timeout">超时时间</param>
        public AsyncApi(bool isAwait=true, int timeout=Actor.TIME_OUT)
        {
            this.isAwait = isAwait;
            this.timeout = timeout; 
        }
    }

}
