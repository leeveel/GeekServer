using PolymorphicMessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeekServer.Serialize
{
    public partial class PolymorphicRegister
    {
        public static void Register(PolymorphicMessagePackSettings settings)
        {
            settings.RegisterType<object, Action>(100);
        }

        static PolymorphicRegister()
        {
            Console.WriteLine("*******************");
            Register(null);
        }
    }

    public partial class PolymorphicRegister
    {
        public static void Init()
        {
            
        }
    }

}
