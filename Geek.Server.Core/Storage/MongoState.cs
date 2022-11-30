namespace Geek.Server.Core.Storage
{
    public class MongoState
    {
        public const string UniqueId = nameof(Id);
        public const string TimestampName = nameof(Timestamp);

        public string Id { get; set; }

        /// <summary>
        /// 回存时间戳
        /// </summary>
        public long Timestamp { get; set; }

        public byte[] Data { get; set; }

    }
}
