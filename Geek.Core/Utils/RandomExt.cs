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

public class RandomExt
{
    private long seed = 100;

    public RandomExt(long seed)
    {
        SetSeed(seed);
    }

    public RandomExt()
    {
 
    }
    
    public void SetSeed(long seed)
    {
        this.seed = (seed ^ 0x5DEECE66DL) & ((1L << 48) - 1);
    }

    public int NextInt(int n)
    {
        if (n <= 0) throw new ArgumentException("n must be positive");

        if ((n & -n) == n)  // i.e., n is a power of 2
        {
            var ret = (int)((n * (long)Next(31)) >> 31);
            //Logging.TestLog("随机数:" + ret);
            return ret;
        }

        long bits, val;
        do
        {
            bits = Next(31);
            val = bits % (long)n;
        }
        while (bits - val + (n - 1) < 0);

        //Logging.TestLog("随机数:" + val);
        return (int)val;
    }

    protected long Next(int bits)
    {
        seed = (seed * 0x5DEECE66DL + 0xBL) & ((1L << 48) - 1);

        return (long)(seed >> (48 - bits));
    }

}