
namespace Geek.Server
{
    /// <summary>
    /// Grpc 调用状态码
    /// </summary>
    public enum GrpcCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        OK = 0,
        /// <summary>
        /// 目标服务器配置未找到
        /// </summary>
        TARGET_SERVER_CONFIG_NOT_FOUND = -1,
        /// <summary>
        /// Grpc调用异常
        /// </summary>
        GRPC_CALL_EXCEPTION = -2,
        /// <summary>
        /// Grpc ReplyMsg未注册
        /// </summary>
        REPLY_MSG_ERROR = -3,
        /// <summary>
        /// Grpc ReplyMsg反序列化失败
        /// </summary>
        REQUEST_MSG_ERROR = -4,
        /// <summary>
        /// 没有找到处理该消息的handler
        /// </summary>
        HANDLER_NOT_FOUND = -5,
        /// <summary>
        /// handler ActionAsync处理异常
        /// </summary>
        HANDLER_EXCEPTION = -6,
        /// <summary>
        /// 服务器非运行状态
        /// </summary>
        APP_NOT_RUNNING = -7,
        /// <summary>
        /// 失败
        /// </summary>
        FAILED = -8,
    }

    public class GrpcRes
    {
        public static GrpcRes OK = Create((int)GrpcCode.OK);
        public static GrpcRes FAILED = Create((int)GrpcCode.FAILED);
        public static bool IsOK(int code) { return code == (int)GrpcCode.OK; }

        public bool IsOk => Code == (int)GrpcCode.OK;

        public readonly int Code;
        internal readonly IMessage imsg;
        

        internal GrpcRes(int code, IMessage msg)
        {
            Code = code;
            imsg = msg;
        }

        public static GrpcRes Create(int code, IMessage msg = null)
        {
            return new GrpcRes(code, msg);
        }

        public static GrpcRes Create(IMessage msg)
        {
            return new GrpcRes((int)GrpcCode.OK, msg);
        }

    }

    public class GrpcRes<T> : GrpcRes where T : IMessage
    {
        public T Msg => (T)imsg;
        internal GrpcRes(int code, IMessage msg) : base(code, msg)
        {
        }
    }
}
