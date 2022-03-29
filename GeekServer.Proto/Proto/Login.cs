namespace Geek.Server.Proto
{
    /// <summary>
    /// 玩家基础信息
    /// </summary>
	[IsState]
    [SClass(SID._111100, SO.NotMsg)]
	public class UserInfo
	{
		/// <summary>
		/// 角色名
		/// </summary>
		[SProperty(0)]
        public string RoleName { get; set; }
		/// <summary>
		/// 角色ID
		/// </summary>
		[SProperty(1)]
        public long RoleId { get; set; }
		/// <summary>
		/// 角色等级
		/// </summary>
		[SProperty(2)]
        public int Level { get; set; }
		/// <summary>
		/// 创建时间
		/// </summary>
		[SProperty(3)]
        public long CreateTime { get; set; }
		/// <summary>
		/// vip等级
		/// </summary>
		[SProperty(4)]
        public int VipLevel { get; set; }
	}

	/// <summary>
	/// 请求登录
	/// </summary>
	[SClass(SID._111101)]
	public class ReqLogin
	{
		[SProperty(0)]
        public string UserName { get; set; }
		[SProperty(1)]
        public string Platform { get; set; }
		[SProperty(2)]
        public int SdkType { get; set; }
		[SProperty(3)]
        public string SdkToken { get; set; }
		[SProperty(4)]
		public string Device { get; set; }
	}


	/// <summary>
	/// 请求登录
	/// </summary>
	[SClass(SID._111102)]
	public class ResLogin
	{
		/// <summary>
		/// 登陆结果，0成功，其他时候为错误码
		/// </summary>
		[SProperty(0)]
        public int Code { get; set; }
		[SProperty(1)]
        public UserInfo UserInfo { get; set; }
	}


	/// <summary>
	/// 等级变化
	/// </summary>
	[SClass(SID._111103)]
	public class ResLevelUp
	{
		/// <summary>
		/// 玩家等级
		/// </summary>
		[SProperty(0)]
        public int Level { get; set; }
	}

	/// <summary>
	/// 双向心跳/收到恢复同样的消息
	/// </summary>
	[SClass(SID._111104)]
	public class HearBeat
	{
		/// <summary>
		/// 当前时间
		/// </summary>
		[SProperty(0)]
		public long TimeTick { get; set; }
	}

	/// <summary>
	/// 客户端每次请求都会回复错误码
	/// </summary>
	[SClass(SID._111105)]
	public class ResErrorCode
	{
		/// <summary>
		/// 0:表示无错误
		/// </summary>
		[SProperty(0)]
		public long ErrCode { get; set; }
		/// <summary>
		/// 错误描述（不为0时有效）
		/// </summary>
		[SProperty(1)]
		public string Desc { get; set; }
	}

}
