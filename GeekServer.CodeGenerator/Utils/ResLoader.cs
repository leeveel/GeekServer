using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Tools.Utils
{
    public class ResLoader
    {

        public static string LoadTemplate(string resourceName)
        {
            string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            resourceName = $"GeekServer.CodeGenerator.Template.{resourceName}";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    byte[] assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return System.Text.Encoding.UTF8.GetString(assemblyData);
                }
            }
            return null;
        }

        public static void LoadDll()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
            {
                AssemblyName name = new AssemblyName(args.Name);
                Assembly loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().FullName == name.FullName);
                if (loadedAssembly != null)
                {
                    return loadedAssembly;
                }

                string resourceName = $"GeekServer.CodeGenerator.{name.Name}.dll";
                using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (resourceStream == null)
                    {
                        return null;
                    }
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        resourceStream.CopyTo(memoryStream);
                        return Assembly.Load(memoryStream.ToArray());
                    }
                }
            };
        }


    }
}
