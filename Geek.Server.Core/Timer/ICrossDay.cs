namespace Geek.Server.Core.Timer
{
    public interface ICrossDay
    {
        public Task OnCrossDay(int openServerDay);
    }
}
