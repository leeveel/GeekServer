
using MessagePack;

namespace Geek.Server.Proto
{
    public enum TestEnum
    {
        A, B, C, D, E, F, G, H, I, J, K, L,
    }


    [MessagePackObject(true)]
    public struct TestStruct
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }

    [MessagePackObject(true)]
    public class A
    {
        public int Age { get; set; }
        public TestEnum E { get; set; } = TestEnum.B;
        public TestStruct TS { get; set; }
    }

    [MessagePackObject(true)]
    public class B : A
    {
        public string Name { get; set; }
        [IgnoreMember]
        public string Test { get; set; }
    }


    /// <summary>
    /// 玩家基础信息
    /// </summary>
    [MessagePackObject(true)]
    public class UserInfo
    {
        /// <summary>
        /// 角色名
        /// </summary>
        public string RoleName { get; set; }
        /// <summary>
        /// 角色ID
        /// </summary>
        public long RoleId { get; set; }
        /// <summary>
        /// 角色等级
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public long CreateTime { get; set; }
        /// <summary>
        /// vip等级
        /// </summary>
        public int VipLevel { get; set; }
    }

    /// <summary>
    /// 请求登录
    /// </summary>
    [MessagePackObject(true)]
    public class ReqLogin : Message
    {
        public string UserName { get; set; }
        public string Platform { get; set; }
        public int SdkType { get; set; }
        public string SdkToken { get; set; }
        public string Device { get; set; }
    }


    /// <summary>
    /// 请求登录
    /// </summary>
    [MessagePackObject(true)]
    public class ResLogin : Message
    {
        /// <summary>
        /// 登陆结果，0成功，其他时候为错误码
        /// </summary>
        public int Code { get; set; }
        public UserInfo UserInfo { get; set; }
    }


    /// <summary>
    /// 等级变化
    /// </summary>
    [MessagePackObject(true)]
    public class ResLevelUp : Message
    {
        /// <summary>
        /// 玩家等级
        /// </summary>
        public int Level { get; set; }
    }

    /// <summary>
    /// 双向心跳/收到恢复同样的消息
    /// </summary>
    [MessagePackObject(true)]
    public class HearBeat : Message
    {
        /// <summary>
        /// 当前时间
        /// </summary>
        public long TimeTick { get; set; }
    }

    /// <summary>
    /// 客户端每次请求都会回复错误码
    /// </summary>
    [MessagePackObject(true)]
    public class ResErrorCode : Message
    {
        /// <summary>
        /// 0:表示无错误
        /// </summary>
        public long ErrCode { get; set; }
        /// <summary>
        /// 错误描述（不为0时有效）
        /// </summary>
        public string Desc { get; set; }
    }

    [MessagePackObject(true)]
    public class ResPrompt : Message
    {
        ///<summary>提示信息类型（1Tip提示，2跑马灯，3插队跑马灯，4弹窗，5弹窗回到登陆，6弹窗退出游戏）</summary>
		public int Type { get; set; }
        ///<summary>提示内容</summary>
        public string Content { get; set; }
    }

}
