using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Geek.Server
{
    public class ActorNode
    {
        public BaseActor Actor { get; set; }
        public ActorNode Parent { get; set; }
        public string Trace { get; set; }
    }

    /// <summary>
    /// debug模式下使用的actor，牺牲效率检测大部分死锁情况
    /// </summary>
    public class DeadlockCheckActor
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        BaseActor cacheActor;
        ActorNode activeNode;
        public void SetActiveNode(ActorNode node)
        {
            lock(nodeLock)
            {
                activeNode = node;
            }
        }
        static readonly object nodeLock = new object();
        static readonly Dictionary<int, DeadlockCheckActor> threadMap = new Dictionary<int, DeadlockCheckActor>();
        readonly ActionBlock<WorkWrapper> actionBlock;
        public DeadlockCheckActor(BaseActor actor)
        {
            cacheActor = actor;
            var scheduler = new LimitedConcurrencyLevelTaskScheduler(1);
            actionBlock = new ActionBlock<WorkWrapper>(InnerRun, new ExecutionDataflowBlockOptions() { TaskScheduler = scheduler });

            lock (nodeLock)
            {
                threadMap.Add(scheduler.Id, this);
            }
        }

        void checkNodeIlegal(ActorNode node)
        {
            if (node == null)
                return;

            string trace = "\nat " + node.Trace;
            var now = node;
            node = node.Parent;
            while (node != null)
            {
                trace += "\nat " + node.Trace;
                if (now.Actor == node.Actor)
                {
                    LOGGER.Fatal("actor dead lock:" + trace);
                    //throw new Exception(string.Format("actor dead lock:{0}", trace));
                }
                node = node.Parent;
            }
        }

        public Task SendAsync(Action work, bool checkLock, int timeOut)
        {
            ActionWrapper at = new ActionWrapper(work);
            at.TimeOut = timeOut;
            lock (nodeLock)
            {
                int thId = TaskScheduler.Current.Id;
                var newNode = new ActorNode();
                if (threadMap.ContainsKey(thId))
                    newNode.Parent = threadMap[thId].activeNode;
                newNode.Actor = cacheActor;
                if (work.Target == null || work.Method.DeclaringType.Name.Contains("<>c"))
                    newNode.Trace = work.Method.DeclaringType.FullName + ":" + work.Method.Name + "(" + work.ToString() + ")";
                else
                    newNode.Trace = work.Target.GetType().FullName + ":" + work.Method.Name + "(" + work.ToString() + ")";
                if (checkLock)
                    checkNodeIlegal(newNode);
                else
                    newNode.Parent = null;
                at.Node = newNode;
            }
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task<T> SendAsync<T>(Func<T> work, bool checkLock, int timeOut)
        {
            FuncWrapper<T> at = new FuncWrapper<T>(work);
            at.TimeOut = timeOut;
            lock (nodeLock)
            {
                int thId = TaskScheduler.Current.Id;
                var newNode = new ActorNode();
                if (threadMap.ContainsKey(thId))
                    newNode.Parent = threadMap[thId].activeNode;
                newNode.Actor = cacheActor;
                if (work.Target == null || work.Method.DeclaringType.Name.Contains("<>c"))
                    newNode.Trace = work.Method.DeclaringType.FullName + ":" + work.Method.Name + "(" + work.ToString() + ")";
                else
                    newNode.Trace = work.Target.GetType().FullName + ":" + work.Method.Name + "(" + work.ToString() + ")";
                if (checkLock)
                    checkNodeIlegal(newNode);
                else
                    newNode.Parent = null;
                at.Node = newNode;
            }

            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task SendAsync(Func<Task> work, bool checkLock, int timeOut)
        {
            ActionAsyncWrapper at = new ActionAsyncWrapper(work);
            at.TimeOut = timeOut;
            lock (nodeLock)
            {
                int thId = TaskScheduler.Current.Id;
                var newNode = new ActorNode();
                if (threadMap.ContainsKey(thId))
                    newNode.Parent = threadMap[thId].activeNode;
                newNode.Actor = cacheActor;
                if (work.Target == null || work.Method.DeclaringType.Name.Contains("<>c"))
                    newNode.Trace = work.Method.DeclaringType.FullName + ":" + work.Method.Name + "(" + work.ToString() + ")";
                else
                    newNode.Trace = work.Target.GetType().FullName + ":" + work.Method.Name + "(" + work.ToString() + ")";
                if (checkLock)
                    checkNodeIlegal(newNode);
                else
                    newNode.Parent = null;
                at.Node = newNode;
            }
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task<T> SendAsync<T>(Func<Task<T>> work, bool checkLock, int timeOut)
        {
            FuncAsyncWrapper<T> at = new FuncAsyncWrapper<T>(work);
            at.TimeOut = timeOut;
            lock (nodeLock)
            {
                int thId = TaskScheduler.Current.Id;
                var newNode = new ActorNode();
                if (threadMap.ContainsKey(thId))
                    newNode.Parent = threadMap[thId].activeNode;
                newNode.Actor = cacheActor;
                if (work.Target == null || work.Method.DeclaringType.Name.Contains("<>c"))
                    newNode.Trace = work.Method.DeclaringType.FullName + ":" + work.Method.Name + "(" + work.ToString() + ")";
                else
                    newNode.Trace = work.Target.GetType().FullName + ":" + work.Method.Name + "(" + work.ToString() + ")";
                if (checkLock)
                    checkNodeIlegal(newNode);
                else
                    newNode.Parent = null;
                at.Node = newNode;
            }
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task<bool> Deactive()
        {
            lock (nodeLock)
            {
                if (threadMap.ContainsKey(TaskScheduler.Current.Id))
                    threadMap.Remove(TaskScheduler.Current.Id);
            }
            return Task.FromResult(true);
        }

        async Task InnerRun(WorkWrapper wrapper)
        {
            var task = wrapper.DoTask();
            var res = await task.WaitAsync(TimeSpan.FromMilliseconds(wrapper.TimeOut));
            if (res)
            {
                LOGGER.Fatal("wrapper执行超时:" + wrapper.GetTrace());
                wrapper.ForceSetResult();
            }
        }
    }
}