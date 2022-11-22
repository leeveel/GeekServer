using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Geek.Server.Center.Web.Pages
{
    public static class Utils
    {
        public static string FormatJson(string str)
        {
            //格式化json字符串
            try
            {
                JsonSerializer serializer = new JsonSerializer();
                TextReader tr = new StringReader(str);
                JsonTextReader jtr = new JsonTextReader(tr);
                object obj = serializer.Deserialize(jtr);
                if (obj != null)
                {
                    StringWriter textWriter = new StringWriter();
                    JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                    {
                        Formatting = Formatting.Indented,
                        Indentation = 4,
                        IndentChar = ' '
                    };
                    serializer.Serialize(jsonWriter, obj);
                    return textWriter.ToString();
                }
                else
                {
                    return str;
                }
            }
            catch (Exception)
            {
                return str;
            }
        }

        public static bool CheckJson(string str)
        {
            //格式化json字符串 
            try
            {
                if (str == null)
                {
                    return false;
                }
                str = str.Trim();
                if (str.StartsWith("{") && str.EndsWith("}") || str.StartsWith("[") && str.EndsWith("]"))
                {
                    var data = MessagePack.MessagePackSerializer.ConvertFromJson(str);
                    return data != null && data.Length > 0;
                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
