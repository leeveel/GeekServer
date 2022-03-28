//auto generated, do not modify it
//限制：命名不能以下划线结尾(可能冲突)
//限制：map的key只支持基础类型和string；list/map不能optional,list/map不能嵌套
//兼容限制：字段只能添加，添加后不能删除，添加字段只能添加到最后,添加消息类型只能添加到最后
//兼容限制：不能修改字段类型（如从bool改为long）
//兼容限制：消息类型(含msdId)不能作为其他消息的成员类型

using Geek.Server;
using System.Collections.Generic;

///<summary></summary>
namespace Geek.Server.Proto
{
	
    public class UserInfo : Serializable
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

		public string RoleName { get; set; }
		public long RoleId { get; set; }
		public int Level { get; set; }
		public long CreateTime { get; set; }
		public int VipLevel { get; set; }
		
		public override int Sid { get;} = 111100;
		public const int SID = 111100;

		public override T Create<T>(int sid)
        {
            return Geek.Server.Proto.SClassFactory.Create<T>(sid);
        }

		///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
		{
			_offset_ = base.Read(_buffer_, _offset_);
			int _startOffset_ = _offset_;
			int _toReadLength_ = XBuffer.ReadInt(_buffer_, ref _offset_);
			
			//字段个数,最多支持255个
			var _fieldNum_ = XBuffer.ReadByte(_buffer_, ref _offset_);
			
			do {
				if(_fieldNum_ > 0)
				{

					//RoleName = SerializeTool.Read_string(false,  _buffer_, ref _offset_);


					RoleName = XBuffer.ReadString(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 1)
				{

					//RoleId = SerializeTool.Read_long(false,  _buffer_, ref _offset_);


					RoleId = XBuffer.ReadLong(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 2)
				{

					//Level = SerializeTool.Read_int(false,  _buffer_, ref _offset_);


					Level = XBuffer.ReadInt(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 3)
				{

					//CreateTime = SerializeTool.Read_long(false,  _buffer_, ref _offset_);


					CreateTime = XBuffer.ReadLong(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 4)
				{

					//VipLevel = SerializeTool.Read_int(false,  _buffer_, ref _offset_);


					VipLevel = XBuffer.ReadInt(_buffer_, ref _offset_);




				}else break;
			}while(false);
			
			//剔除多余数据
			if(_offset_ < _toReadLength_ - _startOffset_)
				_offset_ += _toReadLength_ - _startOffset_;
			return _offset_;
		}

		
		///<summary>序列化，写入数据</summary>
        public override int Write(byte[] _buffer_, int _offset_)
        {	
			_offset_ = base.Write(_buffer_, _offset_);
			//先写入当前对象长度占位符
			int _startOffset_ = _offset_;
			XBuffer.WriteInt(0, _buffer_, ref _offset_);
			
			//写入字段数量,最多支持255个
			XBuffer.WriteByte(5, _buffer_, ref _offset_);
			
			//写入数据

			


			XBuffer.WriteString(RoleName, _buffer_, ref _offset_);
            



			


			XBuffer.WriteLong(RoleId, _buffer_, ref _offset_);
            



			


			XBuffer.WriteInt(Level, _buffer_, ref _offset_);
            



			


			XBuffer.WriteLong(CreateTime, _buffer_, ref _offset_);
            



			


			XBuffer.WriteInt(VipLevel, _buffer_, ref _offset_);
            


			
			//覆盖当前对象长度
			XBuffer.WriteInt(_offset_ - _startOffset_, _buffer_, ref _startOffset_);
			return _offset_;
		}
	}
}