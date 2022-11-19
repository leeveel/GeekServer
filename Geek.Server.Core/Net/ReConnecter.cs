using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Server.Core.Net
{
    public class ReConnecter
    {

        public int RetryTimes { private set; get; }

        public ReConnecter(int retryTimes)
        {
            RetryTimes = retryTimes;
        }



        public void OnDisconnected()
        {
            
        }

        public void OnConnectSuccessed()
        {

        }

        public void OnConnectFailed()
        {

        }

        public void NotifyDingTalk()
        {

        }

    }
}
