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
            int uid;
            string msg;
            bool cmp = false;
            public Awaiter(int uid,string msg)
            {
                this.uid = uid;
                this.msg = msg;
                timer = new Timer(TimeOut, null, 10000, -1);
            }

            public void OnCompleted(Action continuation)
            {
                if (IsCompleted)
                {  
                    continuation();
                }else
                {
                    _callback += continuation;
                }
            }
 

            public void Complete(bool result)
            { 
                if(!cmp)
                {
                    cmp = true;
                    this.result = result;
                    timer.Dispose();
                    _callback();
                }
            }

            void TimeOut(object state)
            {
                Log.Error($"等待消息超时:{uid} {msg} {cmp}");
                Complete(false); 
            }
        }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<int, Awaiter> waitDic = new();

        //long id;
        //public MsgWaiter(long id)
        //{
        //    this.id = id;
        //}

        public void Clear()
        {
            foreach (var kv in waitDic)
                kv.Value.Complete(false);
            waitDic.Clear();
        }

        public Awaiter StartWait(int uniId,string msg)
        {
            Awaiter waiter = null;
            lock (waitDic)
            {
                if (!waitDic.ContainsKey(uniId))
                {
                    waiter = new Awaiter(uniId,msg);
                    waitDic.Add(uniId, waiter);
                }
                else
                {
                    Log.Error("发现重复消息id：" + uniId);
                }
                return waiter;
            }
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
                waiter?.Complete(result); 
            }
        }
    }
}
