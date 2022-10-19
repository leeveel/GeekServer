using Geek.Server.Role;

namespace Geek.Server.Login
{
    public class LoginCompAgent : BaseCompAgent<LoginComp>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public override void Active()
        {
            base.Active();
            Tell(Init);
            StateComp.AddShutdownSaveFunc(SaveAll);
        }

        private async Task Init()
        {
            var col = MongoDBConnection.CurDB.GetCollection<PlayerInfo>();
            using var cursor = await col.FindAsync(Builders<PlayerInfo>.Filter.Empty);
            await cursor.ForEachAsync(t =>
            {
                Comp.PlayerMap[t.playerId] = t;
                //Log.Info("加载数据:" + t.playerId);
            });
        }

        private async Task SaveAll(bool shutdown)
        {
            if (shutdown)
            {
                await SaveLogic(shutdown);
            }
            else
            {
                Tell(() => SaveLogic(shutdown));
            }
        }

        private async Task SaveLogic(bool shutdown)
        {
            var writeList = new List<ReplaceOneModel<PlayerInfo>>();
            foreach (var registInfo in Comp.PlayerMap.Values)
            {
                if (registInfo.IsChanged)
                {
                    var filter = Builders<PlayerInfo>.Filter.Eq(nameof(PlayerInfo.playerId), registInfo.playerId);
                    var model = new ReplaceOneModel<PlayerInfo>(filter, registInfo) { IsUpsert = true };
                    writeList.Add(model);
                }
            }

            if (writeList.Count > 0)
            {
                var col = MongoDBConnection.CurDB.GetCollection<PlayerInfo>();
                var result = await col.BulkWriteAsync(writeList, new BulkWriteOptions { IsOrdered = false });
                if (!shutdown && result.IsAcknowledged)
                {
                    foreach (var model in writeList)
                    {
                        model.Replacement.IsChanged = false;
                    }
                }
            }
        }

        [AsyncApi]
        public virtual async Task OnLogin(NetChannel channel, ReqLogin reqLogin)
        {
            if (string.IsNullOrEmpty(reqLogin.UserName))
            {
                channel.WriteAsync(null, reqLogin.UniId, StateCode.AccountCannotBeNull);
            }

            if (reqLogin.Platform != "android" && reqLogin.Platform != "ios" && reqLogin.Platform != "unity")
            {
                //验证平台合法性
                channel.WriteAsync(null, reqLogin.UniId, StateCode.UnknownPlatform);
            }

            //查询角色账号，这里设定每个服务器只能有一个角色
            var roleId = GetRoleIdOfPlayer(reqLogin.UserName, reqLogin.SdkType);
            var isNewRole = roleId <= 0;
            if (isNewRole)
            {
                //没有老角色，创建新号
                roleId = IdGenerator.GetActorID(ActorType.Role);
                CreateRoleToPlayer(reqLogin.UserName, reqLogin.SdkType, roleId);
                Log.Info("创建新号:" + roleId);
            }

            //添加到session
            var session = new Session
            {
                Id = roleId,
                Time = DateTime.Now,
                Channel = channel,
                Sign = reqLogin.Device
            };
            var oldSession = HotfixMgr.SessionMgr.Add(session);

            if (oldSession != null)
            {
                //通知老链接下线
                _ = Task.Run(async () =>
                {
                    //send msg...
                    await Task.Delay(100);
                    oldSession.Channel?.Abort();
                });
            }
            //登陆流程
            var roleComp = await ActorMgr.GetCompAgent<RoleCompAgent>(roleId);
            //从登录线程-->调用Role线程 所以需要入队
            var resLogin = await roleComp.SendAsync(() => roleComp.OnLogin(reqLogin, isNewRole));
            channel.WriteAsync(resLogin, reqLogin.UniId, StateCode.Success);
        }

        private long GetRoleIdOfPlayer(string userName, int sdkType)
        {
            var playerId = $"{sdkType}_{userName}";
            if (Comp.PlayerMap.TryGetValue(playerId, out var state))
            {
                if (state.RoleMap.TryGetValue(Settings.ServerId, out var roleId))
                    return roleId;
                return 0;
            }
            return 0;
        }

        private void CreateRoleToPlayer(string userName, int sdkType, long roleId)
        {
            var playerId = $"{sdkType}_{userName}";
            Comp.PlayerMap.TryGetValue(playerId, out var info);
            if (info == null)
            {
                info = new PlayerInfo();
                info.playerId = playerId;
                info.SdkType = sdkType;
                info.UserName = userName;
                Comp.PlayerMap[playerId] = info;
            }
            info.IsChanged = true;
            info.RoleMap[Settings.ServerId] = roleId;
        }

    }
}