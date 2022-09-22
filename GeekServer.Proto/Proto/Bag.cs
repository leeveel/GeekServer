using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{

    /// <summary>
    /// 请求背包数据
    /// </summary>
    [MessagePackObject(true)]
    public class ReqBagInfo : Message
    {
    }

    /// <summary>
    /// 返回背包数据
    /// </summary>
    [MessagePackObject(true)]
    public class ResBagInfo : Message
    {
        public Dictionary<int, long> ItemDic { get; set; } = new Dictionary<int, long>();
    }

    /// <summary>
    /// 使用道具
    /// </summary>
    [MessagePackObject(true)]
    public class ReqUseItem : Message
    {
        /// <summary>
        /// 道具id
        /// </summary>
        public int ItemId { get; set; }
    }

    /// <summary>
    /// 出售道具
    /// </summary>
    [MessagePackObject(true)]
    public class ReqSellItem : Message
    {
        /// <summary>
        /// 道具id
        /// </summary>
        public int ItemId { get; set; }
    }

    [MessagePackObject(true)]
    public class ResItemChange : Message
    {
        /// <summary>
        /// 变化的道具
        /// </summary>
        public Dictionary<int, long> ItemDic { get; set; }
    }

}
