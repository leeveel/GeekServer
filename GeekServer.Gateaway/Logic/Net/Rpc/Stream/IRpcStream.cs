using MagicOnion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekServer.Gateaway.Net.Rpc
{
    //gate ->other
    public interface IRpcStreamClient
    {
        public void PlayerMsg2GameServer(int serverId, long connectId, byte[] data)
        {

        }
    }

    //other -> gate
    public interface IRpcStreamServer : IStreamingHub<IRpcStreamServer, IRpcStreamClient>
    {
        public void GameServerMsg2Player(int serverId, long connectId, byte[] data)
        {

        }

        public void GameServerDisconnectPlayer(int serverId, long connectId)
        {

        }
    }
}
