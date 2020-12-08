--${explain}

${name}_TypeEnum = {};


<#list structs as struct>
--${struct.exp}
<#if struct.superName != "">
${name}_${struct.name} = Class("${name}_${struct.name}",${name}_${struct.superName});
<#else>
${name}_${struct.name} = Class("${name}_${struct.name}");
</#if>
${name}_TypeEnum[${struct.type}] = ${name}_${struct.name};
function ${name}_${struct.name}:FakeCtr()
    <#list struct.fields as field>
    <#if field.cls = "int">
    self.${field.name} = 0; --${field.exp}
    <#elseif field.cls = "long">
    self.${field.name} = 0; --${field.exp}
    <#elseif field.cls = "byte">
    self.${field.name} = 0; --${field.exp}
    <#elseif field.cls = "float">
    self.${field.name} = 0; --${field.exp}
    <#elseif field.cls = "double">
    self.${field.name} = 0; --${field.exp}
    <#elseif field.cls = "string">
    self.${field.name} = ""; --${field.exp}
    <#elseif field.cls = "bool">
    self.${field.name} = false; --${field.exp}
    <#elseif field.cls = "short">
    self.${field.name} = 0; --${field.exp}
    <#else>
    self.${field.name} = nil; --${field.exp}
    </#if>
    </#list>
    <#list struct.lists as list>
    self.${list.name} = {};   --${list.exp}
    </#list>
end

function ${name}_${struct.name}:FakeDtr()
<#list struct.fields as field>
    <#if field.cls = "int">
    <#elseif field.cls = "long">
    <#elseif field.cls = "byte">
    <#elseif field.cls = "float">
    <#elseif field.cls = "double">
    <#elseif field.cls = "string">
    <#elseif field.cls = "bool">
    <#elseif field.cls = "short">
    <#else>
    Delete(self.${field.name});
    </#if>
</#list>

<#list struct.lists as list>
    <#if list.cls = "int">
    <#elseif list.cls = "long">
    <#elseif list.cls = "byte">
    <#elseif list.cls = "float">
    <#elseif list.cls = "double">
    <#elseif list.cls = "string">
    <#elseif list.cls = "bool">
    <#elseif list.cls = "short">
    <#else>
    for i,v in ipairs(self.${list.name}) do
        Delete(v);
    end
    </#if>
</#list>
end

function ${name}_${struct.name}:Read(bytes)
    local real_type = 0;
    <#if struct.superName!="">
    ${name}_${struct.superName}.Read(self,bytes);
    </#if>
    <#list struct.fields as field>
    <#if field.cls = "int">
    self.${field.name} = bytes:ReadInt();
    <#elseif field.cls = "long">
    self.${field.name} = bytes:ReadLong();
    <#elseif field.cls = "byte">
    self.${field.name} = bytes:ReadByte();
    <#elseif field.cls = "float">
    self.${field.name} = bytes:ReadFloat();
    <#elseif field.cls = "double">
    self.${field.name} = bytes:ReadDouble();
    <#elseif field.cls = "string">
    self.${field.name} = bytes:ReadString();
    <#elseif field.cls = "bool">
    self.${field.name} = bytes:ReadBool();
    <#elseif field.cls = "short">
    self.${field.name} = bytes:ReadShort();
    <#else>
    real_type = bytes:ReadByte();
    self.${field.name} = New(${name}_TypeEnum[real_type]);
    self.${field.name}:Read(bytes);
    </#if>
    </#list>

    local count = 0;
    <#list struct.lists as list>
    count = bytes:ReadShort();
    for a = 1,count do
        <#if list.cls = "int">
        local value = bytes:ReadInt();
        table.insert(self.${list.name},value)
        <#elseif list.cls = "long">
        local value = bytes:ReadLong();
        table.insert(self.${list.name},value)
        <#elseif list.cls = "byte">
        local value = bytes:ReadByte();
        table.insert(self.${list.name},value)
        <#elseif list.cls = "float">
        local value = bytes:ReadString();
        table.insert(self.${list.name},value);
        <#elseif list.cls = "double">
        local value = bytes:ReadDouble();
        table.insert(self.${list.name},value);
        <#elseif list.cls = "string">
        local value = bytes:ReadString();
        table.insert(self.${list.name},value);
        <#elseif list.cls = "bool">
        local value = bytes:ReadBool();
        table.insert(self.${list.name},value);
        <#elseif list.cls = "short">
        local value = bytes:ReadShort();
        table.insert(self.${list.name},value);
        <#else>
        real_type = bytes:ReadByte();
        local value = New(${name}_TypeEnum[real_type]);
        value:Read(bytes);
        table.insert(self.${list.name},value);
        </#if>
    end
    </#list>
