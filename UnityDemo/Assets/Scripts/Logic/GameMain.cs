using Geek.Client;
using Geek.Client.Message.Bag;
using Geek.Client.Message.Login;
using System.Threading.Tasks;
using UnityEngine;

namespace Logic
{
    public class GameMain : MonoBehaviour
    {
        async void Start()
        {
            DemoService.Singleton.RegisterEventListener();
            await ConnectServer();
            await Login();
            await ReqBagInfo();
        }

        private async Task ConnectServer()
        {
            //这里填写你的本机的内网ip地址,不要使用127.0.0.1（有可能由于hosts设置连不上）
            MessageHandle.GetInstance().BeginConnect("192.168.0.163", 10000);
            await MsgWaiter.StartWait(MessageHandle.ConnectSucceedEvt);
        }

        private Task Login()
        {
            //登陆
            var req = new ReqLogin();
            req.sdkType = 0;
            req.sdkToken = "";
            req.userName = "123456";
            req.device = SystemInfo.deviceUniqueIdentifier;
            if (Application.platform == RuntimePlatform.Android)
                req.platform = "android";
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                req.platform = "ios";
            else
                req.platform = "unity";
            return DemoService.Singleton.SendMsg(req);
        }

        private Task ReqBagInfo()
        {
            ReqBagInfo req = new ReqBagInfo();
            return DemoService.Singleton.SendMsg(req);
        }

        void Update()
        {
            MessageHandle.GetInstance().Update(GED.NED);
        }

    }
}

