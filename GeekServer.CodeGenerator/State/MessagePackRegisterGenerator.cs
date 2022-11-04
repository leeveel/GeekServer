
using Microsoft.CodeAnalysis;
using Scriban;
using System;
using System.Collections.Generic;
using Tools.Utils;

namespace Geek.Server
{
    [Generator]
    public class MessagePackRegisterGenerator : ISourceGenerator
    {
        const string baseDBStateName = "InnerState";
        const string dBStateName = "CacheState";
        public void Initialize(GeneratorInitializationContext context)
        {
            //Debugger.Launch();
            ResLoader.LoadDll();
            context.RegisterForSyntaxNotifications(() => new DBStateFilter());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is DBStateFilter receiver)
            {
                var str = ResLoader.LoadTemplate("PolymorphicDBStateRegister.liquid");
                Template agentTemplate = Template.Parse(str);

                var inheritDBBaseMap = new Dictionary<string, string>();
                foreach (var v in receiver.classFullNamePair)
                {
                    var c = GetInheritDBBase(receiver.classPair, v.Key, v.Key);
                    if (c != "")
                    {
                        inheritDBBaseMap[v.Key] = c;
                    }
                }

                var assemblyName = context.Compilation.AssemblyName;
                if (assemblyName == null)
                    assemblyName = "";
                var arr = new PolymorphicInfoArray();
                arr.prefix = assemblyName.Replace(".", "");

                foreach (var v in inheritDBBaseMap)
                {
                    var keyFullName = GetFullName(receiver.classFullNamePair, v.Key);
                    AddInfo(arr, keyFullName, keyFullName);
                    AddGetAllSubClass(arr, receiver.classFullNamePair, inheritDBBaseMap, v.Key, v.Key);
                }

                RemoveFinalClass(arr, inheritDBBaseMap, receiver.classFullNamePair);

                var source = agentTemplate.Render(arr);
                //File.WriteAllText($"PolymorphicDBStateRegister.cs", source);
                context.AddSource($"{arr.prefix}PolymorphicDBStateRegister.g.cs", source);
            }
        }

        string GetInheritDBBase(Dictionary<string, List<string>> classPair, string startClassName, string currentClassName)
        {
            if (classPair.TryGetValue(currentClassName, out var list))
            {
                foreach (var l in list)
                {
                    if (l == baseDBStateName)
                    {
                        return currentClassName == startClassName ? baseDBStateName : currentClassName;
                    }
                    if (l == dBStateName)
                    {
                        return currentClassName == startClassName ? dBStateName : currentClassName;
                    }
                    var str = GetInheritDBBase(classPair, startClassName, l);
                    if (str != "")
                        return str;
                }
            }
            return "";
        }

        string GetFullName(Dictionary<string, string> nameMap, string name)
        {
            if (nameMap.TryGetValue(name, out var outName))
            {
                return outName;
            }
            if (name == baseDBStateName)
            {
                return "Geek.Server.InnerState";
            }
            if (name == dBStateName)
            {
                return "Geek.Server.CacheState";
            }
            return "";
        }

        void AddGetAllSubClass(PolymorphicInfoArray arr, Dictionary<string, string> fullNameMap, Dictionary<string, string> inheritDBBaseMap, string startClass, string curClass)
        {
            var baseName = GetFullName(fullNameMap, startClass);
            while (true)
            {
                if (inheritDBBaseMap.TryGetValue(curClass, out var inher))
                {
                    var subname = GetFullName(fullNameMap, inher);
                    if (arr.infos.Find(v => v.basename == baseName && v.subname == subname) != null)
                    {
                        return;
                    }
                    AddInfo(arr, baseName, subname);
                    if (inher == baseDBStateName || inher == dBStateName)
                    {
                        return;
                    }
                    AddGetAllSubClass(arr, fullNameMap, inheritDBBaseMap, startClass, inher);
                }
            }
        }

        void AddInfo(PolymorphicInfoArray arr, string baseName, string subName)
        {
            //if (baseName != subName)
            arr.infos.Add(new PolymorphicInfo { basename = baseName, subname = subName, subsid = ((int)MurmurHash3.Hash(baseName, 27)).ToString() });
            //arr.infos.Add(new PolymorphicInfo { basename = baseName, subname = subName, subsid = ((int)xxHash32.ComputeHash(nameBytes, 0, nameBytes.Length, 27)).ToString() });
        }

        void RemoveFinalClass(PolymorphicInfoArray arr, Dictionary<string, string> inheritDBBaseMap, Dictionary<string, string> classFullNamePair)
        {
            var all = arr.infos.ToArray();
            foreach (var c in all)
            {
                if (c.basename == c.subname)
                {
                    bool needRemove = true;
                    foreach (var kv in inheritDBBaseMap)
                    {
                        if (c.basename == GetFullName(classFullNamePair, kv.Value))
                        {
                            needRemove = false;
                            break;
                        }
                    }
                    if (needRemove)
                    {
                        arr.infos.Remove(c);
                    }
                }
            }
        }
    }

}