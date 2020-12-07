/****************************************************************************
Copyright (c) Geek

https://github.com/leeveel/GeekServer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using System;
using System.Threading;
/// <summary>
/// id 生成器
/// </summary>
public class IdGenerator
{
    static long cacheSecond;
    static long genSecond;
    static long increaseNumber = 0;
    static long GapSecond = (long)TimeSpan.FromDays(365.242f * 30).TotalSeconds;

    /// <summary>
    /// 所有服之间不能相同的统一用serverID
    /// </summary>
    public static long GetUniqueId(int serverOrModuleId)
    {
        long second = TimeUtils.CurrentTimeMillis() / 1000;
        if (second != cacheSecond)
        {
            Interlocked.Exchange(ref cacheSecond, second);
            Interlocked.Exchange(ref genSecond, second);
            Interlocked.Exchange(ref increaseNumber, 0);
        }

        return IdCreate(serverOrModuleId);
    }

    static long IdCreate(int moduleId)
    {
        //保证id为正数，最高位不用
        long ret = (long)(moduleId) << 47;//(63 - 16);//serverId 前16位[最大65535]
        ret |= 0x00007FFFFFFFFFFF & (genSecond << 18);//(63-16-17)时间戳用中间30位(可表示34年的秒数2004-1970)//0x00007FFFFFFFFFFF丢掉时间戳的高17位
        Interlocked.Increment(ref increaseNumber);

        if (increaseNumber >= 0x3FFFF)
        {
            //自增18位，每秒可生成262143个*扩容倍数
            //如果越界时间往后推30年,扩容数量9999-2999/30=233倍
            Interlocked.Exchange(ref increaseNumber, 0);
            Interlocked.Add(ref genSecond, GapSecond);
            if (genSecond <= 0)
                throw new Exception("id生成失败");
            return IdCreate(moduleId);
        }

        ret |= increaseNumber;
        return ret;
    }
}

public class IDModule
{
    //单服/玩家不同即可
    public const int Pet            = 1001;
    public const int Voyage         = 1002;
    public const int Rune           = 1003;
    public const int Robot          = 1004;
    public const int FightLog       = 1005;
    public const int EMail          = 1006;
    public const int EMailCMD       = 1007;

    //所有服不能相同的统一用serverID
    //所有服不同，但是由一个服分配
    public const int IslandBattle   = 2001;

    //日志id
    public const int LoggerActor    = 3001;
}
