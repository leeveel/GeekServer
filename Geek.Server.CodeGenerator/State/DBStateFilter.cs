using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Geek.Server.CodeGenerator.State
{
    public class DBStateFilter : ISyntaxReceiver
    {
        public const string NESTED_CLASS_DELIMITER = ".";
        public const string NAMESPACE_CLASS_DELIMITER = ".";

        public Dictionary<string, string> classFullNamePair = new Dictionary<string, string>();
        public Dictionary<string, List<string>> classPair = new Dictionary<string, List<string>>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax source)
            {
                if (source.BaseList == null)
                    return;
                var baseTypes = source.BaseList.Types.ToArray();
                var list = new List<string>();

                var className = source.Identifier.Text;
                classFullNamePair[className] = GetFullName(source);
                classPair[source.Identifier.Text] = list;
                foreach (var v in baseTypes)
                {
                    list.Add(v.ToString());
                }
            }
        }

        public static string GetFullName(ClassDeclarationSyntax source)
        {
            var items = new List<string>();
            var parent = source.Parent;
            while (parent.IsKind(SyntaxKind.ClassDeclaration))
            {
                var parentClass = parent as ClassDeclarationSyntax;
                if (parentClass == null)
                {
                    break;
                }
                items.Add(parentClass.Identifier.Text);

                parent = parent.Parent;
            }

            var nameSpace = parent as NamespaceDeclarationSyntax;

            var sb = new StringBuilder();
            if (nameSpace != null)
                sb.Append(nameSpace.Name);
            sb.Append(NAMESPACE_CLASS_DELIMITER);
            items.Reverse();
            items.ForEach(i => { sb.Append(i).Append(NESTED_CLASS_DELIMITER); });
            sb.Append(source.Identifier.Text);

            var result = sb.ToString();
            return result;
        }
    }
}
