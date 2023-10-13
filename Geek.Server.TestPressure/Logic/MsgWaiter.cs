using Quartz;
using SharpCompress.Writers;
using System.Runtime.CompilerServices;

namespace Geek.Server.TestPressure.Logic
{
    public class MsgWaiter
    {
        public class Awaiter : INotifyCompletion
        {
            private Action _callback = () => { };
            private bool result = false;
            private Timer timer;
            public bool IsCompleted => cmp;
            public bool GetResult() => result;
            public Awaiter GetAwaiter() => this;

            volatile bool cmp;

            public Awaiter(float timeoutSeconds)
            {
                timer = new Timer(TimeOut, null, (int)(timeoutSeconds * 1000), -1);
            }

            public void OnCompleted(Action continuation)
            {
                if (IsCompleted)
                {
                    continuation();
                }
                else
                {
                    _callback += continuation;
                }
            }

            public void Complete(bool result)
            {
                if (!cmp)
                {
                    cmp = true;
                    this.result = result;
                    timer.Dispose();
                    _callback();
                }
            }

            void TimeOut(object state)
            {
                Complete(false);
                Log.Error("等待消息超时");
            }
        }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<int, Awaiter> waitDic = new();

        public void Clear()
        {
            foreach (var kv in waitDic)
                kv.Value.Complete(false);
            waitDic.Clear();
        }

        public Awaiter StartWait(int uniId, float timeoutSeconds = 15)
        {
            Awaiter waiter = null;
            lock (waitDic)
            {
                if (!waitDic.ContainsKey(uniId))
                {
                    waiter = new Awaiter(timeoutSeconds);
                    waitDic.Add(uniId, waiter);
                }
                else
                {
                    Log.Error("发现重复消息id：" + uniId);
                }
            }
            return waiter;
        }

        public void EndWait(int uniId, bool result = true)
        {
            if (!result) Log.Error("await失败：" + uniId);
            Awaiter waiter = null;
            lock (waitDic)
            {
                if (waitDic.ContainsKey(uniId))
                {
                    waiter = waitDic[uniId];
                    waitDic.Remove(uniId);
                }
                else
                {
                    if (uniId > 0)
                        Log.Error("找不到EndWait：" + uniId + ">size：" + waitDic.Count);
                }
            }
            waiter?.Complete(result);
        }
    }
}
