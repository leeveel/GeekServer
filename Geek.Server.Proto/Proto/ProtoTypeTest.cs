using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
    public enum TestEnum
    {
        A, B, C, D, E, F, G, H, I, J, K, L,
    }


    [MessagePackObject(true)]
    public struct TestStruct
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }

    [MessagePackObject(true)]
    public class A
    {
        public int Age { get; set; }
        public TestEnum E { get; set; } = TestEnum.B;
        public TestStruct TS { get; set; }
    }

    [MessagePackObject(true)]
    public class B : A
    {
        public string Name { get; set; }
        [IgnoreMember]
        public string Test { get; set; }
    }
}
