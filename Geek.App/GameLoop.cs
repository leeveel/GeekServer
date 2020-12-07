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
using Geek.Core.Hotfix;
using System.Threading.Tasks;
using Geek.Core.Storage;

public class GameLoop
{
    static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
    public static async Task Enter()
    {
        //开服时间设定
        try
        {
            if (BackupMgr.Singleton.IsNeedRestoreBackup())
            {
                LOGGER.Info("上次关服可能存在回存失败的情况，请确认后重新起服");
                await Task.Delay(5000);
                return;
            }

            if(Settings.Ins.restoreToTime > new DateTime(2020, 1, 1))
            {
                //回档
                LOGGER.Info("检测到回档配置，开始执行回档操作");
                MongoDBConnection.Singleton.Connect(Settings.Ins.mongoDB, Settings.Ins.mongoUrl);
                await BackupMgr.Singleton.RestoreToMongoDB(Settings.Ins.restoreToTime);
                LOGGER.Info("数据已回档到mongodb，请确认后重新起服");
                await Task.Delay(5000);
                return;
            }

            await HotfixMgr.ReloadModule("");
            Settings.Ins.StartServerTime = DateTime.Now;
            Settings.Ins.AppRunning = true;
            LOGGER.Info("enter game loop...");
            Console.WriteLine("enter game loop...");
            int gcTime = 0;
            int saveTime = 0;
            int deltaTime = 1000;
            int del = 0;
            while (Settings.Ins.AppRunning)
            {
                _ = ActorManager.Tick(deltaTime);

                //test hotfix
                if (Settings.Ins.isDebug)
                {
                    del += 1;
                    if (del > 10) //10秒
                    {
                        del = 0;
                        //_ = HotfixMgr.ReloadModule("");
                    }
                }

                //定时回存
                saveTime += deltaTime;
                if (saveTime / 1000 > Settings.Ins.dataFlushTimeMax)
                {
                    saveTime = 0;
                    _ = DBActor.Singleton.TimerSave();
                }

                //gc
                gcTime += deltaTime;
                if (gcTime / 1000 > 2000)//33分钟60*33=1980
                {
                    gcTime = 0;
                    System.GC.Collect();
                }
                await Task.Delay(1000);
            }
        }catch(Exception e)
        {
            Console.WriteLine("执行 Exception");
            LOGGER.Fatal(e.ToString());
        }

        LOGGER.Info("退出GameLoop 本次开服时间：" + (DateTime.Now - Settings.Ins.StartServerTime));
        Console.WriteLine("退出GameLoop 本次开服时间：" + (DateTime.Now - Settings.Ins.StartServerTime));
        await HotfixMgr.Stop();
        LOGGER.Info("hotfix stop end");
        Console.WriteLine("hotfix stop end");
    }
}
