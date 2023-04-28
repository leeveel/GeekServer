namespace Common.Net.Tcp
{
    public interface INetChannel
    {
        public ValueTask Write(object msg);
        public T GetData<T>(string key);
        public void SetData(string key, object v);
        public void Close();
        public long ResCode { get; set; }
        public string RemoteAddress { get; }
        public long DefaultTargetNodeId { get; set; }
        public long NetId { get; set; }
        public bool IsClose();

    }
}
