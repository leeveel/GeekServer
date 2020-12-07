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
using Newtonsoft.Json;
using MongoDB.Bson.Serialization.Attributes;

namespace Geek.Core.Storage
{
    [BsonIgnoreExtraElements(true, Inherited = true)]//忽略代码删除的字段[数据库多余的字段]
    public abstract class CacheState
    {
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        ///<summary>actorId</summary>
        public long _id;

        public string ToJson() { return JsonConvert.SerializeObject(this); }

        [BsonIgnore]
        [JsonIgnore]
        public string toSaveMD5;//timer获取md5

        [BsonIgnore]
        [JsonIgnore]
        public string cacheMD5;//上次回存时的md5
        public string GetMD5()
        {
            try
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(ToJson());
                byte[] md5Bytes = new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(data);
                return BitConverter.ToString(md5Bytes);
            }catch(Exception e)
            {
                LOGGER.Fatal("state to json error:" + this.GetType().FullName);
                LOGGER.Fatal(e.ToString());
                return "";
            }
        }
    }

    [BsonIgnoreExtraElements(true, Inherited = true)]//忽略代码删除的字段[数据库多余的字段]
    public abstract class InnerState
    {

    }
}
