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
using System.Threading.Tasks;

namespace Geek.Core.CrossDay
{

    /// <summary>
    /// 因为actor有自动回收 需要手动在active再判断一次是否跨天
    /// 一般来说玩家的active跨天检测可以放到登陆时
    /// </summary>
    public interface ICrossDay
    {
        Task OnCrossDay(int openServerDay);
    }

    /// <summary>跨天触发器，一个服只能有一个实例,需要最先调用跨天[ServerActor]</summary>
    public interface ICrossDayTrigger
    {
        /// <summary>
        /// 判断跨天
        /// </summary>
        /// <returns>如果满足跨天，执行跨天逻辑，并返回当前开服天数</returns>
        Task<int> CheckCrossDay();
    }
    
}
