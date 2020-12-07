/****************************************************************************
Copyright (c) Geek

https://github.com/leeveel/GeekServer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

public enum ServerType
{
    None = 0,
    Logic = 1,
    Center,
}

public class Settings
{
    public static readonly bool XBuffer_Netty = true;
    /// <summary>是否正常运行中(除开起服/关服准备)</summary>
    public volatile bool AppRunning;
    /// <summary>起服时间</summary>
    public DateTime StartServerTime;
    ///<summary>备份时间间隔(分钟 小于等于0则不备份)</summary>
    public int backupSpan;
    /// <summary>备份保留时间(小时)</summary>
    public int backupRemainTime;
    /// <summary>
    /// 回档时间
    /// 走<see cref="Geek.Core.Storage.MongoDBConnection.LoadState"/>才能回档
    /// </summary>
    public DateTime restoreToTime;
    
    /// <summary> 开发模式 </summary>
    public bool isDebug;
    /// <summary>数据回存最大时间(秒)</summary>
    public int dataFlushTimeMax;
    /// <summary>数据回存最小时间(秒)</summary>
    public int dataFlushTimeMin;
    /// <summary>数据每秒操作上限</summary>
    public int dataFPS;

    /// <summary>组件自动回收时间(分钟)</summary>
    public int compRecycleTime;
    /// <summary>actor自动回收时间(分钟)</summary>
    public int actorRecycleTime;

    /// <summary>最大等待登陆人数</summary>
    public int loginQueueNum;
    /// <summary> 服务器id 从10000开始</summary>
    public int serverId;
    /// <summary> http端口 </summary>
    public int httpPort;
    /// <summary> tcp端口  </summary>
    public int tcpPort;
    /// <summary> 使用libuv </summary>
    public bool useLibuv;

    /// <summary> http外部命令验证 </summary>
    public string httpCode;
    /// <summary> http指令路径 </summary>
    public string httpUrl;
    /// <summary> 服务器使用语言 </summary>
    public string language;
    /// <summary> mongoDB登陆路径 </summary>
    public string mongoDB;
    /// <summary> mongoDB登陆路径 </summary>
    public string mongoUrl;

    public ServerType ServerType { get; private set; }
    public void SetServerType(ServerType serverType)
    {
        ServerType = serverType;
    }


    static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
    public static Settings Ins { get; private set; }
    public static void Load(string configFilePath)
    {
        Ins = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(configFilePath));
#if DEBUG
        if (Ins.isDebug)
        {
            string localIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = _IPAddress.ToString();
                    break;
                }
            }
            if (!string.IsNullOrEmpty(localIP))
            {
                int oldServerId = Ins.serverId;
                var arr = localIP.Split('.');
                Ins.serverId = int.Parse(arr[arr.Length - 1]) + Ins.tcpPort;//ip+端口
                LOGGER.Warn(string.Format("debug mode change serverId>{0}\nip:{1},port:{2},oldServerId:{3}", Settings.Ins.serverId, localIP, Ins.tcpPort, oldServerId));
            }
        }
#endif
    }
}
