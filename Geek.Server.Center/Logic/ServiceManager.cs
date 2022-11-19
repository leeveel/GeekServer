namespace Geek.Server.Center.Logic
{
    internal class ServiceManager
    {
        public static NamingService NamingService { get; private set; } = new NamingService();

        public static ConfigService ConfigService { get; private set; } = new ConfigService();
    }
}
