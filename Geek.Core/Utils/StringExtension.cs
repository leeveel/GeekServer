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
using System.IO;

public static class StringExtension
{
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static int[] SplitToIntArray(this string str, char sep)
    {
        if (string.IsNullOrEmpty(str))
            return new int[0];

        var arr = str.Split(sep);
        int[] ret = new int[arr.Length];
        for (int i = 0; i < arr.Length; ++i)
        {
            if(int.TryParse(arr[i], out var t))
                ret[i] = t;
        }
        return ret;
    }

    public static int[][] SplitTo2IntArray(this string str, char sep1, char sep2)
    {
        if(string.IsNullOrEmpty(str))
            return new int[0][];

        var arr = str.Split(sep1);
        if (arr.Length <= 0)
            return new int[0][];

        int[][] ret = new int[arr.Length][];

        for (int i = 0; i < arr.Length; ++i)
            ret[i] = arr[i].SplitToIntArray(sep2);
        return ret;
    }

    public static void CreateAsDirectory(this string path, bool isFile = false)
    {
        if (isFile)
            path = Path.GetDirectoryName(path);
        if (!Directory.Exists(path))
        {
            CreateAsDirectory(path, true);
            Directory.CreateDirectory(path);
        }
    }
}
