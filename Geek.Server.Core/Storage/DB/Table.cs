using System.Collections;
using System.Text;
using Geek.Server.Core.Serialize;
using NLog;
using RocksDbSharp;

namespace Geek.Server.Core.Storage.DB
{
    public class Table<T> : IEnumerable<T>
    {
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();
        protected EmbeddedDB db;
        ColumnFamilyHandle cfHandle;
        string tableName;
        bool isRawTable;

        internal Table(EmbeddedDB db, string name, ColumnFamilyHandle cfHandle, bool isRawTable = false)
        {
            this.db = db;
            this.cfHandle = cfHandle;
            this.isRawTable = isRawTable;
            tableName = name;
        }

        public void Set<IDType>(IDType id, T value, Transaction trans = null)
        {
            var data = Serializer.Serialize<T>(value);
            SetRaw(id, data);
        }

        public void SetRaw<IDType>(IDType id, byte[] data, Transaction trans = null)
        {
            var mainId = GetDBKey(id);
            if (trans != null)
            {
                trans.Set(mainId, data, cfHandle);
            }
            else
            {
                db.InnerDB.Put(Encoding.UTF8.GetBytes(mainId), data, cfHandle);
            }
        }

        public void SetBatch<IDType>(List<IDType> ids, List<T> values, Transaction trans = null)
        {
            var valueList = new List<byte[]>(values.Count);
            foreach (var value in values)
            {
                var mainId = GetDBKey(value);
                if (string.IsNullOrEmpty(mainId))
                {
                    throw new NotFindKeyException($"no KeyAttribute find in {tableName}");
                }
                valueList.Add(Serializer.Serialize<T>(value));
            }
            SetRawBatch(ids, valueList, trans);
        }

        public void SetRawBatch<IDType>(List<IDType> ids, List<byte[]> values, Transaction trans = null)
        {
            var batch = trans == null ? new WriteBatch() : null;
            var count = values.Count;
            var keyList = new List<string>(count);
            foreach (var value in ids)
            {
                var mainId = GetDBKey(value);
                keyList.Add(mainId);
            }

            for (int i = 0; i < count; i++)
            {
                var mainId = keyList[i];
                var data = values[i];
                if (batch != null)
                {
                    var id = Encoding.UTF8.GetBytes(mainId);
                    batch.Put(id, data, cfHandle);
                }
                else
                {
                    trans.Set(mainId, data, cfHandle);
                }
            }

            if (trans == null)
            {
                db.InnerDB.Write(batch);
                batch.Dispose();
            }
        }

        public void Delete<IDType>(IDType id, Transaction trans = null)
        {
            var mainId = GetDBKey(id);

            if (trans != null)
            {
                trans.Delete(mainId, cfHandle);
            }
            else
            {
                db.InnerDB.Remove(Encoding.UTF8.GetBytes(mainId), cfHandle);
            }
        }

        public void DeleteBatch<IDType>(List<IDType> ids, Transaction trans = null)
        {
            var batch = trans == null ? new WriteBatch() : null;
            foreach (var id in ids)
            {
                var mainId = GetDBKey(id);
                if (batch != null)
                {
                    batch.Delete(Encoding.UTF8.GetBytes(mainId), cfHandle);
                }
                else
                {
                    trans.Delete(mainId, cfHandle);
                }
            }
            if (trans == null)
            {
                db.InnerDB.Write(batch);
                batch.Dispose();
            }
        }

        public T Get<IDType>(IDType id)
        {
            var data = db.InnerDB.Get(Encoding.UTF8.GetBytes(GetDBKey(id)), cfHandle);
            if (data == null)
            {
                return default;
            }
            if (isRawTable)
            {
                return (T)(object)data;
            }
            return Serializer.Deserialize<T>(data);
        }

        public List<T> GetAll()
        {
            return this.ToList();
        }

        protected string GetDBKey<IDType>(IDType id)
        {
            return id.ToString();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public class Enumerator : IEnumerator<T>
        {
            //private Snapshot snapshot;
            private Iterator dbIterator;
            private T currValue = default(T);
            private Table<T> table;

            internal Enumerator(Table<T> table)
            {
                this.table = table;
                var option = new ReadOptions();
                dbIterator = table.db.InnerDB.NewIterator(table.cfHandle, option);
                dbIterator.SeekToFirst();
            }

            private bool isDisposed = false;
            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            void Dispose(bool disposing)
            {
                if (!isDisposed)
                {
                    currValue = default(T);
                    if (dbIterator != null)
                    {
                        dbIterator.Dispose();
                        dbIterator = null;
                    }
                }
                isDisposed = true;
            }

            ~Enumerator()
            {
                Dispose(disposing: false);
            }

            public bool MoveNext()
            {
                if (dbIterator.Valid())
                {
                    if (table.isRawTable)
                        currValue = (T)(object)dbIterator.Value();
                    else
                        currValue = Serializer.Deserialize<T>(dbIterator.Value());
                    dbIterator.Next();
                    return true;
                }
                return false;
            }


            public T Current => currValue;

            object IEnumerator.Current
            {
                get
                {
                    return currValue;
                }
            }

            void IEnumerator.Reset()
            {
                dbIterator.SeekToFirst();
            }
        }
    }
}