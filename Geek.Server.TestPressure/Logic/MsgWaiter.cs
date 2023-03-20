using Quartz;
using SharpCompress.Writers;
using System.Runtime.CompilerServices;

namespace Geek.Server.TestPressure.Logic
{
    public class MsgWaiter
    {
        public class Awaiter : INotifyCompletion
        {
            private static readonly Action _callbackCompleted = () => { };
            private Action _callback;
            private bool result = false;
            private Timer timer;
            public bool IsCompleted => ReferenceEquals(_callback, _callbackCompleted);
            public bool GetResult() => result;
            public Awaiter GetAwaiter() => this;

            public Awaiter()
            {
                timer = new Timer(TimeOut, null, 10000, -1);
            }

            public void OnCompleted(Action continuation)
            {
                if (ReferenceEquals(_callback, _callbackCompleted) || ReferenceEquals(Interlocked.CompareExchange(ref _callback, continuation, null), _callbackCompleted))
                {
                    continuation();
                }
            }

            public void Complete(bool result)
            {
                this.result = result;
                var continuation = Interlocked.Exchange(ref _callback, _callbackCompleted);

                if (continuation != null)
                {
                    continuation();
                    timer.Dispose();
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

        public Awaiter StartWait(int uniId)
        {
            Awaiter waiter = null;
            lock (waitDic)
            {
                if (!waitDic.ContainsKey(uniId))
                {
                    waiter = new Awaiter();
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