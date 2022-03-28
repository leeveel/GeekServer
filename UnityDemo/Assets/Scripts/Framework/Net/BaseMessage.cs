namespace Geek.Client
{
    public abstract class BaseMessage : Serializable
    {
        public virtual bool doCache => false;
        /// <summary>
        /// ÏûÏ¢Î¨Ò»id
        /// </summary>
        public int UniId { get; set; }
        public int MsgId { get { return Sid; } }

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