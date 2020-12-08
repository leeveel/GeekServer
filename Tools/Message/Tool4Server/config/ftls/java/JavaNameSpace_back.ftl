/**
* Auto generated, do not edit it
*
* ${explain}
*/
package ${package};

import io.netty.buffer.ByteBuf;

import java.util.ArrayList;
import java.util.List;
<#list imports as importStr>
import ${package}.${importStr}.*;
</#list>

public final class ${name}{
<#assign listTypeMap = {"Integer":1, "Long":1, "Byte":1, "Boolean":1, "String":1, "Short":1, "Float":1, "Double":1} >
<#assign fieldTypeMap = {"int":1, "long":1, "byte":1, "boolean":1, "short":1, "float":1, "double":1} >
<#assign writeMap = {"int":"writeInt(this.%s_)", "long":"writeLong(this.%s_)", "byte":"writeByte(this.%s_)","float":"writeFloat(this.%s_)", "double":"writeDouble(this.%s_)",
"String":"writeString(this.%s_)","short":"writeShort(this.%s_)", "boolean":"writeBool(this.%s_)", "byte[]":"writeByteArray(this.%s_)" } >
<#assign writeListMap = {"Integer":"writeIntList(this.%s_)", "Long":"writeLongList(this.%s_)", "Byte":"writeByteList(this.%s_)","Float":"writeFloatList(this.%s_)",
"Double":"writeDoubleList(this.%s_)", "String":"writeStringList(this.%s_)","Boolean":"writeBoolList(this.%s_)", "Short":"writeShortList(this.%s_)"} >
<#assign readMap = {"int":"readInt()", "long":"readLong()", "float":"readFloat()", "double":"readDouble()", "String":"readString()",
"short":"readShort()", "boolean":"readBool()"} >
<#assign readListMap = {"Integer":"readIntList(this.%s_)", "Long":"readLongList(this.%s_)", "Float":"readFloatList(this.%s_)",
"Double":"readDoubleList(this.%s_)", "String":"readStringList(this.%s_)", "Boolean":"readBoolList(this.%s_)", "Short":"readShortList(this.%s_)"} >
<#list structs as struct>
    <#assign hasSup = false>
    /**
    * ${struct.exp}
    */
    <#if struct.isMessage == true>
    public final static class ${struct.name} extends BinaryMessage{
        public static final int MsgId = ${struct.msgId?c};
    <#else>
        <#if struct.superName != "">
            <#assign hasSup = true>
        </#if>
        <#if hasSup == true>
    public static class ${struct.name} extends ${struct.superName}{
        <#else>
    public static class ${struct.name} extends BinaryMessageStruct{
        </#if>
    </#if>
    <#assign filedNum = 0>
    <#list struct.fields as field>
        <#if field.isList == true>
            <#if !listTypeMap[field.cls]??>
        private ${field.owner}_${field.cls?cap_first}List ${field.name}_ = new ${field.owner}_${field.cls?cap_first}List(); //${field.exp}
            <#else >
        private ArrayList<${field.cls?cap_first}> ${field.name}_; //${field.exp}
            </#if>
        <#else>
            <#if field.optional == true>
        private byte byte_${field.name} = 0; // ${field.exp} tag
            </#if>
        private ${field.owner}${field.cls} ${field.name}_; // ${field.exp}
        </#if>
        <#assign filedNum = filedNum + 1>
    </#list>

    <#if enumMap?values?seq_contains(struct.name)>
        public byte type()
        {
            return (byte) _StructEnum_.${struct.name}.value;
        }
    </#if>

    <#list struct.fields as field>
        <#if field.isList == true>
        /**
        * get ${field.exp}
        * @return
        */
        public ArrayList<${field.owner}${field.cls}> get${field.name?cap_first}(){
            <#if !listTypeMap[field.cls]??>
			if (this.${field.name}_.${field.cls?uncap_first}_ == null)
                ${field.name}_.${field.cls?uncap_first}_ = new ArrayList<>();
            return ${field.name}_.${field.cls?uncap_first}_;
            <#else >
            return ${field.name}_;
            </#if>
        }

       /**
        * set ${field.exp}
        */
        public ${struct.name} set${field.name?cap_first}(ArrayList<${field.owner}${field.cls}> ${field.name}) {
            <#if !listTypeMap[field.cls]??>
            this.${field.name}_.${field.cls?uncap_first}_ = ${field.name};
            <#else >
            this.${field.name}_ = ${field.name};
            </#if>
            return this;
        }

        /**
        * add ${field.exp}
        */
        public ${struct.name} add${field.name?cap_first}(${field.owner}${field.cls} ${field.name}) {
            <#if !listTypeMap[field.cls]??>
            if (this.${field.name}_.${field.cls?uncap_first}_ == null)
            {
                this.${field.name}_.${field.cls?uncap_first}_ = new ArrayList<>();
            }
            this.${field.name}_.${field.cls?uncap_first}_.add(${field.name});
            <#else >
            if (this.${field.name}_ == null)
                this.${field.name}_ = new ArrayList<>();
            this.${field.name}_.add(${field.name});
            </#if>
            return this;
        }
		
		/**
        * list.size ${field.exp}
        */
		public int get${field.name?cap_first}Size() {
            <#if !listTypeMap[field.cls]??>
            if (this.${field.name}_.${field.cls?uncap_first}_ == null)
                return 0;
			else
				return this.${field.name}_.${field.cls?uncap_first}_.size();
            <#else >
			if (this.${field.name}_ == null)
				return 0;
				
			return this.${field.name}_.size();
            </#if>
        }
		
        <#else>
       /**
        * get ${field.exp}
        * @return ${field.cls}
        */
        public ${field.owner}${field.cls} get${field.name?cap_first}(){
            return ${field.name}_;
        }

       /**
        * set ${field.exp}
        */
        public ${struct.name} set${field.name?cap_first}(${field.owner}${field.cls} ${field.name}){
            <#if !fieldTypeMap[field.cls]??>
            if (${field.name} == null)
                return this;
            </#if>
            <#if field.optional == true>
            this.byte_${field.name} = 1;
            </#if>
            this.${field.name}_ = ${field.name};
            return this;
        }

            <#if field.optional == true>
        public boolean has${field.name?cap_first}()
        {
            return this.byte_${field.name} == 1;
        }
            </#if>
        </#if>
    </#list>

        @Override
        protected void write(){
    <#if hasSup == true>
            super.write();
    </#if>
    <#if struct.isMessage != true>
            int _index = this.buffer.writerIndex();
            writeInt(0);
    </#if>
            <#assign byteNum = (filedNum / 7)?ceiling>
            <#if (byteNum > 0)>
            <#list 1..byteNum as b>
                <#assign _byte = 255>
                <#if b == byteNum>
                    <#assign _byte = 0>
                    <#list ((b - 1) * 7 + 1)..filedNum as f>
                        <#assign _byte = (_byte * 2 + 1)>
                    </#list>
                </#if>
            writeByte((byte)${_byte?c});
                </#list>
            <#else >
            writeByte(0);
            </#if>
    <#list struct.fields as field>
        <#if field.isList == true>
            <#if writeListMap[field.cls]??>
                <#assign fun = writeListMap[field.cls]>
            ${fun?replace("%s", field.name)};
            <#else>
            ${field.cls}.writeList(buffer, this.${field.name}_);
            </#if>
        <#else>
            <#if field.optional == true>
            writeByte(byte_${field.name});
            if (byte_${field.name} == 1)
            {
                <#if writeMap[field.cls]??>
                    <#assign fun = writeMap[field.cls]>
                ${fun?replace("%s", field.name)};
                <#else>
                writeByte(${field.name}_.type());
                ${field.name}_.write(buffer);
                </#if>
            }
            <#else>
                <#if writeMap[field.cls]??>
                    <#assign fun = writeMap[field.cls]>
            ${fun?replace("%s", field.name)};
                <#else>
            writeByte(${field.name}_.type());
            ${field.name}_.write(buffer);
                </#if>
            </#if>
        </#if>
    </#list>
    <#if struct.isMessage != true>
            int _length = this.buffer.writerIndex();
            this.buffer.writerIndex(_index);
            writeInt(_length - _index);
            this.buffer.writerIndex(_length);
    </#if>
        }

        @Override
        protected void read(){
    <#if hasSup == true>
            super.read();
    </#if>
    <#if struct.isMessage != true>
            int _hasReadIndex = this.buffer.readerIndex();
            int _readIndex = readInt();
    </#if>
            List<Boolean> _read_field_num_list = new ArrayList<>();
            while (true)
            {
                short fieldMark = readUnsignedByte();
                if (fieldMark > 127)
                    fieldMark -= 256;

                for(int i = 0; i < 7; i++)
                {
                    _read_field_num_list.add(((byte)fieldMark & 1 << i) == 1 << i);
                }
                if(((byte)fieldMark & 1 << 7) == 0)
                    break;
            }
    <#assign num = 1>
    <#assign index = 0>

            <#if (struct.fields?size > 0)>
            do
            {
                <#list struct.fields as field>
                if (_read_field_num_list.size() > ${index} && _read_field_num_list.get(${index}))
                {
                <#assign index = index + 1>
                <#if field.isList == true>
                    <#if readListMap[field.cls]??>
                        <#assign fun = readListMap[field.cls]>
                    this.${field.name}_ = ${fun?replace("%s", field.name)};
                    <#else>
                    ${field.cls}.readList(buffer, this.${field.name}_);
                    </#if>
                <#else>
                    <#if field.optional == true>
                    if (readByte() == 1)
                    {
                        this.byte_${field.name} = 1;
                    </#if>
                            <#if readMap[field.cls]??>
                                <#assign fun = readMap[field.cls]>
                        this.${field.name}_ = ${fun?replace("%s", field.name)};
                            <#else>
                                <#if extendCon?keys?seq_contains(field.cls) || importExtendCon?keys?seq_contains(field.cls)>
                        ${field.owner}_StructEnum_ _enum${num} = ${field.owner}_StructEnum_.getStructEnum(readByte());
                        switch (_enum${num})
                        {
                            case ${field.cls}:
                                this.${field.name}_ = ${field.owner}${field.cls}.readBy(buffer);
                                break;
                                    <#if extendCon?keys?seq_contains(field.cls)>
                                    <#list extendCon["${field.cls}"] as sonCls>
                            case ${sonCls}:
                                this.${field.name}_ = ${field.owner}${sonCls}.readBy(buffer);
                                break;
                                    </#list>
                                    <#elseif importExtendCon?keys?seq_contains(field.cls)>
                                        <#list importExtendCon["${field.cls}"] as sonCls>
                            case ${sonCls}:
                                this.${field.name}_ = ${field.owner}${sonCls}.readBy(buffer);
                                break;
                                        </#list>
                                    </#if>
                            default:
                                break;
                        }
                                    <#assign num = num + 1>
                                <#else >
                        readByte();
                        this.${field.name}_ = ${field.owner}${field.cls}.readBy(buffer);
                                </#if>
                            </#if>
                    <#if field.optional == true>
                    }
                    </#if>
                </#if>
                }
                else
                {
                    break;
                }
    </#list>
            }
            while (false);
            </#if>

    <#if struct.isMessage != true>
            int _lastRead = _readIndex - (this.buffer.readerIndex() - _hasReadIndex);
            if (_lastRead > 0)
            {
                byte[] _byte = new byte[_lastRead];
                this.buffer.readBytes(_byte);
            }
    </#if>
        }

       /**
        * write from custom struct to buffer.
        */
        public static void writeList(ByteBuf buffer, IMessageList struct){
            int size = struct.size();
            buffer.writeShort((short)size);
            struct.write(buffer);
        }

       /**
        * read to custom struct from buffer.
        */
        public static void readList(ByteBuf buffer, IMessageList struct){
            int num = buffer.readShort();
            struct.read(buffer, num);
        }

       /**
        * read to custom struct from buffer.
        */
        public static ${struct.name} readBy(ByteBuf buffer){
            ${struct.name} _ele = new ${struct.name}();
            _ele.read(buffer);
    <#--<#if struct.isMessage == true>-->
			<#--_ele.buffer.release();-->
    <#--</#if>-->
			_ele.buffer = null;
            return _ele;
        }
    }
    <#assign hasSup = false>
</#list>
<#list enumMap?keys as key>
    public static final class _${enumMap["${key}"]}List implements IMessageList
    {
        public ArrayList<${enumMap["${key}"]}> ${enumMap["${key}"]?uncap_first}_;

        @Override
        public int size()
        {
            return ${enumMap["${key}"]?uncap_first}_ == null ? 0 : ${enumMap["${key}"]?uncap_first}_.size();
        }

        @Override
        public void write(ByteBuf buf)
        {
            if (${enumMap["${key}"]?uncap_first}_ == null)
                return;

            for (int i = 0; i < ${enumMap["${key}"]?uncap_first}_.size(); i++)
            {
                ${enumMap["${key}"]} _ele = ${enumMap["${key}"]?uncap_first}_.get(i);
                buf.writeByte(_ele.type());
                _ele.write(buf);
            }
        }

        @Override
        public void read(ByteBuf buf, int size)
        {
            if (${enumMap["${key}"]?uncap_first}_ == null)
            ${enumMap["${key}"]?uncap_first}_ = new ArrayList<>();

            for(int i = 0; i < size; i++)
            {
                int type = buf.readByte();
                ${enumMap["${key}"]} _ele = null;
    <#if extendCon?keys?seq_contains(enumMap["${key}"])>
                _StructEnum_ _enum = _StructEnum_.getStructEnum(type);
                switch(_enum)
                {
        <#assign clsName = enumMap["${key}"]>
                    case ${clsName}:
                        _ele = new ${enumMap["${key}"]}();
                        break;
        <#list extendCon["${clsName}"] as sonCls>
                    case ${sonCls}:
                        _ele = new ${sonCls}();
                        break;
        </#list>
                    default:
                        break;
                }
    <#else >
                _ele = new ${enumMap["${key}"]}();
    </#if>
                _ele.read(buf);
                ${enumMap["${key}"]?uncap_first}_.add(_ele);
            }
        }
    }
</#list>

    public enum _StructEnum_
    {
<#list enumMap?keys as key>
        ${enumMap["${key}"]}(${key}),
</#list>
        ;
        private int value;
        _StructEnum_(int value)
        {
            this.value = value;
        }

        public static _StructEnum_ getStructEnum(int value)
        {
            for (_StructEnum_ en : _StructEnum_.values())
            {
                if (en.value == value)
                    return en;
            }
            return null;
        }
    }
}