end

function ${name}_${struct.name}:WriteWithType()
    local bytes = New(ByteArray);
    bytes:WriteByte(${struct.type});
    bytes:WriteBytes(self:Write());
    return bytes;
end

function ${name}_${struct.name}:Write()
    local bytes = New(ByteArray);
    <#if struct.superName != "">
    bytes:WriteBytes(${name}_${struct.superName}.Write(self));
    </#if>
    <#list  struct.fields as field>
    <#if field.cls = "int">
    bytes:WriteInt(self.${field.name});
    <#elseif field.cls = "long">
    bytes:WriteLong(self.${field.name});
    <#elseif field.cls = "byte">
    bytes:WriteByte(self.${field.name});
    <#elseif field.cls = "float">
    bytes:WriteFloat(self.${field.name});
    <#elseif field.cls = "double">
    bytes:WriteDouble(self.${field.name});
    <#elseif field.cls = "string">
    bytes:WriteString(self.${field.name});
    <#elseif field.cls = "bool">
    bytes:WriteBool(self.${field.name});
    <#elseif field.cls = "short">
    bytes:WriteShort(self.${field.name});
    <#else>
    if self.${field.name}==nil then
        self.${field.name} = New(${name}_${field.cls});
    end
    bytes:WriteBytes(self.${field.name}:WriteWithType());
    </#if>
    </#list>

    <#list struct.lists as list>
    bytes:WriteShort(#self.${list.name});
    for i,v in ipairs(self.${list.name}) do
        <#if list.cls = "int">
        bytes:WriteInt(v);
        <#elseif list.cls = "long">
        bytes:WriteLong(v);
        <#elseif list.cls = "byte">
        bytes:WriteByte(v);
        <#elseif list.cls = "float">
        bytes:WriteFloat(v);
        <#elseif list.cls = "double">
        bytes:WriteDouble(v);
        <#elseif list.cls = "string">
        bytes:WriteString(v);
        <#elseif list.cls = "bool">
        bytes:WriteBool(v);
        <#elseif list.cls = "short">
        bytes:WriteShort(v);
        <#else>
        bytes:WriteBytes(v:WriteWithType());
        </#if>
    end
    </#list>
    return bytes;
end

function ${name}_${struct.name}:ParseData(csBytes)
    local bytes = New(ByteArray);
    bytes:WriteBuf(csBytes);
    bytes:SetPos(1);
    self:Read(bytes);
    Delete(bytes);
end

function ${name}_${struct.name}:GetData()
    return self:Write():GetBytes();
end
</#list>

<#list messages as message>
--${message.exp}
${message.name} = Class("${message.name}");
${message.name}.MsgId = ${message.msgId?c};
function ${message.name}:FakeCtr()
    self.MsgId = ${message.msgId?c};  --消息ID
    <#list message.fields as field>
    <#if field.cls = "int">
    self.${field.name} = 0; --${field.exp}
    <#elseif field.cls = "long">
    self.${field.name} = 0; --${field.exp}
    <#elseif field.cls = "byte">
    self.${field.name} = 0; --${field.exp}
    <#elseif field.cls = "float">
    self.${field.name} = 0; --${field.exp}
    <#elseif field.cls = "double">
    self.${field.name} = 0; --${field.exp}
    <#elseif field.cls = "string">
    self.${field.name} = ""; --${field.exp}
    <#elseif field.cls = "bool">
    self.${field.name} = false; --${field.exp}
    <#elseif field.cls = "short">
    self.${field.name} = 0; --${field.exp}
    <#else>
    self.${field.name} = New(${name}_${field.cls}); --${field.exp}
    </#if>
    </#list>
    <#list message.lists as list>
    self.${list.name} = {};   --${list.exp}
    </#list>
end

function ${message.name}:FakeDtr()
<#list message.fields as field>
    <#if field.cls = "int">
    <#elseif field.cls = "long">
    <#elseif field.cls = "byte">
    <#elseif field.cls = "float">
    <#elseif field.cls = "double">
    <#elseif field.cls = "string">
    <#elseif field.cls = "bool">
    <#elseif field.cls = "short">
    <#else>
    Delete(self.${field.name});
    </#if>
</#list>

<#list message.lists as list>
    <#if list.cls = "int">
    <#elseif list.cls = "long">
    <#elseif list.cls = "byte">
    <#elseif list.cls = "float">
    <#elseif list.cls = "double">
    <#elseif list.cls = "string">
    <#elseif list.cls = "bool">
    <#elseif list.cls = "short">
    <#else>
    for i,v in ipairs(self.${list.name}) do
        Delete(v);
    end
    </#if>
</#list>
end

function ${message.name}:Read(bytes)
    local real_type = 0;
    <#list message.fields as field>
    <#if field.cls = "int">
    self.${field.name} = bytes:ReadInt();
    <#elseif field.cls = "long">
    self.${field.name} = bytes:ReadLong();
    <#elseif field.cls = "byte">
    self.${field.name} = bytes:ReadByte();
    <#elseif field.cls = "float">
    self.${field.name} = bytes:ReadFloat();
    <#elseif field.cls = "double">
    self.${field.name} = bytes:ReadDouble();
    <#elseif field.cls = "string">
    self.${field.name} = bytes:ReadString();
    <#elseif field.cls = "bool">
    self.${field.name} = bytes:ReadBool();
    <#elseif field.cls = "short">
    self.${field.name} = bytes:ReadShort();
    <#else>
    real_type = bytes:ReadByte();
    self.${field.name} = New(${name}_TypeEnum[real_type]);
    self.${field.name}:Read(bytes);
    </#if>
    </#list>

    local count = 0;
    <#list message.lists as list>
    count = bytes:ReadShort();
    for a = 1,count do
        --${list.exp}
        <#if list.cls = "int">
        local value = bytes:ReadInt();
        table.insert(self.${list.name},value)
        <#elseif list.cls = "long">
        local value = bytes:ReadLong();
        table.insert(self.${list.name},value)
        <#elseif list.cls = "byte">
        local value = bytes:ReadByte();
        table.insert(self.${list.name},value)
        <#elseif list.cls = "float">
        local value = bytes:ReadString();
        table.insert(self.${list.name},value);
        <#elseif list.cls = "double">
        local value = bytes:ReadDouble();
        table.insert(self.${list.name},value);
        <#elseif list.cls = "string">
        local value = bytes:ReadString();
        table.insert(self.${list.name},value);
        <#elseif list.cls = "bool">
        local value = bytes:ReadBool();
        table.insert(self.${list.name},value);
        <#elseif list.cls = "short">
        local value = bytes:ReadShort();
        table.insert(self.${list.name},value);
        <#else>
        real_type = bytes:ReadByte();
        local value = New(${name}_TypeEnum[real_type]);
        value:Read(bytes);
        table.insert(self.${list.name},value);
        </#if>
    end
    </#list>
end

function ${message.name}:Write()
    local bytes = New(ByteArray);
    <#list  message.fields as field>
    <#if field.cls = "int">
    bytes:WriteInt(self.${field.name});
    <#elseif field.cls = "long">
    bytes:WriteLong(self.${field.name});
    <#elseif field.cls = "byte">
    bytes:WriteByte(self.${field.name});
    <#elseif field.cls = "float">
    bytes:WriteFloat(self.${field.name});
    <#elseif field.cls = "double">
    bytes:WriteDouble(self.${field.name});
    <#elseif field.cls = "string">
    bytes:WriteString(self.${field.name});
    <#elseif field.cls = "bool">
    bytes:WriteBool(self.${field.name});
    <#elseif field.cls = "short">
    bytes:WriteShort(self.${field.name});
    <#else>
    if self.${field.name}==nil then
        self.${field.name} = New(${name}_${field.cls});
    end
    bytes:WriteBytes(self.${field.name}:WriteWithType());
    </#if>
    </#list>

    <#list message.lists as list>
    bytes:WriteShort(#self.${list.name});
    for i,v in ipairs(self.${list.name}) do
        <#if list.cls = "int">
        bytes:WriteInt(v);
        <#elseif list.cls = "long">
        bytes:WriteLong(v);
        <#elseif list.cls = "byte">
        bytes:WriteByte(v);
        <#elseif list.cls = "float">
        bytes:WriteFloat(v);
        <#elseif list.cls = "double">
        bytes:WriteDouble(v);
        <#elseif list.cls = "string">
        bytes:WriteString(v);
        <#elseif list.cls = "bool">
        bytes:WriteBool(v);
        <#elseif list.cls = "short">
        bytes:WriteShort(v);
        <#else>
        bytes:WriteBytes(v:WriteWithType());
        </#if>
    end
    </#list>
    return bytes;
end

function ${message.name}:ParseData(csBytes)
    local bytes = New(ByteArray);
    bytes:WriteBuf(csBytes);
    bytes:SetPos(1);
    self:Read(bytes);
    Delete(bytes);
end

function ${message.name}:GetData()
    return self:Write():GetBytes();
end
</#list>
