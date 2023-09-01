using Base.Net;
using Geek.Client;
using Geek.Server.Proto;
using System.Collections.Generic;
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
        public int gatePort = 9102;
        public int serverId = 1001;
        public string userName = "123456";

        private void Awake()
        {
            Singleton = this;
        }

        private void OnGetLogMessage(string logString, string stackTrace, LogType type)
        {
            AppendLog(logString);
        }

        async void Start()
        {
            Txt = GameObject.Find("Text").GetComponent<Text>();
            Application.logMessageReceived += OnGetLogMessage;
            //GameDataManager.ReloadAll(); 
            DemoService.Singleton.RegisterEventListener();
            if (await ConnectServer())
            {
                Debug.Log("连接服务器成功...");
            }
            else
            {
                Debug.Log("连接服务器失败...");
                return;
            }
            await Login();
            await ReqBagInfo();
            await ReqComposePet();
        }

        private async Task<bool> ConnectServer()
        {
            GameClient.Singleton.Connect(new GateList { serverIps = new List<string> { "127.0.0.1" }, ports = new List<int> { 7899 } }, 1001);
            return await MsgWaiter.StartWait(NetConnectMessage.MsgID);
        }

        private Task Login()
        {
            //登陆
            var req = new ReqLogin();
            req.SdkType = 0;
            req.SdkToken = "";
            req.UserName = userName;
            req.Sign = SystemInfo.deviceUniqueIdentifier;
            req.Device = SystemInfo.deviceName;
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
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                _ = GameClient.Singleton.CheckNetAsync();
            }
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

