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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;

namespace Geek.Core.Net.Http
{
    /// <summary>
    /// 游戏逻辑http 具体实现
    /// </summary>
    public class HttpServerHandler : HttpDecoder
    {
        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();
        public override string handleHttpRequest(string ip, string url, Dictionary<string, string> paramMap)
        {
            LOGGER.Info("收到来自[{}]的HTTP请求. 请求url:[{}]", ip, url);
            try
            {
                foreach (var parameter in paramMap)
                {
                    if (parameter.Key.Equals(""))
                        continue;
                    LOGGER.Info("key:[{}] value:[{}]", parameter.Key, parameter.Value);
                }

                var cmd = "";
                if (paramMap.ContainsKey("command"))
                    cmd = paramMap["command"];
                var handler = HttpHandlerFactory.GetHandler(cmd, paramMap);

                if (handler == null)
                {
                    LOGGER.Warn($"http cmd handler 不存在：{cmd}");
                    return HttpResult.Undefine;
                }

                if (handler == null)
                {
                    LOGGER.Warn($"http cmd handler 为空：{cmd}");
                    return HttpResult.Undefine;
                }

                //验证
                var checkCode = handler.CheckSgin(paramMap);
                if (!string.IsNullOrEmpty(checkCode))
                    return checkCode;

                //不用Task.Run,Handler所有await无法返回
                var ret = Task.Run(() => { return handler.Action(ip, url, paramMap); }).Result;
                LOGGER.Warn("http result>" + ret);
                return ret;
            }
            catch (Exception e)
            {
                LOGGER.Error("执行http异常. {} {}", e.Message, e.StackTrace);
                return e.Message;
            }
        }
    }
}

