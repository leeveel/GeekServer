//auto generated, do not modify it
//限制：命名不能以下划线结尾(可能冲突)
//限制：不能跨协议文件继承,不能跨文件使用继承关系
//限制：map的key只支持short, int, long, string；list/map不能optional,list/map不能嵌套
//兼容限制：字段只能添加，添加后不能删除，添加字段只能添加到最后,添加消息类型只能添加到最后
//兼容限制：不能修改字段类型（如从bool改为long）
//兼容限制：消息类型(含msdId)不能作为其他消息的成员类型
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Attributes;

///<summary>举例各种结构写法</summary>
namespace Geek.Server.Message.Sample
{
	internal class SampleMsgFactory
	{
		///<summary>通过msgIdx构造msg</summary>
		public static BaseMessage Create(int msgIdx)
		{
			switch(msgIdx)
			{
				case 1: return new Test1();
				case 2: return new Test2();
				case 3: return new ReqTest();
				default: return default;
			}
		}
	}
	
	
	///<summary>测试类型1</summary>
    public class Test1 : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 1;//最多支持255个消息类型
		
		///<summary>id</summary>
		public long id_f { get{ return _id_f_; } set{ _id_f_ = value; _stateChanged = true; } }
		long _id_f_;

		public string s1 { get{ return _s1_; } set{ _s1_ = value; _stateChanged = true; } }
		string _s1_;

		public int i1 { get{ return _i1_; } set{ _i1_ = value; _stateChanged = true; } }
		int _i1_;

		public bool b1 { get{ return _b1_; } set{ _b1_ = value; _stateChanged = true; } }
		bool _b1_;

		public float f1 { get{ return _f1_; } set{ _f1_ = value; _stateChanged = true; } }
		float _f1_;

		public short s2 { get{ return _s2_; } set{ _s2_ = value; _stateChanged = true; } }
		short _s2_;

		public double d1 { get{ return _d1_; } set{ _d1_ = value; _stateChanged = true; } }
		double _d1_;

		public byte[] b2 { get{ return _b2_; } set{ _b2_ = value; _stateChanged = true; } }
		byte[] _b2_;

		///<summary>测试option</summary>
		public string o1 { get{ return _o1_; } set{ _o1_ = value; _stateChanged = true; } }
		string _o1_;


		
		///<summary>状态是否改变</summary>
		public override bool IsChanged
		{
			get
			{
				if(_stateChanged)
					return true;
				return base.IsChanged;
			}
		}
		
