/****************************************************************************
Copyright (c) Geek

https://github.com/leeveel/GeekServer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using System.Collections.Generic;

namespace Geek.Core.Net.Http
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

        static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public readonly static HttpResult Success = new HttpResult(Code_Success, "ok");
        public readonly static HttpResult Undefine = new HttpResult(Code_Undefine, "undefine command");

        public int code;
        public string msg;
        Dictionary<string, string> extraMap = new Dictionary<string, string>();
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
                LOGGER.Error("HttpResult 额外信息key不能为>" + key);
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
