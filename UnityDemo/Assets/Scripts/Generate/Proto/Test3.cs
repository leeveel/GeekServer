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
	
    public class Test3 : BaseMessage
	{
		public string UserId { get; set; }
		public string Platform { get; set; }
		public List<Geek.Server.Proto.Test1> List { get; set; } = new List<Geek.Server.Proto.Test1>();

		public Dictionary<int, int> Map = new Dictionary<int, int>();


		public Dictionary<int, Geek.Server.Proto.Test1> Map2 = new Dictionary<int, Geek.Server.Proto.Test1>();


		public Dictionary<int, List<Test1>> Map3 = new Dictionary<int, List<Test1>>();


		public Dictionary<int, HashSet<Test1>> Map4 = new Dictionary<int, HashSet<Test1>>();


		public Dictionary<int, Dictionary<long, Geek.Server.Proto.Test1>> Map5 = new Dictionary<int, Dictionary<long, Geek.Server.Proto.Test1>>();

		public Geek.Server.Proto.Test1  T1 { get; set; }
		public Geek.Server.Proto.Test1 T2 { get; set; }
		
		public const int MsgID = SID;
		public override int Sid { get;} = 111103;
		public const int SID = 111103;

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

					//UserId = SerializeTool.Read_string(false,  _buffer_, ref _offset_);


					UserId = XBuffer.ReadString(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 1)
				{

					//Platform = SerializeTool.Read_string(false,  _buffer_, ref _offset_);


					Platform = XBuffer.ReadString(_buffer_, ref _offset_);




				}else break;
				if(_fieldNum_ > 2)
				{
					
					/*********************************************************/
					int count2 = XBuffer.ReadInt(_buffer_, ref _offset_);
					for (int i = 0; i < count2; ++i)
					{
						var sid = XBuffer.ReadInt(_buffer_, ref _offset_);
						if (sid <= 0)
						{
							List.Add(default);
							continue;
						}
						var val = Create<Geek.Server.Proto.Test1>(sid);
						_offset_ = val.Read(_buffer_, _offset_);
						List.Add(val);
					}
					/*********************************************************/


				}else break;
				if(_fieldNum_ > 3)
				{

					
					/*********************************************************/
					int count3 = XBuffer.ReadInt(_buffer_, ref _offset_);
					for (int i = 0; i < count3; ++i)
					{

						var key = XBuffer.ReadInt(_buffer_, ref _offset_);

						
						var val = XBuffer.ReadInt(_buffer_, ref _offset_);

						Map.Add(key, val);
					}
					/*********************************************************/



				}else break;
				if(_fieldNum_ > 4)
				{

					
					/*********************************************************/
					//SerializeTool.Read_int_CustomMap<Geek.Server.Proto.Test1>(Map2, _buffer_, ref _offset_);
					int count4 = XBuffer.ReadInt(_buffer_, ref _offset_);
					for (int i = 0; i < count4; ++i)
					{
						var key = XBuffer.ReadInt(_buffer_, ref _offset_);

						var sid = XBuffer.ReadInt(_buffer_, ref _offset_);
						if (sid <= 0)
						{
							Map2[key] = default;
							continue;
						}
						var val = Create<Geek.Server.Proto.Test1>(sid);
						_offset_ = val.Read(_buffer_, _offset_);
						Map2.Add(key, val);
					}
					/*********************************************************/



				}else break;
				if(_fieldNum_ > 5)
				{



					/*********************************************************/
					int count5 = XBuffer.ReadInt(_buffer_, ref _offset_);
					for (int i = 0; i < count5; ++i)
					{
						var key = XBuffer.ReadInt(_buffer_, ref _offset_);

						var val = new List<Test1>(); //TODO:类型处理

						int count52 = XBuffer.ReadInt(_buffer_, ref _offset_);
						for (int j = 0; j < count52; ++j)
						{
							var sid = XBuffer.ReadInt(_buffer_, ref _offset_);
							if (sid <= 0)
							{
								val.Add(default);
								continue;
							}
							var val2 = Create<Test1>(sid);
							_offset_ = val2.Read(_buffer_, _offset_);
							val.Add(val2);
						}
						Map3.Add(key, val);
					}
					/*********************************************************/



				}else break;
				if(_fieldNum_ > 6)
				{



					/*********************************************************/
					int count6 = XBuffer.ReadInt(_buffer_, ref _offset_);
					for (int i = 0; i < count6; ++i)
					{
						var key = XBuffer.ReadInt(_buffer_, ref _offset_);

						var val = new HashSet<Test1>(); //TODO:类型处理

						int count62 = XBuffer.ReadInt(_buffer_, ref _offset_);
						for (int j = 0; j < count62; ++j)
						{
							var sid = XBuffer.ReadInt(_buffer_, ref _offset_);
							if (sid <= 0)
							{
								val.Add(default);
								continue;
							}
							var val2 = Create<Test1>(sid);
							_offset_ = val2.Read(_buffer_, _offset_);
							val.Add(val2);
						}
						Map4.Add(key, val);
					}
					/*********************************************************/



				}else break;
				if(_fieldNum_ > 7)
				{


					/*********************************************************/
					//SerializeTool.Read_int_long_NestCustomMap<Geek.Server.Proto.Test1>(Map5, _buffer_, ref _offset_);
					int count7 = XBuffer.ReadInt(_buffer_, ref _offset_);
					for (int i = 0; i < count7; ++i)
					{
						var key = XBuffer.ReadInt(_buffer_, ref _offset_);
						var val = new Dictionary<long, Geek.Server.Proto.Test1>(); //TODO:类型处理
						//ReadCustomMap(val, buffer, ref offset);
						int count72 = XBuffer.ReadInt(_buffer_, ref _offset_);
						for (int j = 0; j < count72; ++j)
						{
							var key2 = XBuffer.ReadLong(_buffer_, ref _offset_);

							var sid = XBuffer.ReadInt(_buffer_, ref _offset_);
							if (sid <= 0)
							{
								val[key2] = default;
								continue;
							}
							var val2 = Create<Geek.Server.Proto.Test1>(sid);
							_offset_ = val2.Read(_buffer_, _offset_);
							val.Add(key2, val2);
						}
						Map5.Add(key, val);
					}
					/*********************************************************/



				}else break;
				if(_fieldNum_ > 8)
				{

					T1 = ReadCustom<Geek.Server.Proto.Test1>(T1,true,  _buffer_, ref _offset_);


				}else break;
				if(_fieldNum_ > 9)
				{

					T2 = ReadCustom<Geek.Server.Proto.Test1>(T2,true,  _buffer_, ref _offset_);


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
			XBuffer.WriteByte(10, _buffer_, ref _offset_);
			
			//写入数据

			


			XBuffer.WriteString(UserId, _buffer_, ref _offset_);
            



			


			XBuffer.WriteString(Platform, _buffer_, ref _offset_);
            




			/*********************************************************/
			XBuffer.WriteInt(List.Count, _buffer_, ref _offset_);
            for (int i=0; i<List.Count; i++)
            {
                if (List[i] == null)
                {
                    UnityEngine.Debug.LogError("App.Proto.Test3.List has null item, idx == " + i);
                    XBuffer.WriteInt(0, _buffer_, ref _offset_);
                }
                else
                {
                    XBuffer.WriteInt(List[i].Sid, _buffer_, ref _offset_);
                    _offset_ = List[i].Write(_buffer_, _offset_);
                }
            }
			/*********************************************************/
			


			
			/*********************************************************/
			//_offset_ = SerializeTool.WritePrimitiveMap(Map, _buffer_, ref _offset_);
			XBuffer.WriteInt(Map.Count, _buffer_, ref _offset_);
            foreach (var kv in Map)
            {
				XBuffer.WriteInt(kv.Key, _buffer_, ref _offset_);

				XBuffer.WriteInt(kv.Value, _buffer_, ref _offset_);
            }
			/*********************************************************/




			
			/*********************************************************/
			XBuffer.WriteInt(Map2.Count, _buffer_, ref _offset_);
            foreach (var kv in Map2)
            {
				XBuffer.WriteInt(kv.Key, _buffer_, ref _offset_);

                if (kv.Value == null)
                {
                    UnityEngine.Debug.LogError($"{this.GetType().FullName}.Map2 has null item: {kv.Key}");
                    XBuffer.WriteInt(0, _buffer_, ref _offset_);
                }
                else
                {
                    XBuffer.WriteInt(kv.Value.Sid, _buffer_, ref _offset_);
                    _offset_ = kv.Value.Write(_buffer_, _offset_);
                }
            }
			/*********************************************************/






			/*********************************************************/
			XBuffer.WriteInt(Map3.Count, _buffer_, ref _offset_);
            foreach (var kv in Map3)
            {
				XBuffer.WriteInt(kv.Key, _buffer_, ref _offset_);

				XBuffer.WriteInt(kv.Value.Count, _buffer_, ref _offset_);
				foreach (var item in kv.Value)
				{
					if (item == null)
					{
						UnityEngine.Debug.LogError($"{this.GetType().FullName}.Map3.{kv.Key} has null item");
						XBuffer.WriteInt(0, _buffer_, ref _offset_);
					}
					else
					{
						XBuffer.WriteInt(item.Sid, _buffer_, ref _offset_);
						_offset_ = item.Write(_buffer_, _offset_);
					}
				}
            }
			/*********************************************************/






			/*********************************************************/
			XBuffer.WriteInt(Map4.Count, _buffer_, ref _offset_);
            foreach (var kv in Map4)
            {
				XBuffer.WriteInt(kv.Key, _buffer_, ref _offset_);

				XBuffer.WriteInt(kv.Value.Count, _buffer_, ref _offset_);
				foreach (var item in kv.Value)
				{
					if (item == null)
					{
						UnityEngine.Debug.LogError($"{this.GetType().FullName}.Map4.{kv.Key} has null item");
						XBuffer.WriteInt(0, _buffer_, ref _offset_);
					}
					else
					{
						XBuffer.WriteInt(item.Sid, _buffer_, ref _offset_);
						_offset_ = item.Write(_buffer_, _offset_);
					}
				}
            }
			/*********************************************************/





			/*********************************************************/
			//_offset_ = SerializeTool.WriteNestCustomMap(Map5, _buffer_, ref _offset_);
			XBuffer.WriteInt(Map5.Count, _buffer_, ref _offset_);
            foreach (var kv in Map5)
            {
				XBuffer.WriteInt(kv.Key, _buffer_, ref _offset_);

				XBuffer.WriteInt(kv.Value.Count, _buffer_, ref _offset_);
				foreach (var kv2 in kv.Value)
				{
					XBuffer.WriteLong(kv2.Key, _buffer_, ref _offset_);

					if (kv.Value == null)
					{
						UnityEngine.Debug.LogError($"{this.GetType().FullName}.Map5 has null item: {kv2.Key.ToString()}");
						XBuffer.WriteInt(0, _buffer_, ref _offset_);
					}
					else
					{
						XBuffer.WriteInt(kv2.Value.Sid, _buffer_, ref _offset_);
						_offset_ = kv2.Value.Write(_buffer_, _offset_);
					}
				}
            }
			/*********************************************************/



			
			_offset_ = WriteCustom<Geek.Server.Proto.Test1>(T1,true, _buffer_, ref _offset_);


			
			_offset_ = WriteCustom<Geek.Server.Proto.Test1>(T2,true, _buffer_, ref _offset_);

			
			return _offset_;
		}
	}
}