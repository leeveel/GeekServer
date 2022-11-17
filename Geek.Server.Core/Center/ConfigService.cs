using System.Collections.Concurrent;

namespace Geek.Server.Core.Center
{
    internal class ConfigService
    {
        /// <summary>
        /// configid - config data
        /// </summary>
        internal readonly ConcurrentDictionary<string, byte[]> configMap = new();

        internal byte[] GetConfig(string configId)
        {
            configMap.TryGetValue(configId, out byte[] result);
            return result;
        }

        public List<byte[]> GetAllConfigs()
        {
            return configMap.Values.ToList();
        }

        internal void GetConfig(string configId, byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
