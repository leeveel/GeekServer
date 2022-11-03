using MessagePack;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NLog;

namespace Geek.Server
{
    [BsonIgnoreExtraElements(true, Inherited = true)]// 兼容state的字段修改
    [MessagePackObject(true)]
    public abstract class InnerState
    {

    }

    [BsonIgnoreExtraElements(true, Inherited = true)]// 兼容state的字段修改
    [MessagePackObject(true)]
    public abstract class CacheState
    {
        public const string UniqueId = nameof(Id);

        public long Id { get; set; }

        #region md5
        [BsonIgnore]
        private StateMd5 StateMD5 { get; set; }

        public void AfterLoadFromDB(bool isNew)
        {
            StateMD5 = new StateMd5(this, isNew);
        }

        public (bool isChanged, byte[] data) IsChanged()
        {
            return StateMD5.IsChanged();
        }

        public (bool isChanged, long stateId, byte[] data) IsChangedWithStateId()
        {
            var res = StateMD5.IsChanged();
            return (res.Item1, Id, res.Item2);
        }

        public void AfterSaveToDB()
        {
            StateMD5.AfterSaveToDB();
        }
        #endregion

        public override string ToString()
        {
            return $"{base.ToString()}[Id={Id}]";
        }
    }

    class StateMd5
    {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private CacheState State { get; }

        public StateMd5(CacheState state, bool isNew)
        {
            State = state;
            if (!isNew)
                CacheMD5 = GetMD5AndData(state).md5;
        }

        private string CacheMD5 { get; set; }

        private string ToSaveMD5 { get; set; }

        public (bool, byte[]) IsChanged()
        {
            var (toSaveMD5, data) = GetMD5AndData(State);
            ToSaveMD5 = toSaveMD5;
            return (CacheMD5 == default || toSaveMD5 != CacheMD5, data);
        }

        public void AfterSaveToDB()
        {
            if (CacheMD5 == ToSaveMD5)
            {
                Log.Error($"调用AfterSaveToDB前CacheMD5已经等于ToSaveMD5 {State}");
            }
            CacheMD5 = ToSaveMD5;
        }

        private static (string md5, byte[] data) GetMD5AndData(CacheState state)
        {
            //var data = state.ToBson();
            var data = Serializer.Serialize(state);
            var md5str = CryptographyUtils.Md5(data);
            return (md5str, data);
        }

    }
}