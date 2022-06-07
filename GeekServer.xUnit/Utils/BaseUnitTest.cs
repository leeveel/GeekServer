using Xunit.Abstractions;

namespace Geek.Server.xUnit
{
    public class BaseUnitTest
    {
        protected readonly ITestOutputHelper Output;

        public BaseUnitTest(ITestOutputHelper output)
        {
            Output = output;
            Logger.SetOutput(output);
        }
    }
}
