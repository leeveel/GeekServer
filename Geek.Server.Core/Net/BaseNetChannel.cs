using Geek.Server.Core.Net.Kcp;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Geek.Server.Core.Net
{
    public abstract class BaseNetChannel
    {
        public virtual string RemoteAddress { get; set; } = "";
        public long NetId { get; set; } = 0;
        public int TargetServerId { get; set; }
        private ConcurrentDictionary<string, object> Datas { get; set; } = new();
        public T GetData<T>(string key)
        {
            if (Datas.TryGetValue(key, out var v))
            {
                return (T)v;
            }
            return default;
        }
        public void SetData(string key, object v)
        {
            Datas[key] = v;
        }

        public void RemoveData(string key)
        {
            Datas.TryRemove(key, out _);
        }

        public virtual void Write([NotNull] Message msg) => throw new NotImplementedException();
        public virtual void Write(TempNetPackage package) => throw new NotImplementedException();
        public virtual void Close() => throw new NotImplementedException();
        public virtual bool IsClose() => throw new NotImplementedException();

        protected long lastRecvMessageTime;
        public void UpdateRecvMessageTime(long offsetTicks = 0)
        {
            lastRecvMessageTime = DateTime.UtcNow.Ticks + offsetTicks;
        }

        public long GetLastMessageTimeSecond(in DateTime utcTime)
        {
            return (utcTime.Ticks - lastRecvMessageTime) / 10000_000;
        }
    }
}
