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
            GameClient.Singleton.Init();
            DemoService.Singleton.RegisterEventListener();
            await ConnectServer();
            await Login();

            for (int i = 0; i < 1; i++)
            {
                await ReqBagInfo();
                await Task.Delay(1000);
            }
        }

        private async Task ConnectServer()
        {
            //这里填写你的本机的内网ip地址,不要使用127.0.0.1（有可能由于hosts设置连不上）
            _ = GameClient.Singleton.Connect("192.168.0.163", 10000);
            await MsgWaiter.StartWait(GameClient.ConnectEvt);
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



        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            GameClient.Singleton.Close();
        }

    }
}

