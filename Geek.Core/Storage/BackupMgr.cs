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
using Geek.Core.Actor;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Geek.Core.Storage
{
    public class BackupMgr
    {
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public readonly static BackupMgr Singleton = new BackupMgr();
        const int lineNum = 100;
        WorkerActor lineActor = new WorkerActor(lineNum);

        const string Time_Format = "yyyy-MM-dd HH.mm";
        string NeedRestoreFlagFile = Environment.CurrentDirectory + "/saveFailed.flag";
        DateTime oldClearTime;
        ConcurrentDictionary<Type, ConcurrentDictionary<long, long>> backupTimeMap = new ConcurrentDictionary<Type, ConcurrentDictionary<long, long>>();

        /// <summary>
        /// 备份数据到本地磁盘
        /// </summary>
        /// <param name="stateList">数据</param>
        /// <param name="checkColdTime">是否考虑冷却时间</param>
        /// <param name="alwaysBackup">是否忽略备份开关(关服时存数据库失败了)</param>
        public Task Backup(List<CacheState> stateList, bool checkColdTime, bool alwaysBackup)
        {
            if (Settings.Ins.backupSpan <= 0 && !alwaysBackup)
                return Task.CompletedTask;

            var splitList = new List<List<CacheState>>();
            int num = stateList.Count / lineNum + 1;
            int idx = 0;
            for (int i = 0; i < lineNum; ++i)
            {
                //平均分到每个线程
                var list = new List<CacheState>();
                var addNum = Math.Min(num, stateList.Count - idx);
                list.AddRange(stateList.GetRange(idx, addNum));
                idx += addNum;
                splitList.Add(list);
            }

            lineActor.SendAsync(clearOldBackup);
            var taskList = new List<Task>();
            foreach(var sList in splitList)
            {
                var list = sList;
                var task = lineActor.SendAsync(() =>
                {
                    var now = DateTime.Now;
                    foreach (var state in list)
                    {
                        backupTimeMap.TryGetValue(state.GetType(), out var map);
                        if (map == null)
                            backupTimeMap.TryAdd(state.GetType(), new ConcurrentDictionary<long, long>());
                        backupTimeMap.TryGetValue(state.GetType(), out map);
                        var time = Settings.Ins.StartServerTime;
                        if (map.ContainsKey(state._id))
                            time = new DateTime(map[state._id]); 
                        if (checkColdTime && (now - time).TotalMinutes < Settings.Ins.backupSpan)
                            continue;
                        map.TryRemove(state._id, out _);
                        map.TryAdd(state._id, now.Ticks);

                        try
                        {
                            //stateName/id/time.zip
                            var path = $"{getBackupFolder()}/{state.GetType().FullName}/{state._id}";
                            path.CreateAsDirectory();
                            var json = state.ToJson();
                            using (MemoryStream ms = new MemoryStream())
                            {
                                using (ZipOutputStream compressedzipStream = new ZipOutputStream(ms))
                                {
                                    var data = System.Text.Encoding.UTF8.GetBytes(json);
                                    var entry = new ZipEntry("m");
                                    entry.Size = data.Length;
                                    compressedzipStream.PutNextEntry(entry);
                                    compressedzipStream.Write(data, 0, data.Length);
                                    compressedzipStream.CloseEntry();
                                    compressedzipStream.Close();
                                    File.WriteAllBytes($"{path}/{now.ToString(Time_Format)}.zip", ms.ToArray());
                                }
                            }
                            map[state._id] = now.Ticks;
                        }
                        catch (Exception e)
                        {
                            LOGGER.Error($"备份失败:{state.GetType()} {state._id}");
                            LOGGER.Error(e.ToString());
                        }
                    }
                });
                taskList.Add(task);
            }
            return Task.WhenAll(taskList);
        }

        void clearOldBackup()
        {
            if ((DateTime.Now - oldClearTime).TotalMinutes < Settings.Ins.backupSpan)
                return;
            var folder = new DirectoryInfo(getBackupFolder());
            if (!folder.Exists)
                return;

            //删除过期数据
            //需要最少保留一份已过期的数据
            var now = DateTime.Now;
            oldClearTime = now;
            foreach (var stateDir in folder.GetDirectories())
            {
                foreach (var idDir in stateDir.GetDirectories())
                {
                    // 已过期最近文件
                    DateTime latest = DateTime.MinValue;
                    foreach (var file in idDir.GetFiles())
                    {
                        var timeStr = Path.GetFileNameWithoutExtension(file.Name);
                        var time = DateTime.ParseExact(timeStr, Time_Format, CultureInfo.InvariantCulture);
                        if (time > latest && (now - time).TotalHours > Settings.Ins.backupRemainTime)
                        {
                            latest = time;
                        }
                    }

                    // 如果没有找到，将不会有文件被删除
                    foreach (var file in idDir.GetFiles())
                    {
                        var timeStr = Path.GetFileNameWithoutExtension(file.Name);
                        var time = DateTime.ParseExact(timeStr, Time_Format, CultureInfo.InvariantCulture);
                        if (time < latest)
                            file.Delete();
                    }
                }
            }
        }

        string getBackupFolder()
        {
            return Environment.CurrentDirectory + "/../Backup";
        }

        public void SetNeedRestoreNextStartup()
        {
            File.WriteAllText(NeedRestoreFlagFile, DateTime.Now.ToString());
        }

        public bool IsNeedRestoreBackup()
        {
            return File.Exists(NeedRestoreFlagFile);
        }

        /// <summary>
        /// 如果需要应该在起服的时候同步到数据库
        /// </summary>
        public async Task RestoreToMongoDB(DateTime toTime)
        {
            var folder = new DirectoryInfo(getBackupFolder());
            if (!folder.Exists)
                return;
            
            var db = MongoDBConnection.Singleton.CurDateBase;
            foreach (var stateDir in folder.GetDirectories())
            {
                var stateName = stateDir.Name;
                foreach(var idDir in stateDir.GetDirectories())
                {
                    long.TryParse(idDir.Name, out long id);
                    string filePath = "";
                    DateTime targetTime = DateTime.MinValue;
                    foreach(var file in idDir.GetFiles())
                    {
                        var timeStr = Path.GetFileNameWithoutExtension(file.Name);
                        var time = DateTime.ParseExact(timeStr, Time_Format, CultureInfo.InvariantCulture);
                        if (time <= toTime && time > targetTime)
                        {
                            targetTime = time;
                            filePath = file.FullName;
                        }
                    }

                    if(!string.IsNullOrEmpty(filePath))
                    {
                        try
                        {
                            var data = File.ReadAllBytes(filePath);
                            using (MemoryStream ms = new MemoryStream(data))
                            {
                                using (ZipInputStream zipStream = new ZipInputStream(ms))
                                {
                                    zipStream.IsStreamOwner = true;
                                    var file = zipStream.GetNextEntry();
                                    var after = new byte[file.Size];
                                    zipStream.Read(after, 0, after.Length);
                                    data = after;
                                }
                            }

                            var jsonStr = System.Text.Encoding.UTF8.GetString(data);
                            var doc = BsonDocument.Parse(jsonStr);
                            var col = db.GetCollection<BsonDocument>(stateName);
                            var filter = Builders<BsonDocument>.Filter.Eq(MongoField.UniqueId, id);
                            await col.ReplaceOneAsync(filter, doc);
                        }catch(Exception e)
                        {
                            LOGGER.Error($"备份数据还原失败:" + filePath);
                            LOGGER.Error(e.ToString());
                        }
                    }
                    else
                    {
                        //如果没有则删除数据库中的数据
                        var col = db.GetCollection<BsonDocument>(stateName);
                        var filter = Builders<BsonDocument>.Filter.Eq(MongoField.UniqueId, id);
                        await col.DeleteOneAsync(filter);
                    }
                }
            }
        }
    }
}
