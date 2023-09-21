using Base.Net;
using ClientProto;
using Geek.Client;
using Geek.Client.Config;
using Geek.Server;
using Geek.Server.Proto;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Logic
{
    public class GameMain : MonoBehaviour
    {

        public static GameMain Singleton = null;
        public Text Txt;

        public string serverIp = "127.0.0.1";
        public int serverPort = 8899;
        public string userName = "123456";

        private void Awake()
        {
            Singleton = this;
        }

        async void Start()
        {
            Txt = GameObject.Find("Text").GetComponent<Text>();

            Application.logMessageReceived += (con, stackTrace, type) =>
            {
                GameMain.Singleton.AppendLog(con);
            };

            //GameDataManager.ReloadAll(); 
            DemoService.Singleton.RegisterEventListener();
            if (!await ConnectServer())
            {
                return;
            }
            await Login();
            await ReqBagInfo();
            await ReqComposePet();
        }

        private async Task<bool> ConnectServer()
        {
            return await GameClient.Singleton.Connect(serverIp, serverPort);
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

        void Update()
        {
            GameClient.Singleton.Update(GED.NED);
        }

    }
}

