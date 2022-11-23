
namespace Geek.Server.Core.Storage.DB
{
    public class NotFindKeyException : Exception
    {
        public NotFindKeyException(string message) : base(message)
        {
        }
    }
}