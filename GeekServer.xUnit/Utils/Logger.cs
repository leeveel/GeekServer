using Xunit.Abstractions;

namespace Geek.Server.xUnit
{
    public class Logger
    {
        private static ITestOutputHelper outPut;

        public static void SetOutput(ITestOutputHelper output)
        {
            outPut = output;
        }

        public static void WriteLine(string msg)
        {
            if(outPut != null)
                outPut.WriteLine(msg);
        }


        public static void WriteLine(object msg)
        {
            if (outPut != null)
                outPut.WriteLine(msg.ToString());
        }

    }
}
