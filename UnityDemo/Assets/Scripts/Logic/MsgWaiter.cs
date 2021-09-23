using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Geek.Client
{
    public class MsgWaiter
    {
        private static readonly Dictionary<int, MsgWaiter> waitDic = new Dictionary<int, MsgWaiter>();

        public static void Clear()
        {
            foreach (var kv in waitDic)
                kv.Value.End(false);
            waitDic.Clear();
        }

        /// <summary>
        /// 是否所有消息都回来了
        /// </summary>
        /// <returns></returns>
        public static bool IsAllBack()
        {
            return waitDic.Count <= 0;
        }

        private static TaskCompletionSource<bool> allTcs;
        /// <summary>
        /// 等待所有消息回来
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> WaitAllBack()
        {
            if (waitDic.Count > 0)
            {
                if(allTcs == null || allTcs.Task.IsCompleted)
                    allTcs = new TaskCompletionSource<bool>();
                await allTcs.Task;
            }
            return true;
        }

        public static async Task<bool> StartWait(int uniId)
        {
            if (!waitDic.ContainsKey(uniId))
            {
                var waiter = new MsgWaiter();
                waitDic.Add(uniId, waiter);
                waiter.Start();
                return await waiter.Tcs.Task;
            }
            else
            {
                Debuger.Err("发现重复消息id：" + uniId);
            }
            return true;
        }

        public static void EndWait(int uniId, bool result=true)
        {
            if(!result) Debuger.Err("await失败：" + uniId);
            if (waitDic.ContainsKey(uniId))
            {
                var waiter = waitDic[uniId];
                waiter.End(result);
                waitDic.Remove(uniId);
                if (waitDic.Count <= 0)  //所有等待的消息都回来了再解屏
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
                if(uniId > 0)
                    Debuger.Err("找不到EndWait：" + uniId + ">size：" + waitDic.Count);
            }
        }

        long timerId;
        public TaskCompletionSource<bool> Tcs { private set; get; }

        void Reset()
        {
            Tcs = new TaskCompletionSource<bool>();
        }

        private Timer timer;
        void Start()
        {
            Tcs = new TaskCompletionSource<bool>();
            timer = new Timer(TimeOut, null, 10000, -1);
        }

        void End(bool result)
        {
            timer.Dispose();
            if (Tcs != null)
                Tcs.TrySetResult(result);
            Tcs = null;
        }

        private void TimeOut(object state)
        {
            End(false);
            Debuger.Err("等待消息超时");
        }

    }
}
