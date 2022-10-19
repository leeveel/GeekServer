using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;
using System.Diagnostics;
using Tools.Utils;

namespace Geek.Server
{
    [Generator]
    public class AgentGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            //Debugger.Launch();
            ResLoader.LoadDll();
            context.RegisterForSyntaxNotifications(() => new AgentFilter());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is AgentFilter receiver)
            {
                //生成模板
                var str = ResLoader.LoadTemplate("Agent.liquid");
                Template agentTemplate = Template.Parse(str);
                foreach (var agent in receiver.AgentList)
                {
                    string fullName = agent.GetFullName();
                    AgentInfo info = new AgentInfo();
                    info.Super = agent.Identifier.Text;
                    info.Name = info.Super + "Wrapper";
                    //info.Space = Tools.GetNameSpace(fullName);
                    info.Space = "Wrapper.Agent";
                    //处理Using
                    CompilationUnitSyntax root = agent.SyntaxTree.GetCompilationUnitRoot();
                    foreach (UsingDirectiveSyntax element in root.Usings)
                    {
                        info.Usingspaces.Add(element.Name.ToString());
                    }
                    info.Usingspaces.Add(Tools.GetNameSpace(fullName));

                    foreach (var member in agent.Members)
                    {
                        if (member is MethodDeclarationSyntax method)
                        {

                            if (method.Identifier.Text.Equals("Active")
                                || method.Identifier.Text.Equals("Deactive"))
                            {
                                continue;
                            }

                            MthInfo mth = new MthInfo();
                            //修饰符
                            foreach (var m in method.Modifiers)
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
                            foreach (var a in method.AttributeLists)
                            {
                                var attStr = a.ToString().RemoveWhitespace();
                                if (attStr.Contains(MthInfo.AsyncApi))
                                {
                                    mth.IsAsyncApi = true;
                                    if (a.Attributes[0].ArgumentList == null)
                                        continue;
                                    foreach (var arg in a.Attributes[0].ArgumentList.Arguments)
                                    {
                                        var argStr = arg.ToString();
                                        if (argStr.Contains(MthInfo.NotAwait))
                                        {
                                            mth.Isawait = argStr.Contains("true");
                                        }
                                        else if (argStr.Contains(MthInfo.ThreadSafe))
                                        {
                                            mth.Isthreadsafe = argStr.Contains("true");
                                        }
                                        else if (argStr.Contains(MthInfo.ExecuteTime))
                                        {
                                            mth.Executetime = int.Parse(argStr.Split(':')[1]);
                                        }
                                    }
                                }
                            }

                            if (mth.Isthreadsafe && !mth.Isawait)
                                continue;

                            if (!mth.IsAsyncApi)
                                continue;

                            if (mth.IsAsyncApi && !(mth.IsPublic && mth.IsVirtual))
                                context.LogError($"{fullName}.{method.Identifier.Text}标记为【AsyncApi】的函数必须申明为public virtual");

                            if (mth.IsPublic && mth.IsVirtual)
                            {
                                info.Methods.Add(mth);
                                mth.Name = method.Identifier.Text;
                                mth.ParamDeclare = method.ParameterList.ToString();  //(int a, List<int> list)
                                mth.Returntype = method.ReturnType.ToString();   //Task<T>
                                if (!mth.Isawait && !mth.Returntype.Equals("Task"))
                                    context.LogError($"{fullName}.{method.Identifier.Text}只有返回值为Task类型才能设置isAwait=false");
                                mth.Constraint = method.ConstraintClauses.ToString(); //where T : class, new() where K : BagState
                                mth.Typeparams = method.TypeParameterList?.ToString(); //"<T, K>"	
                                foreach (var p in method.ParameterList.Parameters)
                                {
                                    mth.Params.Add(p.Identifier.Text);
                                }
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