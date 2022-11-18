using NLog;
using System.Collections.Concurrent;

namespace Geek.Server
{

    /// <summary>
    /// 判断Actor交叉死锁
    /// </summary>
    internal static class ActorLimit
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        internal static readonly ConcurrentDictionary<long, ConcurrentDictionary<long, bool>> CrossDic = new();

        private static bool AllowCall(long self, long target)
        {
            // 自己入自己的队允许，会直接执行
            if (self == target)
                return true;
            if (CrossDic.TryGetValue(target, out var set) && set.ContainsKey(self))
            {
                Log.Error($"发生交叉死锁，ActorId1:{self} ActorType1:{IdGenerator.GetActorType(self)} ActorId2:{target} ActorType2:{IdGenerator.GetActorType(target)}");
                return false;
            }

            var selfSet = CrossDic.GetOrAdd(self, k => new());
            selfSet.TryAdd(target, false);

            return true;
        }

        internal static bool AllowCall(long target)
        {
            var actorId = RuntimeContext.CurActor;
            // 从IO线程抛到actor，不涉及入队行为
            if (actorId == 0)
                return true;
            // Actor会在入队成功之后进行设置，这种属于Actor入队
            return AllowCall(actorId, target);
        }
    }
}
