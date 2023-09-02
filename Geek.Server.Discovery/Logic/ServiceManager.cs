namespace Geek.Server.Discovery.Logic
{
    internal class ServiceManager
    {
        public static NamingService NamingService { get; set; } = new NamingService();
        public static SubscribeService SubscribeService { get; set; } = new SubscribeService();
    }
}
