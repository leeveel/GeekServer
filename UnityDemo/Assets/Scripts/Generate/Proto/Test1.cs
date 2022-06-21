//auto generated, do not modify it
//限制：命名不能以下划线结尾(可能冲突)
//限制：map的key只支持基础类型和string；list/map不能optional,list/map不能嵌套
//兼容限制：字段只能添加，添加后不能删除，添加字段只能添加到最后,添加消息类型只能添加到最后
//兼容限制：不能修改字段类型（如从bool改为long）
//兼容限制：消息类型(含msdId)不能作为其他消息的成员类型

using Geek.Client;
using System.Collections.Generic;

///<summary></summary>
namespace Geek.Server.Proto
{
	
    public class Test1 : Serializable
	{
		public long Id { get; set; }
		public string S1 { get; set; }
		public int I1 { get; set; }
		public bool B1 { get; set; }
		public float F1 { get; set; }
		public short S2 { get; set; }
		public double D1 { get; set; }
		public byte[] B2 { get; set; }
		public string O1 { get; set; }
		
		public override int Sid { get;} = 111101;
		public const int SID = 111101;

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

					//Id = SerializeTool.Read_long(false,  _buffer_, ref _offset_);


					Id = XBuffer.ReadLong(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 1)
				{

					//S1 = SerializeTool.Read_string(false,  _buffer_, ref _offset_);


					S1 = XBuffer.ReadString(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 2)
				{

					//I1 = SerializeTool.Read_int(false,  _buffer_, ref _offset_);


					I1 = XBuffer.ReadInt(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 3)
				{

					//B1 = SerializeTool.Read_bool(false,  _buffer_, ref _offset_);


					B1 = XBuffer.ReadBool(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 4)
				{

					//F1 = SerializeTool.Read_float(false,  _buffer_, ref _offset_);


					F1 = XBuffer.ReadFloat(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 5)
				{

					//S2 = SerializeTool.Read_short(false,  _buffer_, ref _offset_);


					S2 = XBuffer.ReadShort(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 6)
				{

					//D1 = SerializeTool.Read_double(false,  _buffer_, ref _offset_);


					D1 = XBuffer.ReadDouble(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 7)
				{

					//B2 = SerializeTool.Read_byte[](false,  _buffer_, ref _offset_);


					B2 = XBuffer.ReadBytes(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 8)
				{

					//O1 = SerializeTool.Read_string(true,  _buffer_, ref _offset_);

					var hasVal8 = XBuffer.ReadBool(_buffer_, ref _offset_);
					if (hasVal8)
						O1 = XBuffer.ReadString(_buffer_, ref _offset_);
					else
						O1 = default;



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
			XBuffer.WriteByte(9, _buffer_, ref _offset_);
			
			//写入数据

			


			XBuffer.WriteLong(Id, _buffer_, ref _offset_);
            



			


			XBuffer.WriteString(S1, _buffer_, ref _offset_);
            



			


			XBuffer.WriteInt(I1, _buffer_, ref _offset_);
            



			


			XBuffer.WriteBool(B1, _buffer_, ref _offset_);
            



			


			XBuffer.WriteFloat(F1, _buffer_, ref _offset_);
            



			


			XBuffer.WriteShort(S2, _buffer_, ref _offset_);
            



			


			XBuffer.WriteDouble(D1, _buffer_, ref _offset_);
            



			


			XBuffer.WriteBytes(B2, _buffer_, ref _offset_);
            



			

			bool hasVal8 = O1 != default;
            XBuffer.WriteBool(hasVal8, _buffer_, ref _offset_);
            if (hasVal8)
                XBuffer.WriteString(O1, _buffer_, ref _offset_);


			
			//覆盖当前对象长度
			XBuffer.WriteInt(_offset_ - _startOffset_, _buffer_, ref _startOffset_);
			return _offset_;
		}
	}
}