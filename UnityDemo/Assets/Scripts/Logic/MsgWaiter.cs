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
                if (allTcs == null || allTcs.Task.IsCompleted)
                    allTcs = new TaskCompletionSource<bool>();
                await allTcs.Task;
            }
            return true;
        }

        public static void DisposeAll()
        {
            if (waitDic.Count > 0)
            {
                foreach (var item in waitDic)
                    item.Value.Timer?.Dispose();
            }
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
                UnityEngine.Debug.LogError("发现重复消息id：" + uniId);
            }
            return true;
        }

        public static void EndWait(int uniId, bool result = true)
        {
            UnityEngine.Debug.Log("结束等待消息:" + uniId);
            if (!result) UnityEngine.Debug.LogError("await失败：" + uniId);
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
                if (uniId > 0)
                    UnityEngine.Debug.LogError("找不到EndWait：" + uniId + ">size：" + waitDic.Count);
            }
        }

        public TaskCompletionSource<bool> Tcs { private set; get; }


        public Timer Timer { private set; get; }
        void Start()
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

        private void TimeOut(object state)
        {
            End(false);
            UnityEngine.Debug.LogError("等待消息超时");
        }

    }
}
