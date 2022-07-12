//auto generated, do not modify it

using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject]
	public class UserInfo 
	{
		[IgnoreMember]
		public const int Sid = 111000;

		/// <summary>
		/// 角色名
		/// </summary>
		[Key(0)]
        public string RoleName { get; set; }
		/// <summary>
		/// 角色ID
		/// </summary>
		[Key(1)]
        public long RoleId { get; set; }
		/// <summary>
		/// 角色等级
		/// </summary>
		[Key(2)]
        public int Level { get; set; }
		/// <summary>
		/// 创建时间
		/// </summary>
		[Key(3)]
        public long CreateTime { get; set; }
		/// <summary>
		/// vip等级
		/// </summary>
		[Key(4)]
        public int VipLevel { get; set; }
	}
}
