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
using Geek.Core.Hotfix;
using Geek.App.Login;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.Hotfix.Logic.Login
{
    public class PlayerCompAgent : ComponentAgent<PlayerComp>
    {
        ////防止playerId和服务器id等重复了 id需要添加前缀
        public async Task<PlayerState> LoadPlayerState(string playerId)
        {
            var col = Comp.GetDB().GetCollection<PlayerState>(typeof(PlayerState).FullName);
            var filter = Builders<PlayerState>.Filter.Eq("Id", playerId);
            var state = await (await col.FindAsync(filter)).FirstOrDefaultAsync();
            if (state == null)
                return new PlayerState();
            return state;
        }

        public async Task SavePlayerState(PlayerState playerState)
        {
            var col = Comp.GetDB().GetCollection<PlayerState>(typeof(PlayerState).FullName);
            var filter = Builders<PlayerState>.Filter.Eq("Id", playerState.Id);
            await col.ReplaceOneAsync(filter, playerState, new ReplaceOptions() { IsUpsert = true });
        }

        public long GetCacheRoleId(PlayerState playerState, int serverId, List<int> oldServerList)
        {
            long roleId = 0;
            if (playerState.roleMap.ContainsKey(serverId))
                roleId = playerState.roleMap[serverId];

            if (roleId <= 0)
            {
                if (oldServerList == null)
                    return roleId;

                foreach (var server in oldServerList)
                {
                    if (playerState.roleMap.ContainsKey(server))
                    {
                        roleId = playerState.roleMap[server];
                        break;
                    }
                }
            }
            return roleId;
        }
    }
}
