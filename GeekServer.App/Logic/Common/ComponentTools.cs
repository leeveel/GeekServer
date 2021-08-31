using Geek.Server.Logic.Login;
using Geek.Server.Logic.Role;
using Geek.Server.Logic.Server;

namespace Geek.Server
{
    public class ComponentTools
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public static void RegistAllComps()
        {
            //注册组件,所有actor的所有组件

            //server
            RegistAutoActiveActor(ActorType.Server);
            RegistComp<ServerComp>(ActorType.Server);

            //login
            RegistAutoActiveActor(ActorType.Login);
            RegistComp<LoginComp>(ActorType.Login);

            //role
            RegistComp<RoleComp>(ActorType.Role);
            RegistComp<RoleLoginComp>(ActorType.Role);
        }





        /// <summary>
        /// 注册组件
        /// </summary>
        /// <typeparam name="TComp"></typeparam>
        /// <param name="actorType">Actor类型</param>
        /// <param name="autoActive">Actor激活的时候自动激活该组件[激活顺序等同于注册顺序],同时会被加入到常驻内存列表</param>
        static void RegistComp<TComp>(ActorType actorType, bool autoActive=false) where TComp : BaseComponent, new()
        {
            ComponentMgr.Singleton.RegistComp<TComp>((int)actorType, autoActive);
        }

        /// <summary>
        /// 注册起服就需要激活的Actor
        /// </summary>
        /// <param name="actorType"></param>
        static void RegistAutoActiveActor(ActorType actorType)
        {
            if (actorType > ActorType.Separator)
                ComponentMgr.Singleton.RegistAutoActiveActor((int)actorType);
            else
                LOGGER.Error($"只能注册全服单例的Actor类型:{actorType}");
        }

    }
}