//auto generated, do not modify it
//限制：命名不能以下划线结尾(可能冲突)
//限制：不能跨协议文件继承,不能跨文件使用继承关系
//限制：map的key只支持short, int, long, string；list/map不能optional,list/map不能嵌套
//兼容限制：字段只能添加，添加后不能删除，添加字段只能添加到最后,添加消息类型只能添加到最后
//兼容限制：不能修改字段类型（如从bool改为long）
//兼容限制：消息类型(含msdId)不能作为其他消息的成员类型
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Attributes;

///<summary>背包</summary>
namespace Geek.Server.Message.DemoBag
{
	internal class DemoBagMsgFactory
	{
		///<summary>通过msgIdx构造msg</summary>
		public static BaseMessage Create(int msgIdx)
		{
			switch(msgIdx)
			{
				case 1: return new ReqBagInfo();
				case 2: return new ResBagInfo();
				case 3: return new ReqUseItem();
				case 4: return new ReqSellItem();
				case 5: return new ResItemChange();
				default: return default;
			}
		}
	}
	
	
	///<summary>背包</summary>
    public class ReqBagInfo : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 1;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 112001;
		

		
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
			
			//字段个数,最多支持255个
			var _fieldNum_ = XBuffer.ReadByte(_buffer_, ref _offset_);
			
			do {
			}while(false);
			
