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
using Geek.Core.Hotfix;
using Geek.App.Net;
using System;
using Geek.Core.Net.Http;
using Geek.Core.Storage;
using System.Threading.Tasks;
using Geek.Core.Net.Handler;
using Geek.Core.Actor;
using Geek.App.Server;
using Geek.App.Session;

namespace Geek.Hotfix.Logic
{
    public class HotfixBridge : IHotfixBridge
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public Task Start()
        {
            LOGGER.Info("start server...");
            HttpHandlerFactory.SetExtraHandlerGetter(HotfixMgr.GetHttpHandler);
            TcpHandlerFactory.SetExtraHandlerGetter(HotfixMgr.GetTcpMsg, HotfixMgr.GetTcpHandler);

            LogicServer.Singleton.Start(Settings.Ins.tcpPort, Settings.Ins.useLibuv);
            HttpServer.Start(Settings.Ins.httpPort);

            try
            {
                LOGGER.Info($"connect mongo {Settings.Ins.mongoDB} {Settings.Ins.mongoUrl}...");
                MongoDBConnection.Singleton.Connect(Settings.Ins.mongoDB, Settings.Ins.mongoUrl);
            }
            catch (Exception e)
            {
                LOGGER.Fatal(e.ToString());
            }

            LOGGER.Info("regist components...");
            ComponentTools.RegistAll();

            LOGGER.Info("index mongodb...");
            //await MongoDBConnection.Singleton.IndexCollectoinMore<RoleState>(MongoField.Name);
            return Task.CompletedTask;
        }

        public async Task OnLoadSucceed(bool isReload)
        {
            if (isReload)
                return;

            LOGGER.Info("active server actor " + Settings.Ins.serverId);
            await ActorManager.GetOrNew<ServerActor>(ServerActorID.Normal);
        }

        public Task Reload()
        {
            LOGGER.Info("reload hotfix");
            HttpHandlerFactory.SetExtraHandlerGetter(HotfixMgr.GetHttpHandler);
            TcpHandlerFactory.SetExtraHandlerGetter(HotfixMgr.GetTcpMsg, HotfixMgr.GetTcpHandler);

            return Task.CompletedTask;
        }

        public async Task Stop()
        {
            LOGGER.Info("stop hotfix");
            await SessionManager.RemoveAll();
            await DBActor.Singleton.OnShutdownSave();
            await ActorManager.RemoveAll();
        }
    }
}
