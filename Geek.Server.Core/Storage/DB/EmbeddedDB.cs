using LiteDB;
using LiteDB.Async;
using PolymorphicMessagePack;

namespace Core.Storage.DB
{
    /// <summary>
    /// 内嵌数据库-基于RocksDB
    /// </summary>
    public class EmbeddedDB
    {
        public class TableNameIndex
        {
            [BsonId]
            public string Name { get; set; }
            [BsonIgnore]
            ushort index;
            public ushort Index
            {
                get => index;
                set
                {
                    index = value;
                    InnerName = "t" + value.ToString();
                }
            }
            [BsonIgnore]
            public string InnerName { get; private set; }
        }

        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public LiteDatabaseAsync InnerDB { get; private set; }
        protected ILiteCollection<TableNameIndex> nameCollection;
        readonly Dictionary<string, TableNameIndex> nameCollectMemoryCache = new();
        readonly Dictionary<ushort, TableNameIndex> nameIndexCollectMemoryCache = new();
        public string DbPath { get; private set; } = "";
        public string SecondPath { get; private set; } = "";
        public bool ReadOnly { get; private set; } = false;

        public EmbeddedDB(string path, bool readOnly = false)
        {
            this.ReadOnly = readOnly;
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            DbPath = path;
            InnerDB = new LiteDatabaseAsync($"filename={path}.db");
            InnerDB.UnderlyingDatabase.Mapper.idToTypeGetter = PolymorphicTypeMapper.TryGet;
            InnerDB.UnderlyingDatabase.Mapper.typeToIdGetter = PolymorphicTypeMapper.TryGet;

            nameCollection = InnerDB.UnderlyingDatabase.GetCollection<TableNameIndex>("table_names");
            var all = nameCollection.FindAll().ToList();
            foreach (var n in all)
            {
                nameCollectMemoryCache.Add(n.Name, n);
                nameIndexCollectMemoryCache.Add(n.Index, n);
            }
        }

        public string GetTableIndexName(string name, bool addIfNotExsit = false)
        {
            lock (nameCollection)
            {
                if (nameCollectMemoryCache.TryGetValue(name, out var value))
                {
                    return value.InnerName;
                }
                if (addIfNotExsit)
                {
                    ushort index = (ushort)nameCollectMemoryCache.Count;
                    while (nameIndexCollectMemoryCache.ContainsKey(index))
                    {
                        index = (ushort)Math.Max(1, (index + 1) % ushort.MaxValue);
                    }
                    var tnIndex = new TableNameIndex { Name = name, Index = index };
                    nameCollectMemoryCache.Add(name, tnIndex);
                    nameIndexCollectMemoryCache.Add(index, tnIndex);
                    nameCollection.Insert(tnIndex);
                    return tnIndex.InnerName;
                }
                return default;
            }
        }

        public Table<T> GetTable<T>(string tableName = null) where T : class
        {
            var tName = typeof(T).FullName;
            if (tableName == null && tName == typeof(BsonDocument).FullName)
            {
                LOGGER.Error("当类型为BsonDocument时，必须指定table名");
                return null;
            }
            var name = (tableName ?? tName).Replace(".", "_");
            return new Table<T>(this, GetTableIndexName(name, true), name);
        }

        public async Task Flush()
        {
            try
            {
                await InnerDB.CheckpointAsync();
            }
            catch (Exception e)
            {
                LOGGER.Error($"EmbeddedDB flush错误:{e}");
            }
        }

        public async Task Backup(string targetPath)
        {
            await InnerDB.Backup(targetPath);
        }

        public void Close()
        {
            InnerDB?.Dispose();
            InnerDB = null;
        }
    }
}
