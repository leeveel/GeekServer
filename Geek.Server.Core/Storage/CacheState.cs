using Geek.Server.Core.Serialize;
using Geek.Server.Core.Utils;
using MessagePack;
using NLog;
using Standart.Hash.xxHash;

namespace Geek.Server.Core.Storage
{  

    [MessagePackObject(true)]
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

        public void AfterLoadFromDB(bool isNew)
        {
            stateHash = new StateHash(this, isNew);
        }

        public bool IsChanged()
        {
            return stateHash.IsChanged();
        } 

        public void AfterSaveToDB()
        {
            stateHash.AfterSaveToDB();
        }
        #endregion
    }


    class StateHash
    {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private CacheState State { get; }

        public StateHash(CacheState state, bool isNew)
        {
            State = state;
            if (!isNew)
                CacheHash = GetHash(state);
        }

        private UInt128 CacheHash { get; set; } = 0;

        private UInt128 ToSaveHash { get; set; }

        public bool IsChanged()
        {
            ToSaveHash = GetHash(State);
            return CacheHash == 0 || ToSaveHash != CacheHash;
        }

        public void AfterSaveToDB()
        {
            CacheHash = ToSaveHash;
        }

        class HashStream : Stream
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

        unsafe private static UInt128 GetHash(CacheState state)
        {
            try
            {
                var hashSteam = new HashStream();
                Serializer.Serialize(hashSteam, state);
                return new UInt128(hashSteam.hash, (ulong)hashSteam.Position);
            }
            catch (Exception e)
            {
                Log.Error($"异常类型是 {state.GetType().FullName} {e.Message}");
            }
            return 0;
        }
    } 
}
