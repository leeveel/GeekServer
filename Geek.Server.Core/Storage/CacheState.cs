using Geek.Server.Core.Serialize;
using MessagePack;
using MongoDB.Bson.Serialization.Attributes;
using NLog;

namespace Geek.Server.Core.Storage
{

    [MessagePackObject(true)]
    [BsonIgnoreExtraElements(true,Inherited =true)]
    public abstract class CacheState
    {
        public const string UniqueId = nameof(Id);

        public long Id { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}[Id={Id}]";
        }

        #region hash 
        private StateHash stateHash;

        public void AfterLoadFromDB()
        {
            stateHash ??= new StateHash(this, true);
        }

        public bool IsChanged()
        {
            stateHash ??= new StateHash(this, false);
            return stateHash.IsChanged();
        }

        public void AfterSaveToDB()
        {
            stateHash.AfterSaveToDB();
        }
        #endregion
    }


    public class StateHash
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private CacheState State { get; }
        private UInt128 CurrentHash { get; set; }
        private UInt128 DBHash { get; set; }

        public StateHash(CacheState state, bool loadFromDB = false)
        {
            State = state;
            CurrentHash = GetHash();
            if (loadFromDB)
            {
                DBHash = CurrentHash;
            }
        }

        public bool IsChanged()
        {
            if (DBHash != CurrentHash)
                return true;
            CurrentHash = GetHash();
            return DBHash != CurrentHash || CurrentHash == 0;
        }

        public void AfterSaveToDB()
        {
            DBHash = CurrentHash;
        }

        public class HashStream : Stream
        {
            public ulong hash = 3074457345618258791ul;
            public override bool CanRead => false;

            public override bool CanSeek => false;

            public override bool CanWrite => true;

            public override long Length => 0;

            public override long Position { set; get; }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return 0;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return 0;
            }

            public override void SetLength(long value)
            {

            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                Position += count;
                for (int i = offset; i < count; i++)
                {
                    hash += buffer[i];
                    hash *= 3074457345618258799ul;
                }
            }
        }

        unsafe private UInt128 GetHash()
        {
            if (State == null)
                return 0;
            try
            {
                var hashSteam = new HashStream();
                Serializer.Serialize(hashSteam, State);
                return new UInt128(hashSteam.hash, (ulong)hashSteam.Position);
            }
            catch (Exception e)
            {
                Log.Error($"GetHash异常,type:[{State.GetType().FullName}]:{e.Message}");
            }
            return 0;
        }
    }
}
