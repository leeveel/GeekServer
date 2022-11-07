
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using RocksDbSharp;

namespace Geek.Server
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
        protected IRemoteBackup _remoteBackup;
        protected FlushOptions flushOption;
        protected ConcurrentDictionary<string, ColumnFamilyHandle> columnFamilie = new ConcurrentDictionary<string, ColumnFamilyHandle>();

        public IRemoteBackup remoteBackup
        {
            get
            {
                if (_remoteBackup == null)
                    _remoteBackup = new IRemoteBackup();
                return _remoteBackup;
            }
        }

        public EmbeddedDB(string path, bool readOnlay = false)
        {
            DbPath = path;
            var option = new DbOptions();
            RocksDb.TryListColumnFamilies(option, DbPath, out var cfList);
            var cfs = new ColumnFamilies();

            foreach (var cf in cfList)
            {
                cfs.Add(cf, new ColumnFamilyOptions());
                columnFamilie[cf] = null;
            }

            option.SetCreateIfMissing(true).SetCreateMissingColumnFamilies(true);
            if (readOnlay)
            {
                option.SetMaxOpenFiles(-1);
                SecondPath = DbPath + "_second";
                InnerDB = RocksDb.OpenAsSecondary(option, DbPath, SecondPath, cfs);
            }
            else
            {
                InnerDB = RocksDb.Open(option, DbPath, cfs);
            }

            flushOption = new FlushOptions();
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
                else
                {
                    var option = new ColumnFamilyOptions();
                    handle = InnerDB.CreateColumnFamily(option, name);
                    columnFamilie[name] = handle;
                    return handle;
                }
            }
        }

        public void TryCatchUpWithPrimary()
        {
            if (ReadOnly)
            {
                InnerDB.TryCatchUpWithPrimary();
            }
        }

        public void SetRemoteBackup(IRemoteBackup remote)
        {
            _remoteBackup = remote;
        }

        public Table<T> GetTable<T>() where T : class
        {
            var name = typeof(T).FullName;
            return new Table<T>(this, name, GetOrCreateColumnFamilyHandle(name));
        }

        public Table<byte[]> GetRawTable(string fullName)
        {
            return new Table<byte[]>(this, fullName, GetOrCreateColumnFamilyHandle(fullName), true);
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
                remoteBackup.Flush();
            }
        }

        public void Close()
        {
            Flush(true);
            Native.Instance.rocksdb_cancel_all_background_work(InnerDB.Handle, true);
            Native.Instance.rocksdb_free(flushOption.Handle);
            InnerDB.Dispose();
        }
    }
}