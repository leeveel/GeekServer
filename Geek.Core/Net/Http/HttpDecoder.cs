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
using NLog;
using SimpleJSON;
using System.Collections.Generic;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.Multipart;
using DotNetty.Transport.Channels;

namespace Geek.Core.Net.Http
{
    public class HttpDecoder : SimpleChannelInboundHandler<IFullHttpRequest>
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        protected override void ChannelRead0(IChannelHandlerContext ctx, IFullHttpRequest msg)
        {
            try
            {
                IFullHttpRequest request = msg;
                string uri = request.Uri;
                QueryStringDecoder queryStringDecoder = new QueryStringDecoder(uri);
                string path = queryStringDecoder.Path;
                if (!path.Equals(Settings.Ins.httpUrl))
                {
                    ctx.CloseAsync();
                    return;
                }

                // chrome等浏览器会请求一次.ico
                if (uri.EndsWith(".ico"))
                {
                    ctx.WriteAndFlushAsync(_response(""));
                    return;
                }

                Dictionary<string, string> parmMap = new Dictionary<string, string>();
                HttpMethod method = request.Method;

                QueryStringDecoder decoder = new QueryStringDecoder(request.Uri);
                foreach (var keyValuePair in decoder.Parameters)
                {
                    parmMap.Add(keyValuePair.Key, keyValuePair.Value[0]);
                }

                if (Equals(HttpMethod.Post, method))
                {
                    var headCType = request.Headers.Get(HttpHeaderNames.ContentType, null);
                    if (headCType == null)
                    {
                        ctx.WriteAndFlushAsync(_response(HttpHeaderNames.ContentType + " is null"));
                        ctx.CloseAsync();
                        return;
                    }
                    
                    string content_type = headCType.ToString().ToLower();
                    if (content_type != null && content_type.Equals("application/json"))
                    {
                        // json 格式
                        string str = request.Content.ToString(Encoding.UTF8);
                        var jsonNode = JSON.Parse(str) as JSONClass;
                        if (jsonNode == null)
                            return;

                        var enumerator = jsonNode.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            var keyValuePair = (KeyValuePair<string, JSONNode>)enumerator.Current;
                            if(parmMap.ContainsKey(keyValuePair.Key))
                            {
                                ctx.WriteAndFlushAsync(_response("参数重复了>" + keyValuePair.Key));
                                ctx.CloseAsync();
                                return;
                            }

                            if(!string.IsNullOrEmpty(keyValuePair.Value.Value))
                                parmMap.Add(keyValuePair.Key, keyValuePair.Value.Value);
                            else
                                parmMap.Add(keyValuePair.Key, keyValuePair.Value.ToString());
                        }
                    }
                    else
                    {
                        // key value 形式
                        HttpPostRequestDecoder decoder1 = new HttpPostRequestDecoder(request);
                        decoder1.Offer(request);
                        List<IInterfaceHttpData> parmList = decoder1.GetBodyHttpDatas();
                        foreach (var httpData in parmList)
                        {
                            if (httpData is IAttribute data)
                                parmMap.Add(data.Name, data.Value);
                        }

                        decoder1.Destroy();
                    }
                }

                string res = handleHttpRequest(ctx.Channel.RemoteAddress.ToString(), uri, parmMap);
                ctx.WriteAndFlushAsync(_response(res));
            }
            catch (Exception e)
            {
                LOGGER.Error("HTTP parse ERROR! {} \n {}", e.Message, e.StackTrace);
                ctx.WriteAndFlushAsync(_response(e.Message));
                ctx.CloseAsync();
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext ctx)
        {
            ctx.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
        {
            LOGGER.Error("httpServerHandler Exception : {}, {}", cause.Message, cause);
            ctx.CloseAsync();
        }

        public virtual string handleHttpRequest(string ip, string url, Dictionary<string, string> parameters)
        {
            LOGGER.Info("收到来自[{}]的HTTP请求. 请求url:[{}]", ip, url);
            StringBuilder bu = new StringBuilder();
            LOGGER.Info("请求参数:");
            foreach (var parameter in parameters)
            {
                if (parameter.Key.Equals(""))
                    continue;
                bu.Append("'").Append(parameter.Key).Append("'='").Append(parameter.Value).Append("'  ");
            }

            LOGGER.Info(bu.ToString());
            return "ok";
        }

        /// <summary>
        /// 构造返回信息
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private static DefaultFullHttpResponse _response(string result)
        {
            var b = Encoding.UTF8.GetBytes(result);
            DefaultFullHttpResponse response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK,
                Unpooled.WrappedBuffer(b));
            response.Headers.Add(HttpHeaderNames.ContentEncoding, "UTF-8");
            response.Headers.Add(HttpHeaderNames.ContentType, "text/html");
            response.Headers.Add(HttpHeaderNames.ContentLength, b.Length);
            return response;
        }
    }
}

