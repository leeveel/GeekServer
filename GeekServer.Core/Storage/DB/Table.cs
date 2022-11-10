using System.Text;
using RocksDbSharp;
using NLog;
using System.Collections;

namespace Geek.Server
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
            if (!db.InnerDB.TryGetColumnFamily(tableName, out cfHandle))
            {
                var option = new ColumnFamilyOptions();
                cfHandle = db.InnerDB.CreateColumnFamily(option, tableName);
            }
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
                trans.Set(tableName, mainId, data, cfHandle);
            }
            else
            {
                db.InnerDB.Put(Encoding.UTF8.GetBytes(mainId), data, cfHandle);
                db.remoteBackup.Set(tableName, mainId, data);
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
                    trans.Set(tableName, mainId, data, cfHandle);
                }
            }

            if (trans == null)
            {
                db.InnerDB.Write(batch);
                db.remoteBackup.SetBatch(tableName, keyList, values);
            }
        }

        public void Delete<IDType>(IDType id, Transaction trans = null)
        {
            var mainId = GetDBKey(id);

            if (trans != null)
            {
                trans.Delete(tableName, mainId, cfHandle);
            }
            else
            {
                db.InnerDB.Remove(Encoding.UTF8.GetBytes(mainId), cfHandle);
                db.remoteBackup.Delete(tableName, mainId);
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
            private T _current = default(T);
            private Table<T> table;

            internal Enumerator(Table<T> table)
            {
                this.table = table;
                //snapshot = table.db.InnerDB.CreateSnapshot();
                //var option = new ReadOptions().SetSnapshot(snapshot); 
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
                    _current = default(T);
                    if (dbIterator != null)
                    {
                        dbIterator.Dispose();
                        //snapshot.Dispose();
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
                        _current = (T)(object)dbIterator.Value();
                    else
                        _current = Serializer.Deserialize<T>(dbIterator.Value());
                    dbIterator.Next();
                    return true;
                }
                return false;
            }


            public T Current => _current;

            object IEnumerator.Current
            {
                get
                {
                    return _current;
                }
            }

            void IEnumerator.Reset()
            {
                dbIterator.SeekToFirst();
            }
        }
    }
}