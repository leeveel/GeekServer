using Geek.Server.Logic.Bag;
using Geek.Server.Logic.Login;
using Geek.Server.Logic.Role;
using Geek.Server.Logic.Server;

namespace Geek.Server
{
    public class ComponentTools
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public static void RegistAll()
        {
            //注册组件,所有actor的所有组件

            //server
            RegistServerComp<ServerComp>(EntityType.Server);

            //login
            RegistServerComp<LoginComp>(EntityType.Login);

            //role
            RegistRoleComp<RoleComp>();
            RegistRoleComp<RoleLoginComp>();
            RegistRoleComp<BagComp>();

            //设置实体共享actor (默认为true)
            //CompSetting.Singleton.SetIfEntityCompShareActor((int)EntityType.Role, true);
        }


        static void RegistRoleComp<TComp>() where TComp : BaseComponent, new()
        {
            CompSetting.Singleton.RegistComp<TComp>((int)EntityType.Role, false);
        }

        static void RegistGuildComp<TComp>() where TComp : BaseComponent, new()
        {
            CompSetting.Singleton.RegistComp<TComp>((int)EntityType.Guild, false);
        }

        static void RegistServerComp<TComp>(EntityType type) where TComp : BaseComponent, new()
        {
            CompSetting.Singleton.RegistComp<TComp>((int)type, true);
        }


    }
}