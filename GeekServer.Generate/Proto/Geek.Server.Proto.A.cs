//auto generated, do not modify it

using MessagePack;

namespace Geek.Server.Proto
{
    [MessagePackObject(true)]
    public class A
    {
        [IgnoreMember]
        public const int Sid = 1544071388;


        public int Age { get; set; }
        public TestEnum E { get; set; } = TestEnum.B;
        public TestStruct TS { get; set; }
    }
}
