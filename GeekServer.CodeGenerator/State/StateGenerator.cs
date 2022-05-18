using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;
using System.Diagnostics;
using Tools.Utils;

namespace Geek.Server
{
    [Generator]
    public class StateGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            //Debugger.Launch();
            ResLoader.LoadDll();
            context.RegisterForSyntaxNotifications(() => new StateFilter());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is StateFilter receiver)
            {
                //生成模板
                var str = ResLoader.LoadTemplate("State.liquid");
                Template agentTemplate = Template.Parse(str);
                foreach (var agent in receiver.StateList)
                {
                    string fullName = agent.GetFullName();
                    StateInfo info = new StateInfo();
                    info.Super = agent.Identifier.Text;
                    info.Name = info.Super + "Wrapper";
                    info.Space = Tools.GetNameSpace(fullName);
                    //处理Using
                    CompilationUnitSyntax root = agent.SyntaxTree.GetCompilationUnitRoot();
                    foreach (UsingDirectiveSyntax element in root.Usings)
                    {
                        info.Usingspaces.Add(element.Name.ToString());
                    }
                    info.Usingspaces.Add(Tools.GetNameSpace(fullName));

                    foreach (var member in agent.Members)
                    {
                        if (member is PropertyDeclarationSyntax property)
                        {

                            PropInfo mth = new PropInfo();
                            //修饰符
                            foreach (var m in property.Modifiers)
                            {
                                if (m.Text.Equals("public"))
                                    mth.IsPublic = true;
                                if (m.Text.Equals("virtual"))
                                    mth.IsVirtual = true;
                                if (m.Text.Equals("static"))
                                    mth.IsStatic = true;
                            }

                            if (mth.IsStatic)
                                continue;

                            //遍历注解
                            foreach (var a in property.AttributeLists)
                            {
                                mth.Attributelist.Add(a.ToString());
                            }

                            if (mth.IsStatic)
                                continue;

                            if (!mth.IsPublic || !mth.IsVirtual)
                            {
                                context.LogError($"{fullName}.{property.Identifier.Text}State中的字段必须为申明为public virtual");
                            }

                            if (mth.IsPublic && mth.IsVirtual)
                            {
                                info.Props.Add(mth);
                                mth.Name = property.Identifier.Text;
                                mth.FieldType = property.Type.ToString();
                            }
                        }
                    }
                    var source = agentTemplate.Render(info);
                    context.AddSource($"{info.Name}.g.cs", source);
                }
            }
        }

    }
}