using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Geek.Server.CodeGenerator.Agent
{
    public class AgentFilter : ISyntaxReceiver
    {

        public List<ClassDeclarationSyntax> AgentList { get; private set; } = new List<ClassDeclarationSyntax>();
        private HashSet<string> AgentNameList = new HashSet<string>();
        private List<ClassDeclarationSyntax> PartialList = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax syntax)
            {
                if (IsCompAgent(syntax))
                {
                    AgentList.Add(syntax);
                    AgentNameList.Add(syntax.Identifier.Text);
                }
                else
                {
                    if (syntax.Modifiers.ToList().FindIndex(s => s.Text == "partial") >= 0)
                    {
                        PartialList.Add(syntax);
                    }
                }
            }
        }

        public void ClearPartialList()
        {
            foreach (var p in PartialList)
            {
                if (AgentNameList.Contains(p.Identifier.Text))
                {
                    AgentList.Add(p);
                }
            }
            PartialList.Clear();
        }

        public bool IsCompAgent(ClassDeclarationSyntax source)
        {
            if (source.BaseList == null)
                return false;
            IEnumerable<BaseTypeSyntax> baseTypes = source.BaseList.Types.Select(baseType => baseType);
            var res = baseTypes.Any((baseType) =>
            {
                string baseName = baseType.ToString();
                return baseName.Contains("StateCompAgent")
                      || baseName.Contains("FuncCompAgent")
                      || baseName.Contains("QueryCompAgent")
                      || baseName.Contains("BaseCompAgent");
            });
            return res;
        }
    }
}
