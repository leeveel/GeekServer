using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

namespace Geek.Server.Weavers
{
    //自定义weaver配置说明
    //https://github.com/Fody/Home/blob/master/pages/in-solution-weaving.md
    public class StateWeaver : BaseModuleWeaver
    {
        public override void Execute()
        {
            WriteMessage($"StateWeaver Start：{ModuleDefinition.Assembly.Name}", MessageImportance.High);
            var baseStateTypeDef = FindTypeDefinition("Geek.Server.BaseState");
            if (baseStateTypeDef == null)
            {
                WriteError("获取Geek.Server.BaseState类型失败");
                return;
            }

            var changeFieldDef = baseStateTypeDef.Fields.FirstOrDefault(fd => fd.Name == "_stateChanged");
            if (changeFieldDef == null)
            {
                WriteError("BaseState获取_stateChanged属性失败");
                return;
            }
            var changeFieldRef = ModuleDefinition.ImportReference(changeFieldDef);


            var innerStateTypeDef = FindTypeDefinition("Geek.Server.InnerDBState");
            if (innerStateTypeDef == null)
            {
                WriteError("获取Geek.Server.InnerDBState类型失败");
                return;
            }

            var stFieldDef = innerStateTypeDef.Fields.FirstOrDefault(fd => fd.Name == "stList");
            if (stFieldDef == null)
            {
                WriteError("InnerDBState获取stList属性失败");
                return;
            }

            var stFieldRef = ModuleDefinition.ImportReference(stFieldDef);

            var stListTypeDef = stFieldDef.FieldType.Resolve();
            if (stListTypeDef == null)
            {
                WriteError("获取HashSet<Geek.Server.BaseState>类型失败");
                return;
            }

            var stList_AddMethodDef = stListTypeDef.Methods.FirstOrDefault(md => md.Name == "Add");
            var stList_AddMethodRef = ModuleDefinition.ImportReference(stList_AddMethodDef.MakeGeneric(baseStateTypeDef));
            var stList_RemoveMethodDef = stListTypeDef.Methods.FirstOrDefault(md => md.Name == "Remove");
            var stList_RemoveMethodRef = ModuleDefinition.ImportReference(stList_RemoveMethodDef.MakeGeneric(baseStateTypeDef));
            //import end




            var typeList = ModuleDefinition.GetTypes();
            foreach (var typeDef in typeList)
            {
                //类型是否需要weave
                bool needTypeWeave = false;
                var checkTypeType = typeDef;
                while (checkTypeType != null)
                {
                    if (checkTypeType.FullName == "Geek.Server.InnerDBState")
                    {
                        needTypeWeave = true;
                        break;
                    }
                    if (checkTypeType.BaseType == null)
                        break;
                    checkTypeType = checkTypeType.BaseType.Resolve();
                }
                if (!needTypeWeave)
                    continue;


                //mongodb字段
                foreach(var fdDef in typeDef.Fields)
                {
                    if (!fdDef.IsPublic || fdDef.IsStatic)
                        continue;
                    bool ignore = false;
                    var atts = fdDef.CustomAttributes;
                    foreach (var att in atts)
                    {
                        if (att.AttributeType.FullName == "MongoDB.Bson.Serialization.Attributes.BsonIgnoreAttribute")
                        {
                            ignore = true;
                            break;
                        }
                    }
                    if (ignore)
                        continue;
                    WriteError($"{fdDef.DeclaringType.FullName}.{fdDef.Name} 没有使用属性定义(get/set)，将无法检测State变化；改用属性定义/添加BsonIgnore标签");
                }


                //构造函数
                var constructor = typeDef.Methods.FirstOrDefault(md => md.Name == ".ctor");
                //weave属性
                foreach (var proDef in typeDef.Properties)
                {
                    if (!proDef.HasThis) //static
                        continue;
                    if (!proDef.SetMethod.HasBody || !proDef.GetMethod.HasBody) //get only/set only
                        continue;
                    if (!proDef.SetMethod.IsPublic || !proDef.GetMethod.IsPublic) //no public
                        continue;

                    bool ignore = false;
                    var atts = proDef.CustomAttributes;
                    foreach(var att in atts)
                    {
                        if (att.AttributeType.FullName == "Geek.Server.StateWeaveIgnoreAttribute")
                        {
                            ignore = true;
                            break;
                        }
                    }
                    if (ignore)
                        continue;
                    
                    var getMethod = proDef.GetMethod;
                    var instructions = proDef.SetMethod.Body.Instructions;
                    if (instructions.Count > 4)
                    {
                        WriteError($"{proDef.DeclaringType.FullName}.{proDef.Name} 已包含实现，无法weave");
                        continue;
                    }

                    if (proDef.PropertyType.IsValueType || proDef.PropertyType == ModuleDefinition.TypeSystem.String)
                    {
                        //_stateChanged = true;
                        instructions.Insert(3,
                            Instruction.Create(OpCodes.Ldarg_0),
                            Instruction.Create(OpCodes.Ldc_I4_1),
                            Instruction.Create(OpCodes.Stfld, changeFieldRef),
                            Instruction.Create(OpCodes.Nop)
                        );
                    }
                    else
                    {
                        //属性是否需要weave
                        bool proNeedWeave = false;
                        var checkProType = proDef.PropertyType.Resolve();
                        while(checkProType != null)
                        {
                            if (checkProType.FullName == "Geek.Server.BaseState")
                            {
                                proNeedWeave = true;
                                break;
                            }
                            if (checkProType.BaseType == null)
                                break;
                            checkProType = checkProType.BaseType.Resolve();
                        }
                        if (!proNeedWeave)
                        {
                            WriteError($"{proDef.DeclaringType.FullName}.{proDef.Name} 不是基础类型也不是BaseState，将无法检测State变化；基类改为BaseState/添加StateWeaveIgnore标签");
                            continue;
                        }

                        //构造函数加入stList
                        var ctorIns = constructor.Body.Instructions;
                        var ctrIf = Instruction.Create(OpCodes.Nop);
                        ctorIns.Insert(ctorIns.Count - 1,
                            Instruction.Create(OpCodes.Ldarg_0), //this
                            Instruction.Create(OpCodes.Call, getMethod), //field
                            Instruction.Create(OpCodes.Ldnull), //null
                            Instruction.Create(OpCodes.Cgt_Un), //比较
                            Instruction.Create(OpCodes.Brfalse_S, ctrIf), //if flase jump

                            //if内
                            Instruction.Create(OpCodes.Ldarg_0),//this
                            Instruction.Create(OpCodes.Ldfld, stFieldRef), //stList
                            Instruction.Create(OpCodes.Ldarg_0), //this
                            Instruction.Create(OpCodes.Call, getMethod), //field
                            Instruction.Create(OpCodes.Callvirt, stList_AddMethodRef), //调用stList.Add
                            Instruction.Create(OpCodes.Pop),
                            ctrIf);


                        int index = 0;
                        var fieldDef = instructions[2].Operand as FieldDefinition;

                        //if(oldValue != null) stList.Remove(oldValue)
                        var removeEnd = Instruction.Create(OpCodes.Nop);
                        index = instructions.Insert(index,
                            Instruction.Create(OpCodes.Ldarg_0), //this
                            Instruction.Create(OpCodes.Ldfld, fieldDef), //field
                            Instruction.Create(OpCodes.Ldnull), //null
                            Instruction.Create(OpCodes.Cgt_Un), //比较
                            Instruction.Create(OpCodes.Brfalse_S, removeEnd), //if flase jump

                            //if内
                            Instruction.Create(OpCodes.Ldarg_0),//this
                            Instruction.Create(OpCodes.Ldfld, stFieldRef), //stList
                            Instruction.Create(OpCodes.Ldarg_0), //this
                            Instruction.Create(OpCodes.Ldfld, fieldDef), //field
                            Instruction.Create(OpCodes.Callvirt, stList_RemoveMethodRef), //调用stList.Remove
                            Instruction.Create(OpCodes.Pop),
                            removeEnd);

                        //if(newValue != null) stList.Add(newValue);
                        var addEnd = Instruction.Create(OpCodes.Nop);
                        index = instructions.Insert(index,
                            Instruction.Create(OpCodes.Ldarg_1), //value
                            Instruction.Create(OpCodes.Ldnull), //null
                            Instruction.Create(OpCodes.Cgt_Un), //比较
                            Instruction.Create(OpCodes.Brfalse_S, addEnd), //if flase jump

                            //if内
                            Instruction.Create(OpCodes.Ldarg_0), //this
                            Instruction.Create(OpCodes.Ldfld, stFieldRef), //stList
                            Instruction.Create(OpCodes.Ldarg_1),
                            Instruction.Create(OpCodes.Callvirt, stList_AddMethodRef),//调用stList.add
                            Instruction.Create(OpCodes.Pop),
                            addEnd);

                        //_stateChanged = true;
                        index = instructions.Insert(index,
                            Instruction.Create(OpCodes.Ldarg_0),
                            Instruction.Create(OpCodes.Ldc_I4_1),
                            Instruction.Create(OpCodes.Stfld, changeFieldRef));
                    }
                }
            }

            WriteMessage($"StateWeaver End：{ModuleDefinition.Assembly.Name}", MessageImportance.High);
        }

        //Used as a list for Fody.BaseModuleWeaver.FindType.
        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "System";
            yield return "mscorlib";
            yield return "GeekServer.Core";
            yield return "System.Collections";
        }
    }
}
