﻿using Geek.Server.Core.Center;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace Geek.Server.Center.Logic
{
    internal class ConfigService
    {
        private DBService dbService;
        private SubscribeService subscribeService;
        /// <summary>
        /// configid - config data
        /// </summary>
        internal readonly ConcurrentDictionary<string, ConfigInfo> configMap = new();

        public ConfigService(DBService dbService, SubscribeService subscribeService)
        {
            this.dbService = dbService;
            this.subscribeService = subscribeService;
            var cfgs = dbService.GetAllData<ConfigInfo>();
            foreach (var c in cfgs)
            {
                configMap[c.CfgId] = c;
            }
            if (!configMap.ContainsKey("global"))
            {
                var info = new ConfigInfo
                {
                    CfgId = "global",
                    Describe = "global setting"
                };
                GlobalSetting globalSetting = new GlobalSetting();
                globalSetting.LocalDBPath = "../../database/game/";
                globalSetting.LocalDBPrefix = "gamedb_";
                globalSetting.HttpInnerCode = "inner_httpcode";
                globalSetting.HttpCode = "httpcode";
                globalSetting.MongoUrl = "mongodb://127.0.0.1:27017/?authSource=admin";
                info.Data = JsonConvert.SerializeObject(globalSetting);
                SetConfig(info);
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
            subscribeService.Publish(SubscribeEvent.ConfigChange, cfg);
        }

        internal void DeleteConfig(ConfigInfo cfg)
        {
            configMap.TryRemove(cfg.CfgId, out _);
            dbService.DeleteData<ConfigInfo>(cfg.CfgId);
            cfg.Data = null;
            subscribeService.Publish(SubscribeEvent.ConfigChange, cfg);
        }

        internal void UpdateConfig(ConfigInfo cfg)
        {
            configMap[cfg.CfgId] = cfg;
            dbService.UpdateData(cfg.CfgId, cfg);
            subscribeService.Publish(SubscribeEvent.ConfigChange, cfg);
        }
    }
}