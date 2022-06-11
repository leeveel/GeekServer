using Geek.Server.xUnit;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace GeekServer.xUnit
{
    public class CompressTest : BaseUnitTest
    {
        public CompressTest(ITestOutputHelper output) 
            : base(output)
        {
        }


        [Fact]
        public void Zip()
        {
            var before = new byte[50];
            for(int i=0; i< before.Length; i++)
                before[i] = (byte)i;
            Logger.WriteLine("before:" + before.Length.ToString());
            var after = ZipTool.CompressGZip(before);
            Logger.WriteLine("after:" + after.Length.ToString());



            var unziped = ZipTool.UnGZip(after, 0, after.Length);
            Logger.WriteLine("unziped:" + unziped.Length.ToString());
            for (int i = 0; i < unziped.Length; i++)
            {
                Logger.WriteLine("unziped:" + unziped[i]);
            }
        }


        [Fact]
        public void TestArrayPool()
        {
            Logger.WriteLine(ArrayPool<byte>.Shared.Rent(65).Length);
            Logger.WriteLine(ArrayPool<byte>.Shared.Rent(129).Length);
        }


    }
}
