using Geek.Server.Logic.Handler;
using Geek.Server.Message.Bag;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Geek.Server.Logic.Bag
{
    [TcpMsgMapping(typeof(ReqBagInfo))]
    public class ReqBagInfoHandler : RoleActorHandler
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public override async Task ActionAsync()
        {
            var bagComp = await Actor.GetCompAgent<BagCompAgent>();
            var msg = await bagComp.BuildInfoMsg();
            WriteAndFlush(MSG.Create(msg));
            //int count = 1000000;
            //var sw = new Stopwatch();
            //sw.Reset();
            //sw.Start();
            //for (int i = 0; i < count; i++)
            //{
            //    await bagComp.Test1();
            //}
            //var enqueueTime = sw.ElapsedMilliseconds;

            //sw.Reset();
            //sw.Start();
            //for (int i = 0; i < count; i++)
            //{
            //    await bagComp.Test2();
            //}
            //var noqueueTime = sw.ElapsedMilliseconds;

            //LOGGER.Info($"enqueueTime:{enqueueTime}--noqueueTime:{noqueueTime}");
        }
    }
}
