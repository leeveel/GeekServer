/****************************************************************************
Copyright (c) Geek

https://github.com/leeveel/GeekServer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Geek.Core.Actor
{
    public class ActorNode
    {
        public BaseActor actor;
        public ActorNode parent;
        public string trace;
    }

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
        static Dictionary<int, DeadlockCheckActor> threadMap = new Dictionary<int, DeadlockCheckActor>();
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

            string trace = "\nat " + node.trace;
            var now = node;
            node = node.parent;
            while (node != null)
            {
                trace += "\nat " + node.trace;
                if (now.actor == node.actor)
                {
                    LOGGER.Fatal("actor dead lock:" + trace);
                    //throw new Exception(string.Format("actor dead lock:{0}", trace));
                }
                node = node.parent;
            }
        }

        public Task Enqueue(Action work)
        {
            ActionWrapper at = new ActionWrapper(work);
            lock (nodeLock)
            {
                int thId = TaskScheduler.Current.Id;
                var newNode = new ActorNode();
                if (threadMap.ContainsKey(thId))
                    newNode.parent = threadMap[thId].activeNode;
                newNode.actor = cacheActor;
                if (work.Target == null || work.Method.DeclaringType.Name.Contains("<>c"))
                    newNode.trace = work.Method.DeclaringType.FullName + ":" + work.Method.Name + "(" + work.ToString() + ")";
                else
                    newNode.trace = work.Target.GetType().FullName + ":" + work.Method.Name + "(" + work.ToString() + ")";
                checkNodeIlegal(newNode);
                at.Node = newNode;
            }
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task<T> Enqueue<T>(Func<T> work)
        {
            FuncWrapper<T> at = new FuncWrapper<T>(work);
            lock (nodeLock)
            {
                int thId = TaskScheduler.Current.Id;
                var newNode = new ActorNode();
                if (threadMap.ContainsKey(thId))
                    newNode.parent = threadMap[thId].activeNode;
                newNode.actor = cacheActor;
                if (work.Target == null || work.Method.DeclaringType.Name.Contains("<>c"))
                    newNode.trace = work.Method.DeclaringType.FullName + ":" + work.Method.Name + "(" + work.ToString() + ")";
                else
                    newNode.trace = work.Target.GetType().FullName + ":" + work.Method.Name + "(" + work.ToString() + ")";
                checkNodeIlegal(newNode);
                at.Node = newNode;
            }

            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task Enqueue(Func<Task> work)
        {
            ActionAsyncWrapper at = new ActionAsyncWrapper(work);
            lock (nodeLock)
            {
                int thId = TaskScheduler.Current.Id;
                var newNode = new ActorNode();
                if (threadMap.ContainsKey(thId))
                    newNode.parent = threadMap[thId].activeNode;
                newNode.actor = cacheActor;
                if (work.Target == null || work.Method.DeclaringType.Name.Contains("<>c"))
                    newNode.trace = work.Method.DeclaringType.FullName + ":" + work.Method.Name + "(" + work.ToString() + ")";
                else
                    newNode.trace = work.Target.GetType().FullName + ":" + work.Method.Name + "(" + work.ToString() + ")";
                checkNodeIlegal(newNode);
                at.Node = newNode;
            }
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task<T> Enqueue<T>(Func<Task<T>> work)
        {
            FuncAsyncWrapper<T> at = new FuncAsyncWrapper<T>(work);
            lock (nodeLock)
            {
                int thId = TaskScheduler.Current.Id;
                var newNode = new ActorNode();
                if (threadMap.ContainsKey(thId))
                    newNode.parent = threadMap[thId].activeNode;
                newNode.actor = cacheActor;
                if (work.Target == null || work.Method.DeclaringType.Name.Contains("<>c"))
                    newNode.trace = work.Method.DeclaringType.FullName + ":" + work.Method.Name + "(" + work.ToString() + ")";
                else
                    newNode.trace = work.Target.GetType().FullName + ":" + work.Method.Name + "(" + work.ToString() + ")";
                checkNodeIlegal(newNode);
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

        Task InnerRun(WorkWrapper wrapper)
        {
            return wrapper.DoTask();
        }
    }
}