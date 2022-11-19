//auto generated, do not modify it

using Geek.Server.Core.Net.Messages;
using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public struct TestStruct 
	{
		[IgnoreMember]
		public const int Sid = 299119425;


        public int Age { get; set; }
        public string Name { get; set; }
	}
}
