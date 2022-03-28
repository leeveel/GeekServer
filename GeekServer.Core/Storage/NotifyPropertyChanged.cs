using System.Collections.Generic;

namespace Geek.Server
{
    public class NotifyPropertyChanged
    {
        protected HashSet<BaseDBState> stList = new HashSet<BaseDBState>();
        protected bool _stateChanged;
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
    }
}
