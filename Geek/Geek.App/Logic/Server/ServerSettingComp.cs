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
using Geek.Core.Component;
using Geek.Core.Storage;
using System.Collections.Generic;

namespace Geek.App.Server
{
    public class ServerSettingState : CacheState
    {
        public bool AllowLogin = true;
        public bool AllowRegister = true;
        public bool AllowRecharge = true;
        public bool GuideState = true;
        public bool IsGMOpen = false;

        //登陆采集(现在人数，注册人数)
        public bool LoginCollection = false;
        public string LoginCollectionUrl;

        public int MaxOnlineNum = 5000;
        public int MaxRegisterNum = 30000;

        //登陆白名单
        public List<string> LoginWhiteList = new List<string>();
        //登陆白名单
        public List<string> LoginBlackList = new List<string>();
    }

    public class ServerSettingComp : StateComponent<ServerSettingState>
    {

    }
}
