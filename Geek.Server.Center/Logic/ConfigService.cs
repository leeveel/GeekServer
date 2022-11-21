using Geek.Server.Center.Web.Data;
using Geek.Server.Center.Web.Pages.Config;
using Geek.Server.Core.Center;
using System.Collections.Concurrent;

namespace Geek.Server.Center.Logic
{
    internal class ConfigService
    {
        private DBService dbService;
        /// <summary>
        /// configid - config data
        /// </summary>
        internal readonly ConcurrentDictionary<string, ConfigInfo> configMap = new();

        public ConfigService(DBService dbService)
        {
            this.dbService = dbService;
            var cfgs = dbService.GetAllData<ConfigInfo>();
            foreach (var c in cfgs)
            {
                configMap[c.CfgId] = c;
            }
        }
        internal int ConfigCount()
        {
            return configMap.Count;
        }
        internal ConfigInfo GetConfig(string configId)
        {
            configMap.TryGetValue(configId, out var result);
            return result;
        }

        public List<ConfigInfo> GetAllConfigs()
        {
            return new List<ConfigInfo>(configMap.Values.ToArray());
        }

        internal void SetConfig(ConfigInfo cfg)
        {
            //todo  需要判断是否存在
            configMap[cfg.CfgId] = cfg;
            dbService.UpdateData(cfg.CfgId, cfg);
        }

        internal void DeleteConfig(ConfigInfo cfg)
        {
            configMap.TryRemove(cfg.CfgId, out _);
            dbService.DeleteData<ConfigInfo>(cfg.CfgId);
        }

        internal void UpdateConfig(ConfigInfo cfg)
        {
            configMap[cfg.CfgId] = cfg;
            dbService.UpdateData(cfg.CfgId, cfg);
        }
    }
}
