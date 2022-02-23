using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

//此文件的类型会在编译时自动注入，以最低代价获取状态是否改变
//不要随意改变类名和命名空间
namespace Geek.Server
{
    public abstract class DBState : InnerDBState
    {
        ///<summary>mongodbId=actorId<para/>
        ///需要注意mongodb不区分_id/id/Id/ID
        ///</summary>
        public long Id { get; set; }
    }

    public abstract class InnerDBState : BaseState
    {
        protected HashSet<BaseState> stList = new HashSet<BaseState>();

        public HashSet<string> changedSet = new HashSet<string>();

        public override bool IsChanged
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

        public override void ClearChanges()
        {
            _stateChanged = false;
            changedSet.Clear();
            foreach (var st in stList)
            {
                if (st != null)
                    st.ClearChanges();
            }
        }
    }

    [BsonIgnoreExtraElements(true, Inherited = true)]//忽略代码删除的字段[数据库多余的字段]
    public abstract class BaseState
    {
        protected bool _stateChanged;
        /// <summary>需要在actor线程内部调用才安全</summary>
        public virtual bool IsChanged => _stateChanged;
        /// <summary>需要在actor线程内部调用才安全</summary>
        public virtual void ClearChanges()
        {
            _stateChanged = false;
        }
    }
}
