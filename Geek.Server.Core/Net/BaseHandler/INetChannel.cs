namespace Geek.Server.Core.Net.BaseHandler
{
    public interface INetChannel
    {
        public ValueTask Write(object msg);
        public T GetData<T>(string key);
        public void SetData(string key, object v);
        public void RemoveData(string key);
        public Task Close(bool triggerCloseEvt = true);
        public string RemoteAddress { get; }
        public bool IsClose();
        public Task StartAsync();
    }
}
