using System.Collections.Generic;

namespace Geek.Server
{
    public class PolymorphicInfo
    {
        public string basename { get; set; }

        public string subname { get; set; }

        public string subsid { get; set; }
    }

    public class PolymorphicInfoArray
    {
        public string prefix = "";
        public List<PolymorphicInfo> infos = new List<PolymorphicInfo>();
    }
}
