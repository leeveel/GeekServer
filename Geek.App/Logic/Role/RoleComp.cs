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
using Geek.Core.Component;
using Geek.Core.Storage;

namespace Geek.App.Role
{
    public class LoginInfo
    {
        public bool isReconnect;//是否是重连

        public long roleId;
        public int serverId;
        public string playerId;
        public bool isNewCreate;
        public int openServerDay;
        public string newName;
        public bool IsGMRole;

        public int sdkType;
        public string sdkChannel;
    }

    public class RoleState : CacheState
    {
        public string Name;
        public long RoleId;
        public int OrgServerId;
        public int ServerId;
        public string PlayerId;
        public int CacheCrossDay;

        public int Level;
        public int VipLevel;
        public long GuildId;
        public string GuildName;

        public long CreateTime;
        public long OnlineTime;
        public long OfflineTime;

        public bool IsGMRole;

        public long ChatBlockTime;//禁言截止时间
        public long LoginBlockTime;//封号截止时间
    }

    public class RoleComp : StateComponent<RoleState>
    {

    }
}
