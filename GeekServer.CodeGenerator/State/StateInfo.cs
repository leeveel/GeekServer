using System;
using System.Collections.Generic;
using System.Text;

namespace Geek.Server
{


    public class PropInfo
    {
        public string Name { get; set; }
        public string FieldType { get; set; }

        public bool IsPublic { get; set; }

        public bool IsStatic { get; set; }

        public bool IsVirtual { get; set; }

        public List<string> Attributelist { get; private set; } = new List<string>();

        public bool Iscontainer
        {
            get 
            { 
                return FieldType.Contains("StateMap") || FieldType.Contains("StateList"); 
            }
        }

        public string Declare
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("public override ");
                sb.Append(FieldType);
                sb.Append(" ");
                sb.Append(Name);
                return sb.ToString();
            }
        }

    }

    public class StateInfo
    {
        public string Space { get; set; }
        public string Name { get; set; }
        public string Super { get; set; }
        public List<PropInfo> Props { get; set; } = new List<PropInfo>();
        public List<string> Usingspaces { get; set; } = new List<string>();
    }
}
