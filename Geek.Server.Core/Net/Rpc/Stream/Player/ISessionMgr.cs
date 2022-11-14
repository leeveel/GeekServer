namespace Geek.Server
{
    public interface ISessionMgr
    {
        public Task Remove(long id);

        public Task RemoveAll();

        public Session Get(long id);

        public Session Add(Session session);
    }
}