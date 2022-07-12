using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
    /// <summary>
    /// 请求背包数据
    /// </summary>
	[MessagePackObject]
    [Serialize(112001, true)]
	public class ReqBagInfo 
	{
	}

	/// <summary>
	/// 返回背包数据
	/// </summary>
	[MessagePackObject]
	[Serialize(112002, true)]
	public class ResBagInfo
	{
		[Key(0)]
		public Dictionary<int, long> ItemDic { get; set; } = new Dictionary<int, long>();
	}

	/// <summary>
	/// 使用道具
	/// </summary>
	[MessagePackObject]
	[Serialize(112003, true)]
	public class ReqUseItem
	{
		/// <summary>
		/// 道具id
		/// </summary>
		[Key(0)]
		public int ItemId { get; set; }
	}

	/// <summary>
	/// 出售道具
	/// </summary>
	[MessagePackObject]
	[Serialize(112004, true)]
	public class ReqSellItem
	{
		/// <summary>
		/// 道具id
		/// </summary>
		[Key(0)]
		public int ItemId { get; set; }
	}

	[MessagePackObject]
	[Serialize(112005, true)]
	public class ResItemChange
	{
		/// <summary>
		/// 变化的道具
		/// </summary>
		[Key(0)]
		public Dictionary<int, long> ItemDic { get; set; }
	}

}
