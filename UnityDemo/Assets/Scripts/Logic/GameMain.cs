using Geek.Client;
using Geek.Server.Proto;
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
            req.SdkType = 0;
            req.SdkToken = "";
            req.UserName = "123456";
            req.Device = SystemInfo.deviceUniqueIdentifier;
            if (Application.platform == RuntimePlatform.Android)
                req.Platform = "android";
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                req.Platform = "ios";
            else
                req.Platform = "unity";
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

