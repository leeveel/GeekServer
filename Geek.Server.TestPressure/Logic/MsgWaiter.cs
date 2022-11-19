namespace Geek.Server.TestPressure.Logic
{
    public class MsgWaiter
    {
        class innerWaiter
        {
            public TaskCompletionSource<bool> Tcs { private set; get; }

            public Timer Timer { private set; get; }
            public void Start()
            {
                Tcs = new TaskCompletionSource<bool>();
                Timer = new Timer(TimeOut, null, 10000, -1);
            }

            public void End(bool result)
            {
                Timer.Dispose();
                if (Tcs != null)
                    Tcs.TrySetResult(result);
                Tcs = null;
            }

            public void TimeOut(object state)
            {
                End(false);
                Log.Error("等待消息超时");
            }
        }
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<int, innerWaiter> waitDic = new();

        public void Clear()
        {
            foreach (var kv in waitDic)
                kv.Value.End(false);
            waitDic.Clear();
        }

        /// <summary>
        /// 是否所有消息都回来了
        /// </summary>
        /// <returns></returns>
        public bool IsAllBack()
        {
            return waitDic.Count <= 0;
        }

        private TaskCompletionSource<bool> allTcs;
        /// <summary>
        /// 等待所有消息回来
        /// </summary>
        /// <returns></returns>
        public async Task<bool> WaitAllBack()
        {
            if (waitDic.Count > 0)
            {
                if (allTcs == null || allTcs.Task.IsCompleted)
                    allTcs = new TaskCompletionSource<bool>();
                await allTcs.Task;
            }
            return true;
        }

        public void DisposeAll()
        {
            if (waitDic.Count > 0)
            {
                foreach (var item in waitDic)
                    item.Value.Timer?.Dispose();
            }
        }

        public async Task<bool> StartWait(int uniId)
        {
            if (!waitDic.ContainsKey(uniId))
            {
                var waiter = new innerWaiter();
                waitDic.Add(uniId, waiter);
                waiter.Start();
                return await waiter.Tcs.Task;
            }
            else
            {
                Log.Error("发现重复消息id：" + uniId);
            }
            return true;
        }

        public void EndWait(int uniId, bool result = true)
        {
            if (!result) Log.Error("await失败：" + uniId);
            if (waitDic.ContainsKey(uniId))
            {
                var waiter = waitDic[uniId];
                waiter.End(result);
                waitDic.Remove(uniId);
                if (waitDic.Count <= 0)
                {
                    if (allTcs != null)
                    {
                        allTcs.TrySetResult(true);
                        allTcs = null;
                    }
                }
            }
            else
            {
                if (uniId > 0)
                    Log.Error("找不到EndWait：" + uniId + ">size：" + waitDic.Count);
            }
        }
    }
}
