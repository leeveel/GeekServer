using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Geek.Server.Core.Net.Http
{



    public class HttpResult
    {
        public enum Stauts
        {
            ///<summary>成功</summary>
            Success = 200,
            ///<summary>未定义的命令</summary>
            Undefine = 11,
            ///<summary>非法</summary>
            Illegal = 12,
            ///<summary>参数错误</summary>
            ParamErr = 13,
            ///<summary>验证失败</summary>
            CheckFailed = 14,
            ///<summary>操作失败</summary>
            ActionFailed = 15
        }

        public readonly static HttpResult Success = new HttpResult(Stauts.Success, "ok");
        public readonly static HttpResult Undefine = new HttpResult(Stauts.Undefine, "undefine command");
        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            //WriteIndented = true
        };
        public static HttpResult CreateOk(string retMsg = "")
        {
            return new HttpResult(Stauts.Success, retMsg);
        }

        public static HttpResult CreateErrorParam(string retMsg = "")
        {
            return new HttpResult(Stauts.ParamErr, retMsg);
        }

        public static HttpResult CreateActionFailed(string retMsg = "")
        {
            return new HttpResult(Stauts.ActionFailed, retMsg);
        }

        public string Code { get; set; }
        public string Msg { get; set; }
        public Dictionary<string, string> ExtraMap { get; set; }
        public HttpResult(Stauts retCode = Stauts.Success, string retMsg = "ok")
        {
            Code = retCode.ToString();
            Msg = retMsg;
        }

        public string Get(string key)
        {
            if (ExtraMap == null)
                return null;
            ExtraMap.TryGetValue(key, out var res);
            return res;
        }

        public void Set(string key, string value)
        {
            if (ExtraMap == null)
                ExtraMap = new Dictionary<string, string>();
            ExtraMap[key] = value;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, GetType(), options);
        }

        public static implicit operator string(HttpResult value)
        {
            return value.ToString();
        }
    }
}
