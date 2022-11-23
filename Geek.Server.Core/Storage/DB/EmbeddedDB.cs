using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using RocksDbSharp;

namespace Geek.Server.Core.Storage.DB
{
    /// <summary>
    /// 内嵌数据库-基于RocksDB
    /// </summary>
    public class EmbeddedDB
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public RocksDb InnerDB { get; private set; }
        public string DbPath { get; private set; } = "";
        public string SecondPath { get; private set; } = "";
        public bool ReadOnly { get; private set; } = false; 
        protected FlushOptions flushOption;
        protected ConcurrentDictionary<string, ColumnFamilyHandle> columnFamilie = new ConcurrentDictionary<string, ColumnFamilyHandle>();
  
        public EmbeddedDB(string path, bool readOnly = false, string readonlyPath=null)
        {
            this.ReadOnly = readOnly;
            var dir = Path.GetDirectoryName(path);
            if(!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            DbPath = path;
            var option = new DbOptions();
            RocksDb.TryListColumnFamilies(option, DbPath, out var cfList);
            var cfs = new ColumnFamilies();

            foreach (var cf in cfList)
            {
                cfs.Add(cf, new ColumnFamilyOptions());
                columnFamilie[cf] = null;
            }

            if (readOnly)
            {
                option.SetMaxOpenFiles(-1);
                if (string.IsNullOrEmpty(readonlyPath))
                    SecondPath = DbPath + "_$$$";
                else
                    SecondPath = readonlyPath;
                InnerDB = RocksDb.OpenAsSecondary(option, DbPath, SecondPath, cfs);
            }
            else
            {
                flushOption = new FlushOptions();
                option.SetCreateIfMissing(true).SetCreateMissingColumnFamilies(true);
                InnerDB = RocksDb.Open(option, DbPath, cfs);
            }

        }

        ColumnFamilyHandle GetOrCreateColumnFamilyHandle(string name)
        {
            lock (columnFamilie)
            {
                if (columnFamilie.TryGetValue(name, out var handle))
                {
                    if (handle != null)
                        return handle;
                    InnerDB.TryGetColumnFamily(name, out handle);
                    columnFamilie[name] = handle;
                    return handle;
                }
                else if (!ReadOnly)
                {
                    var option = new ColumnFamilyOptions();
                    handle = InnerDB.CreateColumnFamily(option, name);
                    columnFamilie[name] = handle;
                    return handle;
                }
            }
            return null;
        }

        public void TryCatchUpWithPrimary()
        {
            if (ReadOnly)
            {
                InnerDB.TryCatchUpWithPrimary();
            }
        }
 
        public Table<T> GetTable<T>() where T : class
        {
            var name = typeof(T).FullName;
            var handle = GetOrCreateColumnFamilyHandle(name);
            if (handle == null)
                return null;
            return new Table<T>(this, name, handle);
        }

        public Table<byte[]> GetRawTable(string fullName)
        {
            var handle = GetOrCreateColumnFamilyHandle(fullName);
            if (handle == null)
                return null;
            return new Table<byte[]>(this, fullName, handle, true);
        }

        public Transaction NewTransaction()
        {
            return new Transaction(this);
        }

        public void WriteBatch(WriteBatch batch)
        {
            InnerDB.Write(batch);
        }

        public void Flush(bool wait)
        {
            if (!ReadOnly)
            {
                flushOption.SetWaitForFlush(wait);
                foreach (var c in columnFamilie)
                {
                    if (c.Value != null)
                    {
                        Native.Instance.rocksdb_flush_cf(InnerDB.Handle, flushOption.Handle, c.Value.Handle, out var err);
                        if (err != IntPtr.Zero)
                        {
                            var errStr = Marshal.PtrToStringAnsi(err);
                            Native.Instance.rocksdb_free(err);
                            LOGGER.Fatal($"rocksdb flush 错误:{errStr}");
                        }
                    }
                } 
            }
        }

        public void Close()
        {
            Flush(true);
            Native.Instance.rocksdb_cancel_all_background_work(InnerDB.Handle, true);
            //Native.Instance.rocksdb_free(flushOption.Handle);
            InnerDB.Dispose();
        }
    }
}