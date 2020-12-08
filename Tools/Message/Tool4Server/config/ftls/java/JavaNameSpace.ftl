//Auto generated, do not modify it
//限制：命名不能以下划线开头，不能夸命名空间继承
//兼容限制1、字段只能添加，添加后不能删除，字段只能添加到最后
//兼容限制2、不能修改字段类型（如从bool改为long）
//${explain}

using NLog;
using System;
using Message.Base;
using DotNetty.Buffers;
using System.Collections.Generic;
<#list imports as importStr>
using Message.${importStr};
</#list>

namespace Message.${name}
{
	<#if (enumMap?keys?size > 0)>
    public enum _TypeEnum_
    {
		<#list enumMap?keys as key>
        ${enumMap["${key}"]} = ${key},
		</#list>
    }
	</#if>
	<#assign readMap = {"int":"ReadInt()",
						"long":"ReadLong()",
						"bool":"ReadBool()",
						"string":"ReadString()",
						"short":"ReadShort()",
						"float":"ReadFloat()",
						"double":"ReadDouble()"} >
						
	<#assign writeMap = {"int":"WriteInt(%s)",
						"long":"WriteLong(%s)",
						"bool":"WriteBool(%s)",
						"string":"WriteString(%s)",
						"short":"WriteShort(%s)",
						"float":"WriteFloat(%s)",
						"double":"WriteDouble(%s)"} >
						
	<#assign initMap = {"int":"0",
						"long":"0L",
						"bool":"false",
						"string":"null",
						"short":"0",
						"float":"0f",
						"double":"0"} >
						
    <#list structs as struct>
    //${struct.exp}
    <#if struct.superName != "">
    public class ${struct.name} : ${struct.superName}
    <#else>
    public class ${struct.name} : BinaryMessageStruct
    </#if>
    {
		<#if struct.isMessage == true>
        public const int MsgId = ${struct.msgId?c};
		</#if>
		
		<#list struct.fields as field>
		<#if field.isList>
        public List<${field.owner}${field.cls}> ${field.name}{ get; protected set; } //${field.exp}
		<#elseif field.optional == true>
		protected byte _${field.name} = 0; // ${field.exp} tag
		protected ${field.owner}${field.cls} __${field.name}; // ${field.exp}
		public bool has${field.name?cap_first}() { return this._${field.name} == 1; }
		public ${field.owner}${field.cls} ${field.name} { set { _${field.name} = 1; __${field.name} = value; } get { return __${field.name}; } }
		<#else> 
		public ${field.owner}${field.cls} ${field.name}; // ${field.exp}
		</#if>
		</#list>

        //构造函数
        public ${struct.name}() : base()
        {
			<#list struct.fields as field>
			<#if field.isList>
			${field.name} = new List<${field.cls}>();
			</#if>
			</#list>
        }
		
        //读取数据
        public override void Read()
        {
            try
            {
				<#if struct.superName != "">
                base.Read();
				</#if>
				
				<#if !struct.isMessage>
				int _toReadLenOffset_ = buffer.ReaderIndex;
				int _toReadLength_ = ReadInt();
				</#if>
				
				List<bool> _fieldList_ = new List<bool>();
				while(true)
				{
					var _fieldMark_ = ReadByte();
					for(int i = 0; i < 7; ++i)
					{
						var _h_ = 1 << i;
						bool _mark_ = (_fieldMark_ & _h_) == _h_;
						if(_mark_) _fieldList_.Add(true);
						else break;
					}
					var _e_ = 1 << 7;
					if((_fieldMark_ & _e_) == 0)
						break;
				}
				
				<#if (struct.fields?size > 0)>
				int _fieldNum_ = _fieldList_.Count;
				</#if>
				
				<#assign readIdx = -1>
				while(true)
				{
					<#list struct.fields as field>
					<#assign readIdx = readIdx + 1>
					if(_fieldNum_ > ${readIdx} && _fieldList_[${readIdx}])
					{
						<#if field.isList == true>
						short _count_ = ReadShort();
						for(int _a_ = 0; _a_ < _count_; ++_a_)
						{
						</#if>
							<#if field.optional == true>
							_${field.name} = ReadByte();
							if(_${field.name} == 1)
							{
							</#if>
								<#if !readMap[field.cls]??>
								var _real_type_ = (${field.owner}_TypeEnum_)ReadByte();
								<#if extendCon?keys?seq_contains(field.cls) || importExtendCon?keys?seq_contains(field.cls)>
								${field.owner}${field.cls} _value_ = null;
								switch(_real_type_)
								{
									case ${field.owner}_TypeEnum_.${field.cls} : _value_ = new ${field.owner}${field.cls}(); break;
									<#if extendCon?keys?seq_contains(field.cls)>
									<#list extendCon["${field.cls}"] as sonCls>
									case ${field.owner}_TypeEnum_.${sonCls} : _value_ = new ${field.owner}${sonCls}(); break;
									</#list>
									<#elseif importExtendCon?keys?seq_contains(field.cls)>
									<#list importExtendCon["${field.cls}"] as sonCls>
									case ${field.owner}_TypeEnum_.${sonCls} : _value_ = new ${field.owner}${sonCls}(); break;
									</#list>
									</#if>
									default:break;
								}
								<#else>
								${field.owner}${field.cls} _value_ = new ${field.owner}${field.cls}();
								</#if>
								_value_.Read();
								<#if field.isList == true>
								${field.name}.Add(_value_);
								<#else>
								${field.name} = _value_;
								</#if>
								<#else>
								<#if field.isList == true>
								${field.name}.Add(${readMap[field.cls]});
								<#else>
								${field.name} = ${readMap[field.cls]};
								</#if>
								</#if>
							<#if field.optional == true>
							}
							</#if>
						<#if field.isList == true>
						}
						</#if>
					} else { break; }
					
					</#list>
					break;
				}
				
				<#if !struct.isMessage>
				//剔除未知数据
				while(buffer.ReaderIndex - _toReadLenOffset_ < _toReadLength_)
					ReadByte();
				</#if>
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

		<#if !struct.isMessage>
        public void WriteWithType()
        {
            WriteByte((byte)_TypeEnum_.${struct.name});
            Write();
        }
		</#if>

        //写入数据
        public override void Write()
        {
            try
            {
				<#if struct.superName != "">
				base.Write();
				</#if>
				
				<#if !struct.isMessage>
				int _toWriteLenOffset_ = buffer.WriterIndex;
				WriteInt(0);
				</#if>
				
				<#assign fieldNum = struct.fields?size>
				<#assign len = (fieldNum / 7)?floor>
				<#assign lastByte = 0>
				<#if (len > 0)>
				<#list 1 .. len as t>
				WriteByte(255);
				</#list>
				</#if>
				<#if (fieldNum > 0)>
				<#list (len * 7) + 1 .. fieldNum as t>
				<#assign lastByte = (lastByte * 2) + 1>
				</#list>
				</#if>
				WriteByte(${lastByte?c});
				
				<#assign _lc_ = false>
                <#list struct.fields as field>
				<#if field.isList>
				
				<#if !_lc_>short </#if>_listCount_ = (short)${field.name}.Count;
				<#assign _lc_ = true>
				WriteShort(_listCount_);
				for (int _a_ = 0; _a_ < _listCount_; ++_a_)
                {
				</#if>
					<#if field.optional == true>
					WriteByte(_${field.name});
					if(_${field.name} == 1)
					{
					</#if>
						<#if writeMap[field.cls]??>
						<#if field.isList>
						<#assign fun = writeMap[field.cls]>
						${fun?replace("%s", field.name + "[_a_]")};
						<#else>
						<#assign fun = writeMap[field.cls]>
						${fun?replace("%s", field.name)};
						</#if>
						<#else>
						<#if field.isList>
						if(${field.name}[_a_] == null)
							;//UnityEngine.Debug.LogError("${field.name} has nil item, idx == " + _a_);
						else
							${field.name}[_a_].WriteWithType();
						<#else>
							if(${field.name} == null)
								;//UnityEngine.Debug.LogError("${field.name} is null");
							else
								${field.name}.WriteWithType();
						</#if>
						</#if>
					<#if field.optional == true>
					}
					</#if>
				<#if field.isList>
				}
				</#if>
                </#list>
				
				<#if !struct.isMessage>
				WriteInt(buffer.WriterIndex - _toWriteLenOffset_);
				</#if>
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
    </#list>
}