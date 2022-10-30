using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekServer.Gateaway.MessageHandler
{
    public abstract class BaseHander
    {
        public virtual void Run(INetNode node, object Msg)
        {

        }
    }
}
