using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using Debug = UnityEngine.Debug;

public class MsgWaiter
{
    private static Dictionary<int, MsgWaiter> waitDic = new Dictionary<int, MsgWaiter>();
    public static void Clear()
    {
        List<int> Keys = new List<int>(waitDic.Keys);
        foreach (var key in Keys)
        {
            if (waitDic.ContainsKey(key))
                waitDic[key].End(false);
        }
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

    public static async Task<bool> StartWait(int uniId, float timeOut = 15, string msg = "")
    {
        if (!waitDic.ContainsKey(uniId))
        {
            //ConnectingWindow.Singleton.Show();
            var waiter = new MsgWaiter(uniId, msg);
            waitDic.Add(uniId, waiter);
            waiter.Start(uniId, timeOut);
            return await waiter.Tcs.Task;
        }
        else
        {
            Debug.LogError("发现重复消息id：" + uniId);
        }
        return true;
    }

    public static void EndWait(int uniId, bool result = true)
    {
        var msg = "";
        if (waitDic.ContainsKey(uniId))
        {
            var waiter = waitDic[uniId];
            msg = waiter.msg;
            if (!result) Debug.LogError($"await失败：{uniId}  {msg}");
            waiter.End(result);
            if (waitDic.Count <= 0)  //所有等待的消息都回来了再解屏
            {
                if (allTcs != null)
                {
                    allTcs.TrySetResult(true);
                    allTcs = null;
                }
                //ConnectingWindow.Singleton.Hide();
            }
        }
        else
        {
            if (!result) Debug.LogError("await失败：" + uniId);
            if (uniId > 0)
                Debug.LogError("找不到EndWait：" + uniId + ">size：" + waitDic.Count);
        }
    }

    public MsgWaiter(int uid, string msg)
    {
        this.msg = msg;
        uniId = uid;
    }

    string msg;
    int uniId;
    long timerId;
    public TaskCompletionSource<bool> Tcs { private set; get; }

    void Reset()
    {
        Tcs = new TaskCompletionSource<bool>();
    }

    void Start(int uniId, float timeout = 15)
    {
        Tcs = new TaskCompletionSource<bool>();
        timerId = CoroutineManager.Singleton.delayedCall(timeout, () => { timeOut(uniId); });
    }

    void End(bool result)
    {
        CoroutineManager.Singleton.stopCoroutine(timerId);
        if (waitDic.ContainsKey(uniId))
            waitDic.Remove(uniId);
        if (Tcs != null)
            Tcs.TrySetResult(result);
        Tcs = null;
    }

    void timeOut(int uniId)
    {
        End(false);
        Debug.LogError("服务器未回复消息uniId:" + uniId + " " + msg);
    }
}