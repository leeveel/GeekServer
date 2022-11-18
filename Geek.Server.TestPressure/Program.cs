
namespace Test.Pressure
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            PolymorphicRegister.Load();
            LogManager.Configuration = new XmlLoggingConfiguration("Configs/test_NLog.config");

            TestSettings.Load("Configs/test_config.json");
            var maxCount = TestSettings.Ins.clientCount;
            for (int i = 0; i < maxCount; i++)
            {
                _ = new Client(CreateRoleId(i)).Start();
                await Task.Delay(5);
            }
            Console.ReadLine();
        }
        private static long CreateRoleId(int index)
        {
            long actorType = (long)ActorType.Role;
            long res = (long)666 << 46;//(63-17) 
            res |= actorType << 42; //(63-4-17) 
            return res | (long)index;
        }
    }
}