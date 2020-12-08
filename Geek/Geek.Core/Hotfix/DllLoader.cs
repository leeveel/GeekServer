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
using System.Reflection;
using System.Runtime.Loader;

namespace Geek.Core.Hotfix
{
    public class DllLoader
    {
        class HostAssemblyLoadContext : AssemblyLoadContext
        {
            public HostAssemblyLoadContext() : base(isCollectible: true)
            {

            }

            protected override Assembly Load(AssemblyName name)
            {
                return null;
            }
        }

        public string dllPath;
        private HostAssemblyLoadContext context;
        public Assembly HotfixDll { private set; get; }

        public DllLoader(string dllPath)
        {
            this.dllPath = dllPath;
            context = new HostAssemblyLoadContext();
        }

        public void Load()
        {
            HotfixDll = context.LoadFromAssemblyPath(dllPath);
        }

        public WeakReference Unload()
        {
            context.Unload();
            return new WeakReference(context);
        }
    }
}
