using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{

    [MessagePackObject(true)]
    public class Place
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    [MessagePackObject(true)]
    public class MoveMessage
    {
        public double X { get; set; }
        public double Y { get; set; }
        public long UserID { get; set; }
        public List<Place> places { get; set; }
    }

}
