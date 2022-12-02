using Geek.Client;
using Geek.Client.Config;
using Geek.Server;
using Geek.Server.Proto;
using MessagePack;
using MessagePack.Resolvers;
using Resolvers;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Logic
{
    public class GameMain : MonoBehaviour
    {

        public static GameMain Singleton = null;
        public Text Txt;

        public string gateIp = "127.0.0.1";
        public int gatePort = 8899;
        public int serverId = 1001;
        public string userName = "123456";

        private void Awake()
        {
            Singleton = this;
        }

        async void Start()
        {
            Txt = GameObject.Find("Text").GetComponent<Text>();
            GameDataManager.ReloadAll();
            GameClient.Singleton.Init();
            DemoService.Singleton.RegisterEventListener();
            await ConnectServer();
            await ReqRouter();
            await Login();
            await ReqBagInfo();
            await ReqComposePet();
        }

        private async Task ConnectServer()
        {
            //这里填写你的本机的内网ip地址,不要使用127.0.0.1（有可能由于hosts设置连不上）
            _ = GameClient.Singleton.Connect(gateIp, gatePort);
            await MsgWaiter.StartWait(GameClient.ConnectEvt);
        }

        private Task ReqRouter()
        {
            var req = new ReqConnectGate();
            req.ServerId = serverId;
            return DemoService.Singleton.SendMsg(req);
        }

        private Task Login()
        {
            //登陆
            var req = new ReqLogin();
            req.SdkType = 0;
            req.SdkToken = "";
            req.UserName = userName;
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

        private Task ReqComposePet()
        {
            ReqComposePet req = new ReqComposePet();
            req.FragmentId = 1000;
            return DemoService.Singleton.SendMsg(req);
        }


        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            GameClient.Singleton.Close();
            MsgWaiter.DisposeAll();
        }


        public void AppendLog(string str)
        {
            if (Txt != null)
            {
                var temp = Txt.text + "\r\n";
                temp += str;
                Txt.text = temp;
            }
        }

    }
}

