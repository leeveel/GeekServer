//auto generated, do not modify it

using Geek.Server.Core.Net.Messages;
using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class A 
	{
		[IgnoreMember]
		public const int Sid = 1250601847;


        public int Age { get; set; }
        public TestEnum E { get; set; } = TestEnum.B;
        public TestStruct TS { get; set; }
	}
}
