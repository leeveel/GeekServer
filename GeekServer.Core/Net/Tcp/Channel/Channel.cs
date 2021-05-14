using System;
using DotNetty.Transport.Channels;
using System.Collections.Generic;

namespace Geek.Server
{
    public class Channel
    {
        /// <summary>
        /// 全局标识符
        /// </summary>
        public long Id { set; get; }

        /// <summary>
        /// 连接时间
        /// </summary>
        public DateTime Time { set; get; }

        /// <summary>
        /// netty通道
        /// </summary>
        public IChannelHandlerContext Ctx { set; get; }

        /// <summary>
        /// 连接标示，避免自己顶自己的号,客户端每次启动游戏生成一次
        /// </summary>
        public string Sign { get; set; }

        Dictionary<string, object> dataMap = new Dictionary<string, object>();

        public long GetLong(string key)
        {
            if (dataMap.ContainsKey(key))
                return (long)dataMap[key];
            return default;
        }

        public int GetInt(string key)
        {
            if (dataMap.ContainsKey(key))
                return (int)dataMap[key];
            return default;
        }

        public string GetString(string key)
        {
            if (dataMap.ContainsKey(key))
                return (string)dataMap[key];
            return default;
        }

        public void SetLong(string key, long value)
        {
            dataMap[key] = value;
        }

        public void SetInt(string key, int value)
        {
            dataMap[key] = value;
        }

        public void SetString(string key, string value)
        {
            dataMap[key] = value;
        }
    }
}
