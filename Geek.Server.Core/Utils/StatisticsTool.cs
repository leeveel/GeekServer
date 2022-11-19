
using System.Text;
using Geek.Server.Core.Actors.Impl;

namespace Geek.Server.Core.Utils
{
    public class StatisticsTool
    {
        private const string Format = "yyyy-MM-dd HH:mm";
        private readonly Dictionary<string, Dictionary<string, int>> countDic = new();

        private readonly WorkerActor workerActor = new();

        public async Task<string> CountRecord(int limit = 10)
        {
            return await workerActor.SendAsync(() =>
            {
                var sb = new StringBuilder();
                foreach (var kv in countDic)
                {
                    var time = kv.Key;

                    foreach (var item in kv.Value)
                    {
                        var type = item.Key;
                        var count = item.Value;
                        if (count >= limit)
                            sb.Append('\t').Append(time).Append('\t').Append(count).Append('\t').Append(type).Append('\n');
                    }
                }
                return sb.ToString();
            });
        }

        public void ClearCount()
        {
            workerActor.Tell(countDic.Clear);
        }

        public void ClearCount(DateTime time)
        {
            workerActor.Tell(() =>
            {
                var timeStr = time.ToString(Format);
                countDic.RemoveIf((k, v) => k.CompareTo(timeStr) < 0);
            });
        }

        public void Count(string key, int num = 1)
        {
            if (num <= 0)
                return;

            workerActor.Tell(() =>
            {
                var timeStr = DateTime.Now.ToString(Format);
                if (!countDic.TryGetValue(timeStr, out var cd))
                {
                    cd = new Dictionary<string, int>();
                    countDic[timeStr] = cd;
                }
                var old = cd.GetValueOrDefault(key, 0);
                cd[key] = old + num;
            });
        }
    }
}
