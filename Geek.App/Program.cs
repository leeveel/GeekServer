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
using Geek.Core.Utils;
using NLog.Config;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Geek.App
{
    class Program
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        static Task gameloopTask;
        static void Main(string[] args)
        {
            AppExitHandler.Init(HandleExit);
            Console.WriteLine("init NLog config...");
            NLog.LayoutRenderers.LayoutRenderer.Register<NLogConfigurationLayoutRender>("logConfiguration");
            NLog.LogManager.Configuration = new XmlLoggingConfiguration("Configs/NLog.config");
            NLog.LogManager.AutoShutdown = false;
            Settings.Load("Configs/server_config.json");
            gameloopTask = GameLoop.Enter();
            gameloopTask.Wait();
            NLog.LogManager.Shutdown();
            LOGGER.Info("server exit...\n\n\n");
            Console.WriteLine("server exit...");
        }

        private static bool IsExitCalled = false;
        private static void HandleExit()
        {
            if (IsExitCalled)
                return;
            IsExitCalled = true;
            LOGGER.Info("监听到退出程序消息");
            Settings.Ins.AppRunning = false;
            if (gameloopTask != null)
                gameloopTask.Wait();
            LOGGER.Info("退出程序");
            Process.GetCurrentProcess().Kill();
        }

       
    }
}
