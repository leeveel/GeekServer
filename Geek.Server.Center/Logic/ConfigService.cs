using System.Collections.Concurrent;

namespace Geek.Server.Center.Logic
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

        internal void SetConfig(string configId, byte[] data)
        {
            configMap[configId] = data;
        }
    }
}
