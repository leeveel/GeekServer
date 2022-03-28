using System.Collections.Generic;

namespace Geek.Server.Proto
{
    /// <summary>
    /// 请求背包数据
    /// </summary>
    [SClass(SID._112001, SO.Msg)]
	public class ReqBagInfo : BaseMessage { }

	/// <summary>
	/// 返回背包数据
	/// </summary>
	[SClass(SID._112002, SO.Msg)]
	public class ResBagInfo : BaseMessage
	{
		[SProperty(0)]
		public Dictionary<int, long> ItemDic { get; set; }
	}

	/// <summary>
	/// 使用道具
	/// </summary>
	[SClass(SID._112003, SO.Msg)]
	public class ReqUseItem : BaseMessage
	{
		/// <summary>
		/// 道具id
		/// </summary>
		[SProperty(0)]
		public int ItemId { get; set; }
	}

	/// <summary>
	/// 出售道具
	/// </summary>
	[SClass(SID._112004, SO.Msg)]
	public class ReqSellItem : BaseMessage
	{
		/// <summary>
		/// 道具id
		/// </summary>
		[SProperty(0)]
		public int ItemId { get; set; }
	}

	[SClass(SID._112005, SO.Msg)]
	public class ResItemChange : BaseMessage
	{
		/// <summary>
		/// 变化的道具
		/// </summary>
		[SProperty(0)]
		public Dictionary<int, long> ItemDic { get; set; }
	}

}
