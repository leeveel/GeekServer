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
	
    public class Test2 : BaseMessage
	{
		public long L1 { get; set; }
		public List<string> L2 { get; set; } = new List<string>();
		public List<float> L3 { get; set; } = new List<float>();
		public List<Geek.Server.Proto.Test1> L4 { get; set; } = new List<Geek.Server.Proto.Test1>();

		public Dictionary<long, string> M1 = new Dictionary<long, string>();


		public Dictionary<int, Geek.Server.Proto.Test1> M2 = new Dictionary<int, Geek.Server.Proto.Test1>();

		public long  L5 { get; set; }
		public Geek.Server.Proto.Test1 T1 { get; set; }
		
		public const int MsgID = SID;
		public override int Sid { get;} = 111102;
		public const int SID = 111102;

		public override T Create<T>(int sid)
        {
            return Geek.Server.Proto.SClassFactory.Create<T>(sid);
        }

		///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
		{
			UniId = XBuffer.ReadInt(_buffer_, ref _offset_);
			_offset_ = base.Read(_buffer_, _offset_);
			
			//字段个数,最多支持255个
			var _fieldNum_ = XBuffer.ReadByte(_buffer_, ref _offset_);
			
			do {
				if(_fieldNum_ > 0)
				{

					//L1 = SerializeTool.Read_long(false,  _buffer_, ref _offset_);


					L1 = XBuffer.ReadLong(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 1)
				{
					
					/*********************************************************/
					int count1 = XBuffer.ReadInt(_buffer_, ref _offset_);
					for (int i = 0; i < count1; ++i)
					{
						L2.Add(XBuffer.ReadString(_buffer_, ref _offset_));
					}
					/*********************************************************/


				}else break;
				if(_fieldNum_ > 2)
				{
					
					/*********************************************************/
					int count2 = XBuffer.ReadInt(_buffer_, ref _offset_);
					for (int i = 0; i < count2; ++i)
					{
						L3.Add(XBuffer.ReadFloat(_buffer_, ref _offset_));
					}
					/*********************************************************/


				}else break;
				if(_fieldNum_ > 3)
				{
					
					/*********************************************************/
					int count3 = XBuffer.ReadInt(_buffer_, ref _offset_);
					for (int i = 0; i < count3; ++i)
					{
						var sid = XBuffer.ReadInt(_buffer_, ref _offset_);
						if (sid <= 0)
						{
							L4.Add(default);
							continue;
						}
						var val = Create<Geek.Server.Proto.Test1>(sid);
						_offset_ = val.Read(_buffer_, _offset_);
						L4.Add(val);
					}
					/*********************************************************/


				}else break;
				if(_fieldNum_ > 4)
				{

					
					/*********************************************************/
					int count4 = XBuffer.ReadInt(_buffer_, ref _offset_);
					for (int i = 0; i < count4; ++i)
					{

						var key = XBuffer.ReadLong(_buffer_, ref _offset_);

						
						var val = XBuffer.ReadString(_buffer_, ref _offset_);

						M1.Add(key, val);
					}
					/*********************************************************/



				}else break;
				if(_fieldNum_ > 5)
				{

					
					/*********************************************************/
					//SerializeTool.Read_int_CustomMap<Geek.Server.Proto.Test1>(M2, _buffer_, ref _offset_);
					int count5 = XBuffer.ReadInt(_buffer_, ref _offset_);
					for (int i = 0; i < count5; ++i)
					{
						var key = XBuffer.ReadInt(_buffer_, ref _offset_);

						var sid = XBuffer.ReadInt(_buffer_, ref _offset_);
						if (sid <= 0)
						{
							M2[key] = default;
							continue;
						}
						var val = Create<Geek.Server.Proto.Test1>(sid);
						_offset_ = val.Read(_buffer_, _offset_);
						M2.Add(key, val);
					}
					/*********************************************************/



				}else break;
				if(_fieldNum_ > 6)
				{

					//L5 = SerializeTool.Read_long(true,  _buffer_, ref _offset_);

					var hasVal6 = XBuffer.ReadBool(_buffer_, ref _offset_);
					if (hasVal6)
						L5 = XBuffer.ReadLong(_buffer_, ref _offset_);
					else
						L5 = default;



				}else break;
				if(_fieldNum_ > 7)
				{

					T1 = ReadCustom<Geek.Server.Proto.Test1>(T1,true,  _buffer_, ref _offset_);


				}else break;
			}while(false);
			
			return _offset_;
		}

		
		///<summary>序列化，写入数据</summary>
        public override int Write(byte[] _buffer_, int _offset_)
        {	
			XBuffer.WriteInt(UniId, _buffer_, ref _offset_);
			_offset_ = base.Write(_buffer_, _offset_);
			
			//写入字段数量,最多支持255个
			XBuffer.WriteByte(8, _buffer_, ref _offset_);
			
			//写入数据

			


			XBuffer.WriteLong(L1, _buffer_, ref _offset_);
            




			/*********************************************************/
			XBuffer.WriteInt(L2.Count, _buffer_, ref _offset_);
            foreach (var item in L2)
            {
				XBuffer.WriteString(item, _buffer_, ref _offset_);
            }
			/*********************************************************/
			


			/*********************************************************/
			XBuffer.WriteInt(L3.Count, _buffer_, ref _offset_);
            foreach (var item in L3)
            {
				XBuffer.WriteFloat(item, _buffer_, ref _offset_);
            }
			/*********************************************************/
			


			/*********************************************************/
			XBuffer.WriteInt(L4.Count, _buffer_, ref _offset_);
            for (int i=0; i<L4.Count; i++)
            {
                if (L4[i] == null)
                {
                    UnityEngine.Debug.LogError("App.Proto.Test3.List has null item, idx == " + i);
                    XBuffer.WriteInt(0, _buffer_, ref _offset_);
                }
                else
                {
                    XBuffer.WriteInt(L4[i].Sid, _buffer_, ref _offset_);
                    _offset_ = L4[i].Write(_buffer_, _offset_);
                }
            }
			/*********************************************************/
			


			
			/*********************************************************/
			//_offset_ = SerializeTool.WritePrimitiveMap(M1, _buffer_, ref _offset_);
			XBuffer.WriteInt(M1.Count, _buffer_, ref _offset_);
            foreach (var kv in M1)
            {
				XBuffer.WriteLong(kv.Key, _buffer_, ref _offset_);

				XBuffer.WriteString(kv.Value, _buffer_, ref _offset_);
            }
			/*********************************************************/




			
			/*********************************************************/
			XBuffer.WriteInt(M2.Count, _buffer_, ref _offset_);
            foreach (var kv in M2)
            {
				XBuffer.WriteInt(kv.Key, _buffer_, ref _offset_);

                if (kv.Value == null)
                {
                    UnityEngine.Debug.LogError($"{this.GetType().FullName}.M2 has null item: {kv.Key}");
                    XBuffer.WriteInt(0, _buffer_, ref _offset_);
                }
                else
                {
                    XBuffer.WriteInt(kv.Value.Sid, _buffer_, ref _offset_);
                    _offset_ = kv.Value.Write(_buffer_, _offset_);
                }
            }
			/*********************************************************/



			

			bool hasVal6 = L5 != default;
            XBuffer.WriteBool(hasVal6, _buffer_, ref _offset_);
            if (hasVal6)
                XBuffer.WriteLong(L5, _buffer_, ref _offset_);



			
			_offset_ = WriteCustom<Geek.Server.Proto.Test1>(T1,true, _buffer_, ref _offset_);

			
			return _offset_;
		}
	}
}