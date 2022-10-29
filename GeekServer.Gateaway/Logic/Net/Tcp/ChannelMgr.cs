using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekServer.Gateaway.Net.Tcp
{
    internal class ChannelMgr
    {
        internal static readonly ConcurrentDictionary<long, Channel> ChannelMap = new();
        public static void Remove(long id)
        {

        }

        public static void RemoveAll()
        {

        }

        public static Channel Get(long id)
        {
            return null;
        }

        public static void Add(long id, Channel channel)
        {
            channel.SetSessionId(id);
        }
    }
}
