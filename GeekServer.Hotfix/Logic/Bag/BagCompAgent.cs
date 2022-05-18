using Geek.Server.Proto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.Server.Logic.Bag
{
    public class BagCompAgent : StateComponentAgent<BagComp, BagState>
    {
        public virtual Task Init()
        {
            //赠送默认道具
            if (State.ItemMap.Count <= 0)
            {
                State.ItemMap.Add(101, 1);
                State.ItemMap.Add(103, 100);
            }
            return Task.CompletedTask;
        }

        public virtual Task<long> GetItemNum(int itemId)
        {
            long num = 0;
            if (State.ItemMap.ContainsKey(itemId))
                num = State.ItemMap[itemId];
            return Task.FromResult(num);
        }

        public virtual Task AddItem(int itemId, long num)
        {
            if (State.ItemMap.ContainsKey(itemId))
                State.ItemMap[itemId] += num;
            else
                State.ItemMap[itemId] = num;
            return Task.CompletedTask;
        }

        public virtual Task CutItem(int itemId, long num)
        {
            if (State.ItemMap.ContainsKey(itemId))
            {
                State.ItemMap[itemId] -= num;
                if (State.ItemMap[itemId] <= 0)
                    State.ItemMap.Remove(itemId);
            }
            return Task.CompletedTask;
        }

        public virtual Task<ResBagInfo> BuildInfoMsg()
        {
            var res = new ResBagInfo();
            foreach (var kv in State.ItemMap)
                res.ItemDic[kv.Key] = kv.Value;
            return Task.FromResult(res);
        }

        public virtual Task Test1()
        {
            return Task.CompletedTask;
        }

        [MethodOption.ThreadSafe]
        public virtual Task Test2()
        {
            System.Console.WriteLine("Test2 be Called");
            Test4();
            Test3(1, 2, 3, 4, "");
            return Task.CompletedTask;
        }

        public virtual Task Test3(int a, int b, int c, int d, string e)
        {
            return Task.CompletedTask;
        }


        [MethodOption.ThreadSafe]
        [MethodOption.NotAwait]
        public virtual Task Test4()
        {
            System.Console.WriteLine("Test412 be Called");
            return Task.CompletedTask;
        }


        [MethodOption.ExecuteTime(-1)]
        public virtual Task Test5(int a, List<int> list)
        {
            return Task.CompletedTask;
        }

        public virtual Task Test6<T, K>(int a, List<int> list) where T : class, new() where K : BagState
        {
            return Task.CompletedTask;
        }

    }


    class fff : BagCompAgent
    {
        public override Task Test6<T, K>(int a, List<int> list)
        {
            _ = base.Test6<T, K>(a, list);
            return Task.CompletedTask;
        }
    }

}
