using System;
using System.Collections.Generic;

namespace Geek.Server
{
    public class ActorLimit
    {
        internal struct CallInfo
        {
            public int entityType;
            public Type compType;
            public CallInfo(int entityType, Type compType = null)
            {
                this.entityType = entityType;
                this.compType = compType;
            }

            public bool IsEqual(CallInfo other)
            {
                return entityType == other.entityType && compType == other.compType;
            }
        }

        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        //key可以调用value-list类型
        static Dictionary<CallInfo, List<CallInfo>> limitMap = new Dictionary<CallInfo, List<CallInfo>>();
        public static void RegistCall(Enum callEntityType, Enum beCallEntityType)
        {
            Regist((int)(object)callEntityType, null, (int)(object)beCallEntityType, null);
        }

        public static void RegistCall<CallType>(Enum callEntityType, Enum beCallEntityType)
        {
            Regist((int)(object)callEntityType, typeof(CallType), (int)(object)beCallEntityType, null);
        }

        public static void RegistCall<CallType, BeCallType>(Enum callEntityType, Enum beCallEntityType)
        {
            Regist((int)(object)callEntityType, typeof(CallType), (int)(object)beCallEntityType, typeof(BeCallType));
        }

        public static void RegistBeCall<BeCallType>(Enum callEntityType, Enum beCallEntityType)
        {
            Regist((int)(object)callEntityType, null, (int)(object)beCallEntityType, typeof(BeCallType));
        }

        static void Regist(int callEntityType, Type callCompType, int beCallEntityType, Type beCallCompType)
        {
#if DEBUG_MODE
            if(callEntityType == beCallEntityType && callCompType == null && beCallCompType == null)
                LOGGER.Error($"共享actor的实体不能注册自己调用自己。{callEntityType}");

            var info1 = new CallInfo(callEntityType, callCompType);
            limitMap.TryGetValue(info1, out var list);
            if(list == null)
            {
                list = new List<CallInfo>();
                limitMap[info1] = list;
            }
            var info2 = new CallInfo(beCallEntityType, beCallCompType);
            if(!list.Contains(info2))
                list.Add(info2);
#endif
        }

        static bool regChecked;
        public static void CheckRegist()
        {
#if DEBUG_MODE
            regChecked = true;
            var errList = new List<CallInfo>();
            var infoList = new List<CallInfo>();
            foreach (var kv in limitMap)
            {
                if (!infoList.Contains(kv.Key))
                    infoList.Add(kv.Key);
                foreach (var item in kv.Value)
                {
                    if (!infoList.Contains(item))
                        infoList.Add(item);
                }
            }

            bool checkTopOnly = true;
            if (checkTopOnly)
            {
                //只检测一层, 注册a->b, b->c时允许a->b->c
                foreach (var info1 in infoList)
                {
                    if (errList.Contains(info1))
                        continue;
                    limitMap.TryGetValue(info1, out var list);
                    if (list == null)
                        continue;
                    foreach (var info2 in list)
                    {
                        if (info1.IsEqual(info2))
                            continue;
                        if (errList.Contains(info2))
                            continue;
                        limitMap.TryGetValue(info2, out var list2);
                        if (list2 == null)
                            continue;
                        if (list2.Contains(info1))
                        {
                            errList.Add(info1);
                            errList.Add(info2);
                            LOGGER.Error($"entity调用关系只能是单向的，不能直接相互调用。{info1.entityType}.{info1.compType}&{info2.entityType}.{info2.compType}");
                            break;
                        }
                    }
                }
            }
            else
            {
                //多层检测
                //是否可以从a调用到b
                Func<CallInfo, CallInfo, List<CallInfo>, bool> canCallA2B = null;
                canCallA2B = (a, b, excludeList) => {
                    limitMap.TryGetValue(a, out var aList);
                    if (aList == null)
                        return false;
                    foreach(var item in aList)
                    {
                        if (excludeList.Contains(item))
                            continue;
                        if (item.IsEqual(b))
                            return true;

                        excludeList.Add(item);
                        var ret = canCallA2B(item, b, excludeList);
                        if (ret) return true;
                    }
                    return false;
                };

                var copyList = new List<CallInfo>();
                copyList.AddRange(infoList);
                foreach (var info1 in infoList)
                {
                    if (errList.Contains(info1))
                        continue;
                    foreach(var info2 in copyList)
                    {
                        if (info1.IsEqual(info2))
                            continue;
                        if(canCallA2B(info1, info2, new List<CallInfo>()) && canCallA2B(info2, info1, new List<CallInfo>()))
                        {
                            errList.Add(info1);
                            errList.Add(info2);
                            LOGGER.Error($"entity调用关系只能是单向的，不能直接或者间接相互调用。{info1.entityType}.{info1.compType}&{info2.entityType}.{info2.compType}");
                        }
                    }
                }
            }
#endif
        }

        public static bool AllowCall(int callEntityType, Type callCompType, int beCallEnityType, Type beCallCompType)
        {
#if DEBUG_MODE
            if (!regChecked)
                CheckRegist();

            var info1 = new CallInfo(callEntityType, callCompType);
            var info2 = new CallInfo(beCallEnityType, beCallCompType);
            limitMap.TryGetValue(info1, out var list);
            return list != null && list.Contains(info2);
#else
            return true;
#endif
        }
    }
}
