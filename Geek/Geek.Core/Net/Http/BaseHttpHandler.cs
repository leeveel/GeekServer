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
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geek.Core.Net.Http
{
    public abstract class BaseHttpHandler
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public virtual bool checkSign => true;
        string GetStringSign(string str)
        {
            var data = Encoding.UTF8.GetBytes(str);
            byte[] md5Bytes = new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(data);
            string md5 = BitConverter.ToString(md5Bytes).Replace("-", "").ToLower();
            return md5;
        }

        public string CheckSgin(Dictionary<string, string> paramMap)
        {
            if (!checkSign)
                return "";

            if (!paramMap.ContainsKey("sign") || paramMap.ContainsKey("code"))
            {
                LOGGER.Error("http命令未包含验证参数");
                return new HttpResult(HttpResult.Code_Illegal, "http命令未包含验证参数sign或者code");
            }
            var sign = paramMap["sign"];
            var code = paramMap["code"];

            var str = Settings.Ins.httpCode + code;
            if (sign == GetStringSign(str))
                return "";
            else
                return new HttpResult(HttpResult.Code_Illegal, "命令验证失败");
        }

        public abstract string Action(string ip, string url, Dictionary<string, string> paramMap);
    }
}