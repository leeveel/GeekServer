using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Geek.Server
{
    public class DllLoader
    {
        class HostAssemblyLoadContext : AssemblyLoadContext
        {
            public HostAssemblyLoadContext() : base(true)
            {
            }

            protected override Assembly Load(AssemblyName name)
            {
                return null;
            }
        }

        string dllPath;
        HostAssemblyLoadContext context;
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
