using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Server.Center.Common
{
    internal class CenterSetting : BaseSetting
    {
        public int RpcPort { get; set; }
        public string InitUserName { get; set; }
        public string InitPassword { get; set; }
        public string WebServerUrl { get; set; }
    }
}
