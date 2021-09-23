namespace Geek.Client
{
    public abstract class BaseMessage
    {
        public virtual bool doCache => false;
        /// <summary>
        /// ÏûÏ¢Î¨Ò»id
        /// </summary>
        public int UniId { get; set; }

        public virtual int Read(byte[] buffer, int offset)
        {
            return offset;
        }

        public virtual int Write(byte[] buffer, int offset)
        {
            return offset;
        }
        
        public virtual int WriteWithType(byte[] buffer, int offset)
        {
            return offset;
        }

        public virtual void Reset()
        {

        }

        public virtual int GetMsgId()
        {
            return 0;
        }
        
        public byte[] GetLocalData()
        {
            var data = NetBufferPool.Alloc(1024);
            int offset = this.Write(data, 0);
            if(offset > data.Length)
            {
                NetBufferPool.Free(data);
                data = NetBufferPool.Alloc(offset);
                offset = this.Write(data, 0);
            }
            return data;
        }
    }
}