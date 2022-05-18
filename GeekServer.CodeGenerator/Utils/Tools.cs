using System;
using System.Collections.Generic;
using System.Text;

namespace Geek.Server
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


    }
}
