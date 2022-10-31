using Microsoft.AspNetCore.Components.Web.Virtualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekServer.Gateaway.Net
{
    public enum NodeType
    {
        Client = 1,
        LoginServer = 2,
        GameServer = 3
    }

    public interface INetNode
    {
        long uid { get; }
        long defaultTargetUid { get; set; }
        NodeType type { get; }
        void Write(long fromId, int msgId, byte[] data)
        {

        }

        void OnTargetNotExist()
        {

        }

        void Abort()
        {
        }
    }
}
