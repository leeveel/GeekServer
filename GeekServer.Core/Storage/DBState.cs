using System.Threading;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using System;

//此文件的类型会在编译时自动注入，以最低代价获取状态是否改变
//不要随意改变类名和命名空间
namespace Geek.Server
{
    public abstract class DBState : BaseDBState
    {
        ///<summary>mongodbId=actorId<para/>
        ///需要注意mongodb不区分_id/id/Id/ID
        ///</summary>
        public long Id { get; set; }
    }



    [BsonIgnoreExtraElements(true, Inherited = true)]//忽略代码删除的字段[数据库多余的字段]
    public abstract class BaseDBState
    {
        protected HashSet<BaseDBState> stList = new HashSet<BaseDBState>();
        protected bool _stateChanged;
        public const string StateSuffix = "Wrapper";

        public virtual bool IsChanged
        {
            get
            {
                if (_stateChanged)
                    return _stateChanged;
                foreach (var st in stList)
                {
                    if (st != null && st.IsChanged)
                        return true;
                }
                return false;
            }
        }

        /// <summary>需要在actor线程内部调用才安全</summary>
        public virtual void ClearChanges()
        {
            _stateChanged = false;
            foreach (var st in stList)
            {
                if (st != null)
                    st.ClearChanges();
            }
        }

        public static BaseDBState CreateStateWrapper<T>() where T : BaseDBState
        {
            Type self = typeof(T);
            var wrapperType = self.Assembly.GetType(self.FullName + StateSuffix);
            return (BaseDBState)Activator.CreateInstance(wrapperType);
        }

        public static string WrapperFullName<T>() where T : BaseDBState
        {
            return typeof(T).FullName + StateSuffix;
        }


        #region debug state
#if DEBUG_MODE
        protected Geek.Server.WorkerActor CompActor;
        public void SetCompActor(Geek.Server.WorkerActor actor)
        {
            CompActor = actor;
            foreach (var state in stList)
                state.SetCompActor(actor);
        }

        public static bool CheckCallChainEnable = true;
        public void CheckIsInCompActorCallChain()
        {
            if (CompActor == null)
                return;
            if (!CheckCallChainEnable)
                return;
            long callChainId = Geek.Server.RuntimeContext.Current;
            if (callChainId > 0 && (callChainId == CompActor.curCallChainId))
                return;
            throw new System.Exception($"callChainId={callChainId} actorCallChainId={CompActor.curCallChainId} 当前调用不在State所属Comp的Actor调用链上，可能存在多线程问题。{GetType()}");
        }
#endif
        #endregion

        #region thread safe save
        volatile int changeVersion;
        volatile int savedVersion;
        volatile int tosaveVersion;

        ///<summary>存数据库前先await入队保存要存数据库的change版本</summary>
        public void ReadyToSaveToDB()
        {
            Interlocked.Exchange(ref tosaveVersion, changeVersion);
        }

        ///<summary>保存完后修改已保存版本号</summary>
        public void SavedToDB()
        {
            Interlocked.Exchange(ref savedVersion, tosaveVersion);
        }

        ///<summary>相对数据库是否有改变</summary>
        public bool IsChangedComparedToDB()
        {
            if (IsChanged)
            {
                Interlocked.Increment(ref changeVersion);
                ClearChanges();
            }
            return changeVersion > savedVersion;
        }
        #endregion
    }
}
