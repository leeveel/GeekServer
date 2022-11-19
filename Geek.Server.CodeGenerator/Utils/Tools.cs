using System;

namespace Geek.Server.CodeGenerator.Utils
{
    public static class Tools
    {

        public static string GetNameSpace(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return fullName;
            int i = fullName.LastIndexOf('.');
            if(i < 0)
                return fullName;
            return fullName.Substring(0, i);
        }

        public static string RemoveWhitespace(this string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

    }
}
