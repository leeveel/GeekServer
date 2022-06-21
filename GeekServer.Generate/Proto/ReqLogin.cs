//auto generated, do not modify it
//限制：命名不能以下划线结尾(可能冲突)
//限制：map的key只支持基础类型和string；list/map不能optional,list/map不能嵌套
//兼容限制：字段只能添加，添加后不能删除，添加字段只能添加到最后,添加消息类型只能添加到最后
//兼容限制：不能修改字段类型（如从bool改为long）
//兼容限制：消息类型(含msdId)不能作为其他消息的成员类型
using System;
using System.Text;
using Geek.Server;
using System.Collections.Generic;

///<summary></summary>
namespace Geek.Server.Proto
{


    public class ReqLogin : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();


		/*********************************************************/
		public string  UserName {get;set;}
		public string Platform {get;set;}
		public int SdkType {get;set;}
		public string SdkToken {get;set;}
		public string Device {get;set;}
		/*********************************************************/


		public const int MsgID = SID;
		public override int Sid { get;} = 111101;
		public const int SID = 111101;
		public const bool IsState = false;

		public override T Create<T>(int sid)
        {
            return Geek.Server.Proto.SClassFactory.Create<T>(sid);
        }

		///<summary>反序列化，读取数据</summary>
        public override int Read(Span<byte> _buffer_, int _offset_)
		{
			UniId = XBuffer.ReadInt(_buffer_, ref _offset_);
			_offset_ = base.Read(_buffer_, _offset_);
			
			//字段个数,最多支持255个
			var _fieldNum_ = XBuffer.ReadByte(_buffer_, ref _offset_);
			
			do {
				if(_fieldNum_ > 0)
				{

					//UserName = SerializeTool.Read_string(false,  _buffer_, ref _offset_);


					UserName = XBuffer.ReadString(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 1)
				{

					//Platform = SerializeTool.Read_string(false,  _buffer_, ref _offset_);


					Platform = XBuffer.ReadString(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 2)
				{

					//SdkType = SerializeTool.Read_int(false,  _buffer_, ref _offset_);


					SdkType = XBuffer.ReadInt(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 3)
				{

					//SdkToken = SerializeTool.Read_string(false,  _buffer_, ref _offset_);


					SdkToken = XBuffer.ReadString(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 4)
				{

					//Device = SerializeTool.Read_string(false,  _buffer_, ref _offset_);


					Device = XBuffer.ReadString(_buffer_, ref _offset_);




				}else break;
			}while(false);
			
			return _offset_;
		}

		
		///<summary>序列化，写入数据</summary>
        public override int Write(Span<byte> _buffer_, int _offset_)
        {	
			XBuffer.WriteInt(UniId, _buffer_, ref _offset_);
			_offset_ = base.Write(_buffer_, _offset_);
			
			//写入字段数量,最多支持255个
			XBuffer.WriteByte(5, _buffer_, ref _offset_);
			
			//写入数据

			


			XBuffer.WriteString(UserName, _buffer_, ref _offset_);
            



			


			XBuffer.WriteString(Platform, _buffer_, ref _offset_);
            



			


			XBuffer.WriteInt(SdkType, _buffer_, ref _offset_);
            



			


			XBuffer.WriteString(SdkToken, _buffer_, ref _offset_);
            



			


			XBuffer.WriteString(Device, _buffer_, ref _offset_);
            


			
			return _offset_;
		}



		/*********************************************************/
		public  override int GetSerializeLength()
		{
			int len = XBuffer.IntSize + XBuffer.ByteSize; //UniId + _fieldNum_
	
			
					len += XBuffer.ShortSize + (string.IsNullOrEmpty(UserName)?0:Encoding.UTF8.GetByteCount(UserName)); //UserName

			
					len += XBuffer.ShortSize + (string.IsNullOrEmpty(Platform)?0:Encoding.UTF8.GetByteCount(Platform)); //Platform

			
					len += XBuffer.IntSize;	//SdkType

			
					len += XBuffer.ShortSize + (string.IsNullOrEmpty(SdkToken)?0:Encoding.UTF8.GetByteCount(SdkToken)); //SdkToken

			
					len += XBuffer.ShortSize + (string.IsNullOrEmpty(Device)?0:Encoding.UTF8.GetByteCount(Device)); //Device

			return len;
		}
	}
}