
namespace Geek.Server.Role
{
    public class BagCompAgent : StateCompAgent<BagComp, BagState>
    {
        readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public override void Active()
        {
            if (State.ItemMap.Count <= 0)
            {
                State.ItemMap.Add(101, 1);
                State.ItemMap.Add(103, 100);
            }
        }

        private ResBagInfo BuildInfoMsg()
        {
            var res = new ResBagInfo();
            foreach (var kv in State.ItemMap)
                res.ItemDic[kv.Key] = kv.Value;
            return res;
        }

        [AsyncApi]
        public virtual async Task GetBagInfo(ReqBagInfo msg)
        {
            var ret = BuildInfoMsg();
            await this.NotifyClient(ret, msg.UniId);
        }

    }
}
