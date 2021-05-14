namespace Geek.Server
{
    public class ComponentTools
    {
        public static void RegistAllComps()
        {
            //注册组件,所有actor的所有组件
            ComponentMgr.Singleton.RegistComp<ServerActor, ServerComp>();
        }
    }
}