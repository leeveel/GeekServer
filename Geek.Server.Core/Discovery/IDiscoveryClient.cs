namespace Core.Discovery
{
    public interface IDiscoveryClient
    {
        public void ServerChanged(List<ServerInfo> nodes);
        public void HaveMessage(string eid, byte[] msg);
    }
}
