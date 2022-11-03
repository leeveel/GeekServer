using System.Text;
using RocksDbSharp;

namespace Geek.Server
{
    public class Transaction
    {
        EmbeddedDB db;
        WriteBatch batch;
        IRemoteTransaction remoteTrans;
        public Transaction(EmbeddedDB db)
        {
            this.db = db;
            batch = new WriteBatch();
            remoteTrans = db.remoteBackup.StartTransaction();
        }

        public void Set(string table, string key, byte[] value, ColumnFamilyHandle cfHandle)
        {
            batch.Put(Encoding.UTF8.GetBytes(key), value, cfHandle);
            remoteTrans.Set(table, key, value);
        }

        public void Delete(string table, string key, ColumnFamilyHandle cfHandle)
        {
            batch.Delete(Encoding.UTF8.GetBytes(key), cfHandle);
            remoteTrans.Delete(table, key);
        }

        public void Commit()
        {
            db.InnerDB.Write(batch);
            batch.Dispose();
            remoteTrans.Commit();
        }
    }
}