		///<summary>清除所有改变[含子项]</summary>
		public override void ClearChanges()
		{
			base.ClearChanges();
			//_stateChanged = false;
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
				if(_fieldNum_ > 0){
					id_f = XBuffer.ReadLong(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 1){
					s1 = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 2){
					i1 = XBuffer.ReadInt(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 3){
					b1 = XBuffer.ReadBool(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 4){
					f1 = XBuffer.ReadFloat(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 5){
					s2 = XBuffer.ReadShort(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 6){
					d1 = XBuffer.ReadDouble(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 7){
					b2 = XBuffer.ReadBytes(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 8){
					o1 = XBuffer.ReadString(_buffer_, ref _offset_);
					
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
			XBuffer.WriteLong(id_f, _buffer_, ref _offset_);
			XBuffer.WriteString(s1, _buffer_, ref _offset_);
			XBuffer.WriteInt(i1, _buffer_, ref _offset_);
			XBuffer.WriteBool(b1, _buffer_, ref _offset_);
			XBuffer.WriteFloat(f1, _buffer_, ref _offset_);
			XBuffer.WriteShort(s2, _buffer_, ref _offset_);
			XBuffer.WriteDouble(d1, _buffer_, ref _offset_);
			XBuffer.WriteBytes(b2, _buffer_, ref _offset_);
			XBuffer.WriteString(o1, _buffer_, ref _offset_);
			
			//覆盖当前对象长度
			XBuffer.WriteInt(_offset_ - _startOffset_, _buffer_, ref _startOffset_);
			return _offset_;
		}
	}

	///<summary>测试继承类型</summary>
    public class Test2 : Test1
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal override byte _msgIdx_ => 2;//最多支持255个消息类型
		
		public long l1 { get{ return _l1_; } set{ _l1_ = value; _stateChanged = true; } }
		long _l1_;

		public StateList<string> l2 { get; private set; } = new StateList<string>();

		public StateList<float> l3 { get; private set; } = new StateList<float>();

		public StateList<Test1> l4 { get; private set; } = new StateList<Test1>();

		[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
		public StateMap<long, string> m1 { get; private set; } = new StateMap<long, string>();

		[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
		public StateMap<int, Test1> m2 { get; private set; } = new StateMap<int, Test1>();

		///<summary>测试option</summary>
		public long l5 { get{ return _l5_; } set{ _l5_ = value; _stateChanged = true; } }
		long _l5_;

		///<summary>测试option</summary>
		public Test1 t1 { get{ return _t1_; } set{ _t1_ = value; _stateChanged = true; } }
		Test1 _t1_;


		
		///<summary>状态是否改变</summary>
		public override bool IsChanged
		{
			get
			{
				if(_stateChanged)
					return true;
				if(l2.IsChanged)
					return true;
				if(l3.IsChanged)
					return true;
				if(l4.IsChanged)
					return true;
				if(m1.IsChanged)
					return true;
				if(m2.IsChanged)
					return true;
				if(t1.IsChanged)
					return true;
				return base.IsChanged;
			}
		}
		
		///<summary>清除所有改变[含子项]</summary>
		public override void ClearChanges()
		{
			base.ClearChanges();
			//_stateChanged = false;
			l2.ClearChanges();
			l3.ClearChanges();
			l4.ClearChanges();
			m1.ClearChanges();
			m2.ClearChanges();
			t1.ClearChanges();
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
				if(_fieldNum_ > 0){
					l1 = XBuffer.ReadLong(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 1){
					short _count_ = XBuffer.ReadShort(_buffer_, ref _offset_);
					for(int _a_ = 0; _a_ < _count_; ++_a_)
					{
						l2.Add(XBuffer.ReadString(_buffer_, ref _offset_));
					}

				}else break;
				if(_fieldNum_ > 2){
					short _count_ = XBuffer.ReadShort(_buffer_, ref _offset_);
					for(int _a_ = 0; _a_ < _count_; ++_a_)
					{
						l3.Add(XBuffer.ReadFloat(_buffer_, ref _offset_));
					}

				}else break;
				if(_fieldNum_ > 3){
					short _count_ = XBuffer.ReadShort(_buffer_, ref _offset_);
					for(int _a_ = 0; _a_ < _count_; ++_a_)
					{
						var _idx_ = XBuffer.ReadByte(_buffer_, ref _offset_);
						if(_idx_ <= 0)
						{
							l4.Add(default);
							continue;//为空
						}
						var _val_ = (Test1)SampleMsgFactory.Create(_idx_);
						_offset_ = _val_.Read(_buffer_, _offset_);
						l4.Add(_val_);
					}

				}else break;
				if(_fieldNum_ > 4){
					short _count_ = XBuffer.ReadShort(_buffer_, ref _offset_);
					for(int _a_ = 0; _a_ < _count_; ++_a_)
					{
						var _key_ = XBuffer.ReadLong(_buffer_, ref _offset_);
						var _val_ = XBuffer.ReadString(_buffer_, ref _offset_);
						m1[_key_] = _val_;
					}

				}else break;
				if(_fieldNum_ > 5){
					short _count_ = XBuffer.ReadShort(_buffer_, ref _offset_);
					for(int _a_ = 0; _a_ < _count_; ++_a_)
					{
						var _key_ = XBuffer.ReadInt(_buffer_, ref _offset_);
						var _idx_ = XBuffer.ReadByte(_buffer_, ref _offset_);
						if(_idx_ <= 0)
						{
							m2[_key_] = default;
							continue;//为空
						}
						var _val_ = (Test1)SampleMsgFactory.Create(_idx_);
						_offset_ = _val_.Read(_buffer_, _offset_);
						m2[_key_] = _val_;
					}

				}else break;
				if(_fieldNum_ > 6){
					l5 = XBuffer.ReadLong(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 7){
					var _idx_ = XBuffer.ReadByte(_buffer_, ref _offset_);
					t1 = (Test1)SampleMsgFactory.Create(_idx_);
					_offset_ = t1.Read(_buffer_, _offset_);
					
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
			XBuffer.WriteByte(8, _buffer_, ref _offset_);
			
			//写入数据
			XBuffer.WriteLong(l1, _buffer_, ref _offset_);
			short _listCount_ = (short)l2.Count;
			XBuffer.WriteShort(_listCount_, _buffer_, ref _offset_);
			for(int _a_=0; _a_<_listCount_; ++_a_)
			{
				XBuffer.WriteString(l2[_a_], _buffer_, ref _offset_);
			}
			
			
			_listCount_ = (short)l3.Count;
			XBuffer.WriteShort(_listCount_, _buffer_, ref _offset_);
			for(int _a_=0; _a_<_listCount_; ++_a_)
			{
				XBuffer.WriteFloat(l3[_a_], _buffer_, ref _offset_);
			}
			
			
			_listCount_ = (short)l4.Count;
			XBuffer.WriteShort(_listCount_, _buffer_, ref _offset_);
			for(int _a_=0; _a_<_listCount_; ++_a_)
			{
				if(l4[_a_] == null)
				{
					LOGGER.Error("l4 has null item, idx == " + _a_);
					XBuffer.WriteByte(0, _buffer_, ref _offset_);
				}
				else
				{
					XBuffer.WriteByte(l4[_a_]._msgIdx_, _buffer_, ref _offset_);
					_offset_ = l4[_a_].Write(_buffer_, _offset_);
				}
			}
			
			
			_listCount_ = (short)m1.Count;
			XBuffer.WriteShort(_listCount_, _buffer_, ref _offset_);
			foreach(var kv in m1)
			{
				XBuffer.WriteLong(kv.Key, _buffer_, ref _offset_);
				XBuffer.WriteString(kv.Value, _buffer_, ref _offset_);
			}
			
			_listCount_ = (short)m2.Count;
			XBuffer.WriteShort(_listCount_, _buffer_, ref _offset_);
			foreach(var kv in m2)
			{
				XBuffer.WriteInt(kv.Key, _buffer_, ref _offset_);
				if(kv.Value == null)
				{
					LOGGER.Error("m2 has null value, key == " + kv.Key);
					XBuffer.WriteByte(0, _buffer_, ref _offset_);
				}
				else
				{
					XBuffer.WriteByte(kv.Value._msgIdx_, _buffer_, ref _offset_);
					_offset_ = kv.Value.Write(_buffer_, _offset_);
				}
			}
			
			XBuffer.WriteLong(l5, _buffer_, ref _offset_);
			XBuffer.WriteByte(t1._msgIdx_, _buffer_, ref _offset_);
			_offset_ = t1.Write(_buffer_, _offset_);
			
			//覆盖当前对象长度
			XBuffer.WriteInt(_offset_ - _startOffset_, _buffer_, ref _startOffset_);
			return _offset_;
		}
	}

	///<summary>登陆</summary>
    public class ReqTest : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 3;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 111101;
		
		///<summary>登陆用户id</summary>
		public string userId { get{ return _userId_; } set{ _userId_ = value; _stateChanged = true; } }
		string _userId_;

		///<summary>平台</summary>
		public string platform { get{ return _platform_; } set{ _platform_ = value; _stateChanged = true; } }
		string _platform_;

		///<summary>数据结构</summary>
		public Test2 t { get{ return _t_; } set{ _t_ = value; _stateChanged = true; } }
		Test2 _t_;


		
		///<summary>状态是否改变</summary>
		public override bool IsChanged
		{
			get
			{
				if(_stateChanged)
					return true;
				if(t.IsChanged)
					return true;
				return base.IsChanged;
			}
		}
		
		///<summary>清除所有改变[含子项]</summary>
		public override void ClearChanges()
		{
			base.ClearChanges();
			//_stateChanged = false;
			t.ClearChanges();
		}
		
		
		///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
		{
			_offset_ = base.Read(_buffer_, _offset_);
			
			//字段个数,最多支持255个
			var _fieldNum_ = XBuffer.ReadByte(_buffer_, ref _offset_);
			
			do {
				if(_fieldNum_ > 0){
					userId = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 1){
					platform = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 2){
					var _idx_ = XBuffer.ReadByte(_buffer_, ref _offset_);
					t = new Test2();
					_offset_ = t.Read(_buffer_, _offset_);
					
				}else break;
			}while(false);
			
			return _offset_;
		}
		
		///<summary>序列化，写入数据</summary>
        public override int Write(byte[] _buffer_, int _offset_)
        {	
			_offset_ = base.Write(_buffer_, _offset_);
			
			//写入字段数量,最多支持255个
			XBuffer.WriteByte(3, _buffer_, ref _offset_);
			
			//写入数据
			XBuffer.WriteString(userId, _buffer_, ref _offset_);
			XBuffer.WriteString(platform, _buffer_, ref _offset_);
			XBuffer.WriteByte(t._msgIdx_, _buffer_, ref _offset_);
			_offset_ = t.Write(_buffer_, _offset_);
			
			return _offset_;
		}
	}
}