using Amazon.Runtime.Internal.Transform;
using Geek.Server.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Server.Core.Net.Kcp
{
    public static class NetPackageFlag
    {
        public const byte SYN = 1;
        public const byte SYN_OLD_NET_ID = 2;
        public const byte ACK = 3;
        public const byte HEART = 4;
        public const byte NO_GATE_CONNECT = 5;
        public const byte CLOSE = 6;
        public const byte NO_INNER_SERVER = 7;
        public const byte MSG = 8;

        public static string GetFlagDesc(byte flag)
        {
            return flag switch
            {
                SYN => "连接请求",
                SYN_OLD_NET_ID => "带NetId的连接请求",
                ACK => "连接应答",
                HEART => "心跳",
                NO_GATE_CONNECT => "无网关连接",
                CLOSE => "关闭",
                MSG => "消息",
                NO_INNER_SERVER => "无内部服务器",
                _ => "无效标记:" + flag,
            };
        }
    }

    public ref struct TempNetPackage
    {
        public const int headLen = 13;
        public bool isOk;
        public byte flag;
        public long netId;
        public int innerServerId;
        public ReadOnlySpan<byte> body;

        public TempNetPackage(byte flag, long netId, int targetServerId = 0)
        {
            isOk = true;
            this.flag = flag;
            this.netId = netId;
            innerServerId = targetServerId;
        }

        public TempNetPackage(byte flag, long netId, int targetServerId, ReadOnlySpan<byte> data)
        {
            isOk = true;
            this.flag = flag;
            this.netId = netId;
            innerServerId = targetServerId;
            body = data;
        }

        public TempNetPackage(Span<byte> data)
        {
            if (data.Length < headLen)
            {
                isOk = false;
                return;
            }
            isOk = true;
            flag = data[0];
            netId = data.ReadLong(1);
            innerServerId = data.ReadInt(9);
            body = data[headLen..];
        }

        public int Length
        {
            get => headLen + body.Length;
        }

        public override string ToString()
        {
            return $"{{flag:{NetPackageFlag.GetFlagDesc(flag)},netId:{netId},innerServerId:{innerServerId},bodyLen:{body.Length}}}";
        }
    }
}
