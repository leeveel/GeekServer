using MessagePack;

namespace Geek.Server.Proto
{
    [MessagePackObject]
    [Serialize(111111)]
    public class A
    {
        [Key(0)]
        public int Age { get; set; }
    }

    [MessagePackObject]
    [Serialize(111112)]
    public class B : A
    {
        [Key(1)]
        public string Name { get; set; }
        [IgnoreMember]
        public string Test { get; set; }
    }


    /// <summary>
    /// 玩家基础信息
    /// </summary>
    [MessagePackObject]
    [Serialize(111000)]
    public class UserInfo
    {
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

    /// <summary>
    /// 请求登录
    /// </summary>
    [MessagePackObject]
    [Serialize(111001)]
    public class ReqLogin : Message
    {
        [Key(1)]
        public string UserName { get; set; }
        [Key(2)]
        public string Platform { get; set; }
        [Key(3)]
        public int SdkType { get; set; }
        [Key(4)]
        public string SdkToken { get; set; }
        [Key(5)]
        public string Device { get; set; }
    }


    /// <summary>
    /// 请求登录
    /// </summary>
    [MessagePackObject]
    [Serialize(111002)]
    public class ResLogin : Message
    {
        /// <summary>
        /// 登陆结果，0成功，其他时候为错误码
        /// </summary>
        [Key(1)]
        public int Code { get; set; }
        [Key(2)]
        public UserInfo UserInfo { get; set; }
    }


    /// <summary>
    /// 等级变化
    /// </summary>
    [MessagePackObject]
    [Serialize(111003)]
    public class ResLevelUp : Message
    {
        /// <summary>
        /// 玩家等级
        /// </summary>
        [Key(1)]
        public int Level { get; set; }
    }

    /// <summary>
    /// 双向心跳/收到恢复同样的消息
    /// </summary>
    [MessagePackObject]
    [Serialize(111004)]
    public class HearBeat : Message
    {
        /// <summary>
        /// 当前时间
        /// </summary>
        [Key(1)]
        public long TimeTick { get; set; }
    }

    /// <summary>
    /// 客户端每次请求都会回复错误码
    /// </summary>
    [MessagePackObject]
    [Serialize(111005)]
    public class ResErrorCode : Message
    {
        /// <summary>
        /// 0:表示无错误
        /// </summary>
        [Key(1)]
        public long ErrCode { get; set; }
        /// <summary>
        /// 错误描述（不为0时有效）
        /// </summary>
        [Key(2)]
        public string Desc { get; set; }
    }

}
