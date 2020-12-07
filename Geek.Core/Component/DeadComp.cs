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
using Geek.Core.Storage;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.Core.Component
{
    public class DeadComp : QueryComponent
    {
        public async Task Die(List<Type> compTypeList)
        {
            foreach (var t in compTypeList)
            {
                var key = "";
                var p = t.GetProperty("State");
                if (p != null)
                {
                    key = p.PropertyType.FullName;
                }
                else
                {
                    var f = t.GetField("State");
                    if (f != null)
                        key = f.FieldType.FullName;
                }
                if (string.IsNullOrEmpty(key))
                    continue;

                var col = GetDB().GetCollection<BsonDocument>(key);
                var filter = Builders<BsonDocument>.Filter.Eq(MongoField.UniqueId, ActorId);
                await col.DeleteOneAsync(filter);
            }
        }
    }
}
