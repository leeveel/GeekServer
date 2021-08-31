using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Collections.Generic;
using System.Linq;

namespace Geek.Server.Weavers
{
    public class AgentWeaver : BaseModuleWeaver
    {
        public override void Execute()
        {

            WriteMessage($"AgentWeaver Start：{ModuleDefinition.Assembly.Name}", MessageImportance.High);
            //import
            var baseCompTypeDef = FindTypeDefinition("Geek.Server.BaseComponent");
            if (baseCompTypeDef == null)
            {
                WriteError("获取Geek.Server.BaseComponent类型失败");
                return;
            }
            var compGetActorMethodDef = baseCompTypeDef.Methods.FirstOrDefault(md => md.Name == "get_Actor");
            if (compGetActorMethodDef == null)
            {
                WriteError("获取Geek.Server.BaseComponent.Actor属性失败");
                return;
            }
            var compGetActorMethodRef = ModuleDefinition.ImportReference(compGetActorMethodDef);
            var actorType = compGetActorMethodDef.ReturnType.Resolve().BaseType.Resolve();
            var actorSendAsync_Action_Def = actorType.Methods.FirstOrDefault(md => md.FullName.Contains("SendAsync(System.Action,"));
            var actorSendAsync_Func_Def = actorType.Methods.FirstOrDefault(md => md.FullName.Contains("SendAsync(System.Func`1<T>,"));
            var actorSendAsync_Task_Action_Def = actorType.Methods.FirstOrDefault(md => md.FullName.Contains("SendAsync(System.Func`1<System.Threading.Tasks.Task>,"));
            var actorSendAsync_Task_Func_Def = actorType.Methods.FirstOrDefault(md => md.FullName.Contains("SendAsync(System.Func`1<System.Threading.Tasks.Task`1<T>>,"));

            var actorSendAsync_Action = ModuleDefinition.ImportReference(actorSendAsync_Action_Def);
            var actorSendAsync_Func = ModuleDefinition.ImportReference(actorSendAsync_Func_Def);
            var actorSendAsync_Task_Action = ModuleDefinition.ImportReference(actorSendAsync_Task_Action_Def);
            var actorSendAsync_Task_Func = ModuleDefinition.ImportReference(actorSendAsync_Task_Func_Def);

            var func_def = actorSendAsync_Task_Action.Parameters[0].ParameterType.Resolve().Methods.FirstOrDefault(md => md.Name == ".ctor");

            var objDef = FindTypeDefinition("System.Object");
            var objConsDef = objDef.Methods.FirstOrDefault(md => md.Name == ".ctor");
            var objRef = ModuleDefinition.ImportReference(objDef);
            var objConsRef = ModuleDefinition.ImportReference(objConsDef);

            //WriteWarning("---222--" + (actorSendAsync_Action == null) + ",," + (actorSendAsync_Func == null )+ ",," + (actorSendAsync_Task_Action == null )+ ",," + (actorSendAsync_Task_Func == null));

            var adds = new List<TypeDefinition>(ModuleDefinition.GetTypes());
            foreach (var typeDef in adds)
            {
                if (typeDef.IsAbstract)
                    continue;

                bool isAgentComp = false;
                var interfaceArr = typeDef.GetAllInterfaces();
                foreach (var face in interfaceArr)
                {
                    if (face.FullName == "Geek.Server.IComponentAgent")
                    {
                        isAgentComp = true;
                        break;
                    }
                }
                if (!isAgentComp)
                    continue;

                if (!typeDef.BaseType.Resolve().FullName.StartsWith("Geek.Server.StateComponentAgent")
                        && !typeDef.BaseType.Resolve().FullName.StartsWith("Geek.Server.FuncComponentAgent"))
                {
                    WriteError($"{typeDef.FullName} Agent必须直接继承于StateComponentAgent或FuncComponentAgent，不允许二次继承");
                    continue;
                }

                var wrapperType = new TypeDefinition("Geek.Server.Wrapper", typeDef.Name + "Wrapper", typeDef.Attributes, typeDef);
                ModuleDefinition.Types.Add(wrapperType);

                int n = 0;
                foreach (var pm in typeDef.Methods)
                {
                    //排除非pulic和静态函数
                    if (!pm.IsPublic || pm.IsStatic)
                        continue;
                    if (pm.Name == "Active" || pm.Name == "Deactive")
                        continue;
                    //// 跳过Agent中override的方法
                    if (pm.IsConstructor)
                    {
                        var cons = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, TypeSystem.VoidReference);
                        var consIns = cons.Body.Instructions;
                        consIns.Append(
                            Instruction.Create(OpCodes.Ldarg_0),
                            Instruction.Create(OpCodes.Call, pm),
                            Instruction.Create(OpCodes.Nop),
                            Instruction.Create(OpCodes.Ret)
                            );
                        wrapperType.Methods.Add(cons);
                        continue;
                    }
                    // public且非static且非override且非构造函数返回值需要是Task形式
                    if (!pm.ReturnType.FullName.StartsWith("System.Threading.Tasks.Task"))
                    {
                        WriteError($"{pm.DeclaringType.FullName}.{pm.Name} 返回值非Task，应修改为Task形式");
                        continue;
                    }

                    if (!pm.IsVirtual)
                    {
                        pm.IsVirtual = true;
                        pm.IsNewSlot = true; //override
                    }

                    bool isGenericMethod = pm.GenericParameters.Count > 0;
                    bool notAwait = pm.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "NotAwait") != null;

                    // public方法生成
                    var wrapperMethod = new MethodDefinition(pm.Name, pm.Attributes ^ MethodAttributes.NewSlot, pm.ReturnType);

                    for (int p = 0; p < pm.Parameters.Count; ++p)
                        wrapperMethod.Parameters.Add(pm.Parameters[p]);

                    for (int i = 0; i < pm.GenericParameters.Count; i++)
                    {
                        var yourTypeParam = pm.GenericParameters[i];
                        var targetTypeParam = new GenericParameter(yourTypeParam.Name, wrapperMethod)
                        {
                            Attributes = yourTypeParam.Attributes //includes non-type constraints
                        };
                        wrapperMethod.GenericParameters.Add(targetTypeParam);
                    }
                    wrapperType.Methods.Add(wrapperMethod);
                    wrapperMethod.Body = new MethodBody(wrapperMethod);
                    var instructions = wrapperMethod.Body.Instructions;

                    //找到对应的compAgent.Actor函数Ref
                    TypeDefinition stComp = FindBaseStateOrQueryType(typeDef);
                    var getActorMethodDef = stComp.Methods.FirstOrDefault(m => m.Name == "get_Actor");
                    var getActorMethodRef = ModuleDefinition.ImportReference(getActorMethodDef).MakeGeneric(((GenericInstanceType)(typeDef.BaseType)).GenericArguments[0]);
                    MethodReference sendRef;
                    if (pm.ReturnType.FullName == "System.Threading.Tasks.Task")
                    {
                        sendRef = actorSendAsync_Task_Action;
                        // Func<Task>
                    }
                    else
                    {
                        // Func<Task<T>>
                        sendRef = ModuleDefinition.ImportReference(actorSendAsync_Task_Func_Def.MakeGenericMethod(((GenericInstanceType)(pm.ReturnType)).GenericArguments[0]));
                    }

                    var returnFuncRef = ModuleDefinition.ImportReference(func_def).MakeGeneric(pm.ReturnType);

                    //判断是否有参数
                    if (wrapperMethod.Parameters.Count > 0)
                    {
                        // 创建私有方法
                        var privateMethodDef = new MethodDefinition($"<>Gen_m_{n}_{pm.Name}", MethodAttributes.Private | MethodAttributes.HideBySig, pm.ReturnType);
                        var privateMethodInstructions = privateMethodDef.Body.Instructions;
                        privateMethodInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                        for (int i = 0; i < pm.Parameters.Count; i++)
                        {
                            ParameterDefinition param = pm.Parameters[i];
                            privateMethodDef.Parameters.Add(param);
                            var code = GetCodeByFieldIndex(i + 1);
                            if (code == OpCodes.Ldarga_S)
                            {
                                privateMethodInstructions.Add(Instruction.Create(code, param));
                            }
                            else
                            {
                                privateMethodInstructions.Add(Instruction.Create(code));
                            }
                        }

                        foreach (var item in pm.GenericParameters)
                        {
                            privateMethodDef.GenericParameters.Add(new GenericParameter(item.Name, privateMethodDef));
                        }

                        if (isGenericMethod)
                        {
                            var result = new GenericInstanceMethod(pm);
                            foreach (var item in pm.GenericParameters)
                            {
                                result.GenericArguments.Add(item);
                            }

                            privateMethodInstructions.Add(Instruction.Create(OpCodes.Call, result));
                        }
                        else
                        {
                            privateMethodInstructions.Add(Instruction.Create(OpCodes.Call, pm));
                        }

                        privateMethodInstructions.Add(Instruction.Create(OpCodes.Ret));

                        wrapperType.Methods.Add(privateMethodDef);

                        //创建内部类
                        var innerWrapper = new TypeDefinition("Geek.Server.Wrapper", $"<>Gen_c_{n++}_{pm.Name}", TypeAttributes.NestedPrivate | TypeAttributes.BeforeFieldInit | TypeAttributes.Sealed, TypeSystem.ObjectReference);
                        wrapperType.NestedTypes.Add(innerWrapper);
                        var innerConsDef = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, ModuleDefinition.TypeSystem.Void);
                        innerConsDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                        innerConsDef.Body.Instructions.Add(Instruction.Create(OpCodes.Call, objConsRef));
                        innerConsDef.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
                        innerConsDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                        innerWrapper.Methods.Add(innerConsDef);

                        WriteWarning($"AgentWeaver End：{pm.Name}Wrapper");

                        foreach (var item in pm.GenericParameters)
                        {
                            innerWrapper.GenericParameters.Add(new GenericParameter(item.Name, innerWrapper));
                        }

                        innerWrapper.Fields.Add(new FieldDefinition($"outer", FieldAttributes.Public, wrapperType));

                        foreach (ParameterDefinition pd in pm.Parameters)
                        {
                            innerWrapper.Fields.Add(new FieldDefinition(pd.Name, FieldAttributes.Public, pd.ParameterType));
                        }

                        var innerDec = new GenericInstanceType(innerWrapper);
                        foreach (var item1 in innerWrapper.GenericParameters)
                        {
                            innerDec.GenericArguments.Add(item1);
                        }

                        var innerMethod = new MethodDefinition($"{pm.Name}", MethodAttributes.HideBySig | MethodAttributes.Assembly, pm.ReturnType);
                        innerWrapper.Methods.Add(innerMethod);
                        var innerMethodIns = innerMethod.Body.Instructions;

                        foreach (var item in innerWrapper.Fields)
                        {
                            innerMethodIns.Append(
                                Instruction.Create(OpCodes.Ldarg_0),
                                Instruction.Create(OpCodes.Ldfld, isGenericMethod ? new FieldReference(item.Name, item.FieldType, innerDec) : item)
                                );
                        }
                        if (isGenericMethod)
                        {
                            var genericPrivateMethodRef = new GenericInstanceMethod(privateMethodDef);
                            foreach (var item in innerWrapper.GenericParameters)
                            {
                                genericPrivateMethodRef.GenericArguments.Add(item);
                            }

                            innerMethodIns.Add(Instruction.Create(OpCodes.Call, genericPrivateMethodRef));
                        }
                        else
                        {
                            innerMethodIns.Add(Instruction.Create(OpCodes.Call, privateMethodDef));
                        }
                        innerMethodIns.Add(Instruction.Create(OpCodes.Ret));

                        var genericInnner = new GenericInstanceType(innerWrapper);
                        foreach (var item in wrapperMethod.GenericParameters)
                        {
                            genericInnner.GenericArguments.Add(item);
                        }

                        if (isGenericMethod)
                        {
                            wrapperMethod.Body.Variables.Add(new VariableDefinition(genericInnner));
                        }
                        else
                        {
                            wrapperMethod.Body.Variables.Add(new VariableDefinition(ModuleDefinition.ImportReference(innerWrapper)));
                        }

                        instructions.Append(
                            Instruction.Create(OpCodes.Newobj, isGenericMethod ? new MethodReference(innerConsDef.Name, innerConsDef.ReturnType, genericInnner)
                            {
                                ExplicitThis = innerConsDef.ExplicitThis,
                                HasThis = innerConsDef.HasThis
                            } : ModuleDefinition.ImportReference(innerConsDef)),// 内部类构造函数
                            Instruction.Create(OpCodes.Stloc_0)
                            );

                        for (int i = 0; i < innerWrapper.Fields.Count; i++)
                        {
                            var code = GetCodeByFieldIndex(i);
                            var fieldDef = innerWrapper.Fields[i];
                            instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                            if (code == OpCodes.Ldarg_S)
                            {
                                instructions.Add(Instruction.Create(code, pm.Parameters[i - 1]));
                            }
                            else
                            {
                                instructions.Add(Instruction.Create(code));
                            }
                            instructions.Add(Instruction.Create(OpCodes.Stfld, isGenericMethod ? new FieldReference(fieldDef.Name, fieldDef.FieldType, genericInnner) : fieldDef));

                        }

                        var ldloc1 = Instruction.Create(OpCodes.Ldloc_1);
                        instructions.Append(
                            Instruction.Create(OpCodes.Nop),
                            Instruction.Create(OpCodes.Ldarg_0),
                            Instruction.Create(OpCodes.Call, getActorMethodRef),//.Actor
                            Instruction.Create(OpCodes.Ldloc_0),
                            Instruction.Create(OpCodes.Ldftn, isGenericMethod ? new MethodReference(innerMethod.Name, innerMethod.ReturnType, genericInnner)
                            {
                                ExplicitThis = innerMethod.ExplicitThis,
                                HasThis = innerMethod.HasThis
                            } : innerMethod),// 内部类 方法
                            Instruction.Create(OpCodes.Newobj, returnFuncRef),//Func的构造方法
                            Instruction.Create(notAwait ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1),
                            Instruction.Create(OpCodes.Ldc_I4, 10000),
                            Instruction.Create(OpCodes.Callvirt, sendRef),//sendAsync
                            Instruction.Create(OpCodes.Stloc_1),
                            Instruction.Create(OpCodes.Br_S, ldloc1),
                            ldloc1,
                            Instruction.Create(OpCodes.Ret)
                            );
                    }
                    else
                    {
                        var ldloc0 = Instruction.Create(OpCodes.Ldloc_0);
                        instructions.Insert(0,
                            Instruction.Create(OpCodes.Nop),
                            Instruction.Create(OpCodes.Ldarg_0),//this
                            Instruction.Create(OpCodes.Call, getActorMethodRef),//.Actor
                            Instruction.Create(OpCodes.Ldarg_0),//this
                            Instruction.Create(OpCodes.Ldftn, pm),//base.func
                            Instruction.Create(OpCodes.Newobj, returnFuncRef),//lamda
                            Instruction.Create(notAwait ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1),
                            Instruction.Create(OpCodes.Ldc_I4, 10000),//10000
                            Instruction.Create(OpCodes.Callvirt, sendRef),//actor.SendAsync
                            Instruction.Create(OpCodes.Stloc_0),
                            Instruction.Create(OpCodes.Br_S, ldloc0),
                            ldloc0,
                            Instruction.Create(OpCodes.Ret)
                            );
                    }

                    wrapperMethod.Body.InitLocals = true;
                    wrapperMethod.Body.Variables.Add(new VariableDefinition(pm.ReturnType));
                }
            }

            WriteMessage($"AgentWeaver End：{ModuleDefinition.Assembly.Name}", MessageImportance.High);
        }

        private static TypeDefinition FindBaseStateOrQueryType(TypeDefinition typeDef)
        {
            var stComp = typeDef.BaseType.Resolve();
            while (!stComp.FullName.StartsWith("Geek.Server.BaseComponentAgent")
                && !stComp.FullName.StartsWith("Geek.Server.QueryComponentAgent"))
                stComp = stComp.BaseType.Resolve();
            return stComp;
        }

        private static OpCode GetCodeByFieldIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return OpCodes.Ldarg_0;
                case 1:
                    return OpCodes.Ldarg_1;
                case 2:
                    return OpCodes.Ldarg_2;
                case 3:
                    return OpCodes.Ldarg_3;
                default:
                    return OpCodes.Ldarg_S;
            }
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "System";
            yield return "mscorlib";
            yield return "GeekServer.Core";
            yield return "GeekServer.Hotfix";
            yield return "System.Collections";
        }
    }
}
