using MessagePack;
using MongoDB.Bson.Serialization.Attributes;
using NLog;

namespace Geek.Server
{
    [MessagePackObject(true)]
    public abstract class InnerState
    {

    }

    [MessagePackObject(true)]
    public abstract class CacheState
    {
        public const string UniqueId = nameof(Id);

        public long Id { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}[Id={Id}]";
        }
    }

}