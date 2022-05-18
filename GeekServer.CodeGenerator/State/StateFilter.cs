using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Geek.Server
{
    public class StateFilter : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> StateList { get; private set; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax syntax)
            {
                if (IsCompAgent(syntax))
                {
                    StateList.Add(syntax);
                }
            }
        }

        public bool IsCompAgent(ClassDeclarationSyntax source)
        {
            if(source.BaseList == null)
                return false;
            IEnumerable<BaseTypeSyntax> baseTypes = source.BaseList.Types.Select(baseType => baseType);
            var res = baseTypes.Any((baseType) => 
            {
                string baseName = baseType.ToString();
                return baseName.Contains("DBState")
                      || baseName.Contains("BaseDBState");
            });
            return res;
        }

    }
}
