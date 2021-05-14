using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace Geek.Server
{
    public abstract class DBState : InnerDBState
    {
        ///<summary>mongodbId=actorId</summary>
        public long _id;
    }

    //https://github.com/Fody/PropertyChanged/wiki/EventInvokerSelectionInjection
    public abstract class InnerDBState : BaseState, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            _stateChanged = true;
        }
    }


    [BsonIgnoreExtraElements(true, Inherited = true)]//忽略代码删除的字段[数据库多余的字段]
    public abstract class BaseState
    {
        protected bool _stateChanged;
        public virtual bool IsChanged => _stateChanged;
        public virtual void ClearChanges()
        {
            _stateChanged = false;
        }

        #region thread safe save
        volatile int changeVersion;
        volatile int savedVersion;
        volatile int tosaveVersion;

        ///<summary>更新改变</summary>
        public void UpdateChangeVersion()
        {
            if (IsChanged)
                changeVersion++;
            ClearChanges();
        }

        ///<summary>存数据库前先await入队保存要存数据库的change版本</summary>
        public void ReadyToSaveToDB()
        {
            tosaveVersion = changeVersion;
        }

        ///<summary>保存完后修改已保存版本号</summary>
        public void SavedToDB()
        {
            savedVersion = tosaveVersion;
        }

        ///<summary>相对数据库是否有改变</summary>
        public bool IsChangedRefDB(bool updateVersion = false)
        {
            var ret = changeVersion > savedVersion;
            if (!ret && updateVersion)
                return IsChanged;
            return ret;
        }
        #endregion
    }
}
