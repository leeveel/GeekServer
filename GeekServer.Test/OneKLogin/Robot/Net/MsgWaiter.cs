using NLog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.Server.Test
{
    public class MsgWaiter
    {

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private Dictionary<int, MsgWaiter> waitDic = new Dictionary<int, MsgWaiter>();

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

        private static TaskCompletionSource<bool> allTcs;
        /// <summary>
        /// 等待所有消息回来
        /// </summary>
        /// <returns></returns>
        public async Task<bool> WaitAllBack()
        {
            if (waitDic.Count > 0)
            {
                if(allTcs == null || allTcs.Task.IsCompleted)
                    allTcs = new TaskCompletionSource<bool>();
                await allTcs.Task;
            }
            return true;
        }

        public async Task<bool> StartWait(int uniId)
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
               LOGGER.Error("发现重复消息id：" + uniId);
            }
            return true;
        }

        public void EndWait(int uniId, bool result=true)
        {
            if(!result) LOGGER.Error("await失败：" + uniId);
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
                    LOGGER.Error("找不到EndWait：" + uniId + ">size：" + waitDic.Count);
            }
        }

        public TaskCompletionSource<bool> Tcs { private set; get; }

        void Start()
        {
            Tcs = new TaskCompletionSource<bool>();
        }

        void End(bool result)
        {
            if (Tcs != null)
                Tcs.TrySetResult(result);
            Tcs = null;
        }

    }
}