			return _offset_;
		}
		
		///<summary>序列化，写入数据</summary>
        public override int Write(byte[] _buffer_, int _offset_)
        {	
			_offset_ = base.Write(_buffer_, _offset_);
			
			//写入字段数量,最多支持255个
			XBuffer.WriteByte(0, _buffer_, ref _offset_);
			
			//写入数据
			
			return _offset_;
		}
	}

	///<summary>背包</summary>
    public class ResBagInfo : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 2;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 112002;
		
		///<summary>道具</summary>
		[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
		public StateMap<int, long> itemDic { get; set; } = new StateMap<int, long>();


		
		///<summary>状态是否改变</summary>
		public override bool IsChanged
		{
			get
			{
				if(_stateChanged)
					return true;
				if(itemDic.IsChanged)
					return true;
				return base.IsChanged;
			}
		}
		
		///<summary>清除所有改变[含子项]</summary>
		public override void ClearChanges()
		{
			base.ClearChanges();
			//_stateChanged = false;
			itemDic.ClearChanges();
		}
		
		
		///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
		{
			_offset_ = base.Read(_buffer_, _offset_);
			
			//字段个数,最多支持255个
			var _fieldNum_ = XBuffer.ReadByte(_buffer_, ref _offset_);
			
			do {
				if(_fieldNum_ > 0){
					short _count_ = XBuffer.ReadShort(_buffer_, ref _offset_);
					for(int _a_ = 0; _a_ < _count_; ++_a_)
					{
						var _key_ = XBuffer.ReadInt(_buffer_, ref _offset_);
						var _val_ = XBuffer.ReadLong(_buffer_, ref _offset_);
						itemDic[_key_] = _val_;
					}

				}else break;
			}while(false);
			
			return _offset_;
		}
		
		///<summary>序列化，写入数据</summary>
        public override int Write(byte[] _buffer_, int _offset_)
        {	
			_offset_ = base.Write(_buffer_, _offset_);
			
			//写入字段数量,最多支持255个
			XBuffer.WriteByte(1, _buffer_, ref _offset_);
			
			//写入数据
			short _listCount_ = (short)itemDic.Count;
			XBuffer.WriteShort(_listCount_, _buffer_, ref _offset_);
			foreach(var kv in itemDic)
			{
				XBuffer.WriteInt(kv.Key, _buffer_, ref _offset_);
				XBuffer.WriteLong(kv.Value, _buffer_, ref _offset_);
			}
			
			
			return _offset_;
		}
	}

	///<summary>使用道具</summary>
    public class ReqUseItem : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 3;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 112003;
		
		///<summary>道具id</summary>
		public int itemId { get{ return _itemId_; } set{ _itemId_ = value; _stateChanged = true; } }
		int _itemId_;


		
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
			
			//字段个数,最多支持255个
			var _fieldNum_ = XBuffer.ReadByte(_buffer_, ref _offset_);
			
			do {
				if(_fieldNum_ > 0){
					itemId = XBuffer.ReadInt(_buffer_, ref _offset_);
					
				}else break;
			}while(false);
			
			return _offset_;
		}
		
		///<summary>序列化，写入数据</summary>
        public override int Write(byte[] _buffer_, int _offset_)
        {	
			_offset_ = base.Write(_buffer_, _offset_);
			
			//写入字段数量,最多支持255个
			XBuffer.WriteByte(1, _buffer_, ref _offset_);
			
			//写入数据
			XBuffer.WriteInt(itemId, _buffer_, ref _offset_);
			
			return _offset_;
		}
	}

	///<summary>出售道具</summary>
    public class ReqSellItem : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 4;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 112004;
		
		///<summary>道具id</summary>
		public int itemId { get{ return _itemId_; } set{ _itemId_ = value; _stateChanged = true; } }
		int _itemId_;


		
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
			
			//字段个数,最多支持255个
			var _fieldNum_ = XBuffer.ReadByte(_buffer_, ref _offset_);
			
			do {
				if(_fieldNum_ > 0){
					itemId = XBuffer.ReadInt(_buffer_, ref _offset_);
					
				}else break;
			}while(false);
			
			return _offset_;
		}
		
		///<summary>序列化，写入数据</summary>
        public override int Write(byte[] _buffer_, int _offset_)
        {	
			_offset_ = base.Write(_buffer_, _offset_);
			
			//写入字段数量,最多支持255个
			XBuffer.WriteByte(1, _buffer_, ref _offset_);
			
			//写入数据
			XBuffer.WriteInt(itemId, _buffer_, ref _offset_);
			
			return _offset_;
		}
	}

	///<summary>道具变化</summary>
    public class ResItemChange : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 5;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 112005;
		
		///<summary>变化的道具</summary>
		[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
		public StateMap<int, long> itemDic { get; set; } = new StateMap<int, long>();


		
		///<summary>状态是否改变</summary>
		public override bool IsChanged
		{
			get
			{
				if(_stateChanged)
					return true;
				if(itemDic.IsChanged)
					return true;
				return base.IsChanged;
			}
		}
		
		///<summary>清除所有改变[含子项]</summary>
		public override void ClearChanges()
		{
			base.ClearChanges();
			//_stateChanged = false;
			itemDic.ClearChanges();
		}
		
		
		///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
		{
			_offset_ = base.Read(_buffer_, _offset_);
			
			//字段个数,最多支持255个
			var _fieldNum_ = XBuffer.ReadByte(_buffer_, ref _offset_);
			
			do {
				if(_fieldNum_ > 0){
					short _count_ = XBuffer.ReadShort(_buffer_, ref _offset_);
					for(int _a_ = 0; _a_ < _count_; ++_a_)
					{
						var _key_ = XBuffer.ReadInt(_buffer_, ref _offset_);
						var _val_ = XBuffer.ReadLong(_buffer_, ref _offset_);
						itemDic[_key_] = _val_;
					}

				}else break;
			}while(false);
			
			return _offset_;
		}
		
		///<summary>序列化，写入数据</summary>
        public override int Write(byte[] _buffer_, int _offset_)
        {	
			_offset_ = base.Write(_buffer_, _offset_);
			
			//写入字段数量,最多支持255个
			XBuffer.WriteByte(1, _buffer_, ref _offset_);
			
			//写入数据
			short _listCount_ = (short)itemDic.Count;
			XBuffer.WriteShort(_listCount_, _buffer_, ref _offset_);
			foreach(var kv in itemDic)
			{
				XBuffer.WriteInt(kv.Key, _buffer_, ref _offset_);
				XBuffer.WriteLong(kv.Value, _buffer_, ref _offset_);
			}
			
			
			return _offset_;
		}
	}
}