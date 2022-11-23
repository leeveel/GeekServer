
using Geek.Server.App.Common.Event;
using Geek.Server.App.Logic.Role.Bag;
using Geek.Server.Core.Hotfix.Agent;
using Geek.Server.Core.Utils;
using Server.Logic.Common.Events;
using Server.Logic.Logic.Role.Base;

namespace Server.Logic.Logic.Role.Bag
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

        public async Task GetBagInfo(ReqBagInfo reqMsg)
        {
            var ret = BuildInfoMsg();
            await this.NotifyClient(ret, reqMsg.UniId);
        }

        /// <summary>
        /// 宠物合成
        /// </summary>
        /// <returns></returns>
        public async Task ComposePet(ReqComposePet reqMsg)
        {
            //宠物碎片合成相关逻辑
            //.....
            //.....

            //合成成功后分发一个获得宠物的事件(在PetCompAgent中监听此事件)
            this.Dispatch(EventID.GotNewPet, new OneParam<int>(1000));

            var res = new ResComposePet();
            res.PetId = 1000;
            await this.NotifyClient(res, reqMsg.UniId);
        }



    }
}
