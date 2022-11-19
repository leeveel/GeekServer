using System.Reflection;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Hotfix.Agent;
using Geek.Server.Core.Utils;
using NLog;

namespace Geek.Server.Core.Comps
{
    public static class CompRegister
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// ActorType -> CompTypeList
        /// </summary>
        private static readonly Dictionary<ActorType, HashSet<Type>> ActorCompDic = new();

        /// <summary>
        /// CompType -> ActorType
        /// </summary>
        internal static readonly Dictionary<Type, ActorType> CompActorDic = new();

        /// <summary>
        /// func -> CompTypes
        /// </summary>
        private static readonly Dictionary<int, HashSet<Type>> FuncCompDic = new();

        /// <summary>
        /// CompType -> func
        /// </summary>
        private static readonly Dictionary<Type, short> CompFuncDic = new();

        public static ActorType GetActorType(Type compType)
        {
            CompActorDic.TryGetValue(compType, out var actorType);
            return actorType;
        }

        public static IEnumerable<Type> GetComps(ActorType actorType)
        {
            ActorCompDic.TryGetValue(actorType, out var comps);
            return comps;
        }

        public static Task Init(Assembly assembly = null)
        {
            if (assembly == null)
                assembly = Assembly.GetEntryAssembly();
            Type baseCompName = typeof(BaseComp);
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || !type.IsSubclassOf(baseCompName))
                    continue;

                if (type.GetCustomAttribute(typeof(CompAttribute)) is CompAttribute compAttr)
                {
                    var actorType = compAttr.ActorType;
                    var compTypes = ActorCompDic.GetOrAdd(actorType);
                    compTypes.Add(type);

                    CompActorDic[type] = actorType;

                    if (actorType == ActorType.Role)
                    {
                        if (type.GetCustomAttribute(typeof(FuncAttribute)) is FuncAttribute funcAttr)
                        {
                            var set = FuncCompDic.GetOrAdd(funcAttr.func);
                            set.Add(type);
                            CompFuncDic[type] = funcAttr.func;
                        }
                    }
                }
                else
                {
                    throw new Exception($"comp:{type.FullName}未绑定actor类型");
                }
            }
            Log.Info($"初始化组件注册完成");
            return Task.CompletedTask;
        }

        public static async Task ActiveGlobalComps()
        {
            try
            {
                foreach (var kv in ActorCompDic)
                {
                    var actorType = kv.Key;
                    foreach (var compType in kv.Value)
                    {
                        var agentType = HotfixMgr.GetAgentType(compType);
                        if (agentType == null)
                        {
                            throw new Exception($"{compType}未实现agent");
                        }

                        //if (actorType > ActorType.Separator)
                        //{
                        //    Log.Info($"激活全局组件：{actorType} {compType}");
                        //    await ActorMgr.GetCompAgent(agentType, actorType);
                        //}
                    }
                    if (actorType > ActorType.Separator)
                    {
                        Log.Info($"激活全局Actor: {actorType}");
                        await ActorMgr.GetOrNew(IdGenerator.GetActorID(actorType));
                    }
                }
                Log.Info($"激活全局组件并检测组件是否都包含Agent实现完成");
            }
            catch (Exception)
            {
                Log.Error($"激活全局组件并检测组件是否都包含Agent实现失败");
                throw;
            }
        }

        public static Task ActiveRoleComps(ICompAgent compAgent, HashSet<short> openFuncSet)
        {
            return ActiveComps(compAgent.Owner.Actor,
                t => !CompFuncDic.TryGetValue(t, out var func)
                || openFuncSet.Contains(func));
            //foreach (var compType in GetComps(ActorType.Role))
            //{
            //    bool active;
            //    if (CompFuncDic.TryGetValue(compType, out var func))
            //    {
            //        active = openFuncSet.Contains(func);
            //    }
            //    else
            //    {
            //        active = true;
            //    }
            //    if (active)
            //    {
            //        var agentType = HotfixMgr.GetAgentType(compType);
            //        await compAgent.GetCompAgent(agentType);
            //    }
            //}
        }

        internal static async Task ActiveComps(Actor actor, Func<Type, bool> predict = null)
        {
            foreach (var compType in GetComps(actor.Type))
            {
                if (predict == null || predict(compType))
                {
                    var agentType = HotfixMgr.GetAgentType(compType);
                    await actor.GetCompAgent(agentType);
                }
            }
        }

        internal static BaseComp NewComp(Actor actor, Type compType)
        {
            if (!ActorCompDic.TryGetValue(actor.Type, out var compTypes) || !compTypes.Contains(compType))
            {
                throw new Exception($"获取不属于此actor：{actor.Type}的comp:{compType.FullName}");
            }
            var comp = (BaseComp)Activator.CreateInstance(compType);
            comp.Actor = actor;
            return comp;
        }
    }
}
