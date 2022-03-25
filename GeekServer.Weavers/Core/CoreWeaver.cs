using Fody;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace Weavers
{
    public class CoreWeaver : BaseModuleWeaver
    {
        public override void Execute()
        {
            WriteMessage($"CoreWeaver Start：{ModuleDefinition.Assembly.Name}", MessageImportance.High);

            //Actor属性改为public
            var actorList = new List<string>() {
                "Geek.Server.BaseComponent",
                "Geek.Server.BaseComponentAgent`1",
                "Geek.Server.QueryComponentAgent`1"
            };

            var actorType = "Geek.Server.BaseActor";
            foreach (var typeDef in ModuleDefinition.GetTypes())
            {
                if (actorList.Contains(typeDef.FullName))
                {
                    var getActorMethodDef = typeDef.Methods.FirstOrDefault(md => md.Name == "get_Actor");
                    if (getActorMethodDef == null)
                    {
                        WriteError($"获取{typeDef.FullName}.Actor属性失败");
                        return;
                    }
                    getActorMethodDef.IsPublic = true;
                }

                if(typeDef.FullName == actorType)
                {
                    //baseActor函数改为public
                    foreach(var mthDef in typeDef.Methods)
                    {
                        var mthName = mthDef.Name;
                        if (mthName == "IsNeedEnqueue"
                            || mthName == "NewChainId"
                            || mthName == "Enqueue")
                            mthDef.IsPublic = true;
                    }
                }
            }

            WriteMessage($"CoreWeaver End：{ModuleDefinition.Assembly.Name}", MessageImportance.High);
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "System";
            yield return "mscorlib";
            yield return "System.Collections";
        }
    }
}
