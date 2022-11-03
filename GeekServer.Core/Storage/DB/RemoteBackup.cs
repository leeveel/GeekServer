
using System;
using RocksDbSharp;

namespace Geek.Server
{
    public class IRemoteTransaction
    {
        public virtual void Set(string tablename, string key, byte[] value)
        {

        }
        public virtual void Delete(string tablename, string key)
        {

        }

        public virtual void Commit()
        {

        }
    }
    public class IRemoteBackup
    {
        public virtual IRemoteTransaction StartTransaction()
        {
            return new IRemoteTransaction();
        }

        public virtual void Set(string tablename, string key, byte[] value)
        {

        }
        public virtual void Delete(string tablename, string key)
        {

        }
        public virtual void SetBatch(string tablename, List<string> keys, List<byte[]> datas)
        {

        }

        public virtual void Flush()
        {

        }
    }
}