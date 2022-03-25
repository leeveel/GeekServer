using System.Collections.Generic;

namespace Geek.Server
{
    public class HttpResult
    {
        ///<summary>成功</summary>
        public const int Code_Success = 200;
        ///<summary>未定义的命令</summary>
        public const int Code_Undefine = 11;
        ///<summary>非法</summary>
        public const int Code_Illegal = 12;
        ///<summary>参数错误</summary>
        public const int Code_ParamErr = 13;
        ///<summary>验证失败</summary>
        public const int Code_CheckFailed = 14;
        ///<summary>操作失败</summary>
        public const int Code_ActionFailed = 15;

        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public readonly static HttpResult Success = new HttpResult(Code_Success, "ok");
        public readonly static HttpResult Undefine = new HttpResult(Code_Undefine, "undefine command");
        public static HttpResult CreateOk(string retMsg = "")
        {
            return new HttpResult(Code_Success, retMsg);
        }

        public static HttpResult CreateErrorParam(string retMsg = "")
        {
            return new HttpResult(Code_ParamErr, retMsg);
        }

        public static HttpResult CreateActionFailed(string retMsg = "")
        {
            return new HttpResult(Code_ActionFailed, retMsg);
        }

        public int code;
        public string msg;
        readonly Dictionary<string, string> extraMap = new Dictionary<string, string>();
        public HttpResult(int retCode = 200, string retMsg = "ok")
        {
            code = retCode;
            msg = retMsg;
        }

        public string Get(string key)
        {
            if (extraMap.ContainsKey(key))
                return extraMap[key];
            return "";
        }

        /// <summary>
        /// key不能为code和msg
        /// </summary>
        public void Set(string key, string value)
        {
            if (key == "code" || key == "msg")
            {
                LOGGER.Error("HttpResult 额外信息key不能为:" + key);
                return;
            }
            extraMap[key] = value;
        }

        public override string ToString()
        {
            var json = new SimpleJSON.JSONClass();
            json["code"].AsInt = code;
            json["msg"] = msg;
            foreach(var kv in extraMap)
                json[kv.Key] = kv.Value;
            return json.ToString();
        }

        public static implicit operator string(HttpResult value)
        {
            return value.ToString();
        }
    }
}
