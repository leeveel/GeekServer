using System.Net.Sockets;

namespace Base.Net
{
    public class NetUtils
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string getIPv6(string mHost, string mPort);
#endif

        //"192.168.1.1&&ipv4"
        static string GetIPv6(string mHost, string mPort)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
		    string mIPv6 = getIPv6(mHost, mPort);
		    return mIPv6;
#else
            return mHost + "&&ipv4";
#endif
        }

        public static (AddressFamily, string) GetIPv6Address(string host, int port)
        {
            string ipv6 = GetIPv6(host, port.ToString());
            string ip = host;
            var ipType = AddressFamily.InterNetwork;
            if (!string.IsNullOrEmpty(ipv6))
            {
                string[] tmp = System.Text.RegularExpressions.Regex.Split(ipv6, "&&");
                if (tmp != null && tmp.Length >= 2)
                {
                    string type = tmp[1];
                    if (type == "ipv6")
                    {
                        ip = tmp[0];
                        ipType = AddressFamily.InterNetworkV6;
                    }
                }
            }
            return (ipType, ip);
        }
    }
}