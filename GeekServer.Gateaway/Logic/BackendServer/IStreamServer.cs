using MagicOnion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekServer.Gateaway.BackendServer
{
    //gate ->other (里面接口由客户端实现，服务器调用)
    public interface IStreamClient
    {
        public void PlayerMsg2GameServer(long uid, byte[] data)
        {

        }
    }

    //other -> gate(里面接口由服务器实现，客户端调用)
    public interface IStreamServer : IStreamingHub<IStreamServer, IStreamClient>
    {
        public void GameServerSetInfo(long serverId, int type)
        {

        }
        public void GameServerMsg2Player(int serverId, long uid, byte[] data)
        {

        }

        public void GameServerDisconnectPlayer(int serverId, long uid)
        {

        }
    }
}
