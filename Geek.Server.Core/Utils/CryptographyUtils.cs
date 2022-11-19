using System.Security.Cryptography;

namespace Geek.Server.Core.Utils
{
    public static class CryptographyUtils
    {
        private static readonly ThreadLocal<MD5> _md5Cache = new(() => MD5.Create());

        public static MD5 MD5
        {
            get
            {
                return _md5Cache.Value;
            }
        }

        public static string Md5(byte[] inputBytes)
        {
            var hashBytes = MD5.ComputeHash(inputBytes);
            var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
            return hash;
        }
    }
}
