//auto generated, do not modify it

using Protocol;
using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class MoveMessage 
	{
		[IgnoreMember]
		public const int Sid = 696796597;


        public double X { get; set; }
        public double Y { get; set; }
        public long UserID { get; set; }
        public List<Place> places { get; set; }
	}
}
