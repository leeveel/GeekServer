using System.IO;

public static class StringExtension
{
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static int[] SplitToIntArray(this string str, char sep = '+')
    {
        if (string.IsNullOrEmpty(str))
            return Array.Empty<int>();

        var arr = str.Split(sep);
        int[] ret = new int[arr.Length];
        for (int i = 0; i < arr.Length; ++i)
        {
            if (int.TryParse(arr[i], out var t))
                ret[i] = t;
        }
        return ret;
    }

    public static int[][] SplitTo2IntArray(this string str, char sep1 = ';', char sep2 = '+')
    {
        if (string.IsNullOrEmpty(str))
            return Array.Empty<int[]>();

        var arr = str.Split(sep1);
        if (arr.Length <= 0)
            return Array.Empty<int[]>();

        int[][] ret = new int[arr.Length][];

        for (int i = 0; i < arr.Length; ++i)
            ret[i] = arr[i].SplitToIntArray(sep2);
        return ret;
    }

    /// <summary>
    /// 根据字符串创建目录,递归
    /// </summary>
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