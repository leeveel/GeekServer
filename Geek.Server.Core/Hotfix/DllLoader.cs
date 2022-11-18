using System.Reflection;
using System.Runtime.Loader;

namespace Geek.Server.Core.Hotfix
{
    internal class DllLoader
    {
        public DllLoader(string dllPath)
        {
            Context = new HostAssemblyLoadContext();
            HotfixDll = Context.LoadFromAssemblyPath(dllPath);
        }

        public Assembly HotfixDll { get; }

        private HostAssemblyLoadContext Context { get; }

        public WeakReference Unload()
        {
            Context.Unload();
            return new WeakReference(Context);
        }

        class HostAssemblyLoadContext : AssemblyLoadContext
        {
            public HostAssemblyLoadContext() : base(true) { }

            protected override Assembly Load(AssemblyName assemblyName)
            {
                return null;
            }
        }

    }
}
