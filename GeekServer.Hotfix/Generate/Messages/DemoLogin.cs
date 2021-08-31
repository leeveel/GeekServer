//auto generated, do not modify it
//限制：命名不能以下划线结尾(可能冲突)
//限制：不能跨协议文件继承,不能跨文件使用继承关系
//限制：map的key只支持short, int, long, string；list/map不能optional,list/map不能嵌套
//兼容限制：字段只能添加，添加后不能删除，添加字段只能添加到最后,添加消息类型只能添加到最后
//兼容限制：不能修改字段类型（如从bool改为long）
//兼容限制：消息类型(含msdId)不能作为其他消息的成员类型
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Attributes;

///<summary>登陆</summary>
namespace Geek.Server.Message.DemoLogin
{
	internal class DemoLoginMsgFactory
	{
		///<summary>通过msgIdx构造msg</summary>
		public static BaseMessage Create(int msgIdx)
		{
			switch(msgIdx)
			{
				case 1: return new UserInfo();
				case 2: return new ReqLogin();
				case 3: return new ResLogin();
				case 4: return new ResLevelUp();
				case 5: return new ResNotice();
				case 6: return new ReqChangeName();
				case 7: return new ResChangeName();
				default: return default;
			}
		}
	}
	
	
	///<summary>玩家基础信息</summary>
    public class UserInfo : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 1;//最多支持255个消息类型
		
		///<summary>角色名</summary>
		public string roleName { get{ return _roleName_; } set{ _roleName_ = value; _stateChanged = true; } }
		string _roleName_;

		///<summary>角色id</summary>
		public long roleId { get{ return _roleId_; } set{ _roleId_ = value; _stateChanged = true; } }
		long _roleId_;

		///<summary>等级</summary>
		public int level { get{ return _level_; } set{ _level_ = value; _stateChanged = true; } }
		int _level_;

		///<summary>创建时间</summary>
		public long createTime { get{ return _createTime_; } set{ _createTime_ = value; _stateChanged = true; } }
		long _createTime_;

		///<summary>vip等级</summary>
		public int vipLevel { get{ return _vipLevel_; } set{ _vipLevel_ = value; _stateChanged = true; } }
		int _vipLevel_;


		
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
					roleName = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 1){
					roleId = XBuffer.ReadLong(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 2){
					level = XBuffer.ReadInt(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 3){
					createTime = XBuffer.ReadLong(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 4){
					vipLevel = XBuffer.ReadInt(_buffer_, ref _offset_);
					
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
			XBuffer.WriteString(roleName, _buffer_, ref _offset_);
			XBuffer.WriteLong(roleId, _buffer_, ref _offset_);
			XBuffer.WriteInt(level, _buffer_, ref _offset_);
			XBuffer.WriteLong(createTime, _buffer_, ref _offset_);
			XBuffer.WriteInt(vipLevel, _buffer_, ref _offset_);
			
			//覆盖当前对象长度
			XBuffer.WriteInt(_offset_ - _startOffset_, _buffer_, ref _startOffset_);
			return _offset_;
		}
	}

	///<summary>登陆</summary>
    public class ReqLogin : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 2;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 111001;
		
		///<summary>账号</summary>
		public string userName { get{ return _userName_; } set{ _userName_ = value; _stateChanged = true; } }
		string _userName_;

		///<summary>平台</summary>
		public string platform { get{ return _platform_; } set{ _platform_ = value; _stateChanged = true; } }
		string _platform_;

		///<summary>sdk类型</summary>
		public int sdkType { get{ return _sdkType_; } set{ _sdkType_ = value; _stateChanged = true; } }
		int _sdkType_;

		///<summary>sdk token</summary>
		public string sdkToken { get{ return _sdkToken_; } set{ _sdkToken_ = value; _stateChanged = true; } }
		string _sdkToken_;

		///<summary>设备id</summary>
		public string device { get{ return _device_; } set{ _device_ = value; _stateChanged = true; } }
		string _device_;


		
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
					userName = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 1){
					platform = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 2){
					sdkType = XBuffer.ReadInt(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 3){
					sdkToken = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 4){
					device = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
			}while(false);
			
			return _offset_;
		}
		
		///<summary>序列化，写入数据</summary>
        public override int Write(byte[] _buffer_, int _offset_)
        {	
			_offset_ = base.Write(_buffer_, _offset_);
			
			//写入字段数量,最多支持255个
			XBuffer.WriteByte(5, _buffer_, ref _offset_);
			
			//写入数据
			XBuffer.WriteString(userName, _buffer_, ref _offset_);
			XBuffer.WriteString(platform, _buffer_, ref _offset_);
			XBuffer.WriteInt(sdkType, _buffer_, ref _offset_);
			XBuffer.WriteString(sdkToken, _buffer_, ref _offset_);
			XBuffer.WriteString(device, _buffer_, ref _offset_);
			
			return _offset_;
		}
	}

	///<summary>登陆结果</summary>
    public class ResLogin : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 3;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 111002;
		
		///<summary>登陆结果，0成功，其他时候为错误码</summary>
		public int code { get{ return _code_; } set{ _code_ = value; _stateChanged = true; } }
		int _code_;

		///<summary>角色信息</summary>
		public UserInfo userInfo { get{ return _userInfo_; } set{ _userInfo_ = value; _stateChanged = true; } }
		UserInfo _userInfo_;


		
		///<summary>状态是否改变</summary>
		public override bool IsChanged
		{
			get
			{
				if(_stateChanged)
					return true;
				if(userInfo.IsChanged)
					return true;
				return base.IsChanged;
			}
		}
		
		///<summary>清除所有改变[含子项]</summary>
		public override void ClearChanges()
		{
			base.ClearChanges();
			//_stateChanged = false;
			userInfo.ClearChanges();
		}
		
		
		///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
		{
			_offset_ = base.Read(_buffer_, _offset_);
			
			//字段个数,最多支持255个
			var _fieldNum_ = XBuffer.ReadByte(_buffer_, ref _offset_);
			
			do {
				if(_fieldNum_ > 0){
					code = XBuffer.ReadInt(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 1){
					var _idx_ = XBuffer.ReadByte(_buffer_, ref _offset_);
					userInfo = new UserInfo();
					_offset_ = userInfo.Read(_buffer_, _offset_);
					
				}else break;
			}while(false);
			
			return _offset_;
		}
		
		///<summary>序列化，写入数据</summary>
        public override int Write(byte[] _buffer_, int _offset_)
        {	
			_offset_ = base.Write(_buffer_, _offset_);
			
			//写入字段数量,最多支持255个
			XBuffer.WriteByte(2, _buffer_, ref _offset_);
			
			//写入数据
			XBuffer.WriteInt(code, _buffer_, ref _offset_);
			XBuffer.WriteByte(userInfo._msgIdx_, _buffer_, ref _offset_);
			_offset_ = userInfo.Write(_buffer_, _offset_);
			
			return _offset_;
		}
	}

	///<summary>等级变化</summary>
    public class ResLevelUp : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 4;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 111003;
		
		///<summary>玩家等级</summary>
		public int level { get{ return _level_; } set{ _level_ = value; _stateChanged = true; } }
		int _level_;


		
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
					level = XBuffer.ReadInt(_buffer_, ref _offset_);
					
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
			XBuffer.WriteInt(level, _buffer_, ref _offset_);
			
			return _offset_;
		}
	}

	///<summary>提示信息</summary>
    public class ResNotice : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 5;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 111004;
		
		///<summary>提示内容</summary>
		public string tip { get{ return _tip_; } set{ _tip_ = value; _stateChanged = true; } }
		string _tip_;


		
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
					tip = XBuffer.ReadString(_buffer_, ref _offset_);
					
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
			XBuffer.WriteString(tip, _buffer_, ref _offset_);
			
			return _offset_;
		}
	}

	///<summary>改名字</summary>
    public class ReqChangeName : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 6;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 111005;
		
		///<summary>新名字</summary>
		public string newName { get{ return _newName_; } set{ _newName_ = value; _stateChanged = true; } }
		string _newName_;


		
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
					newName = XBuffer.ReadString(_buffer_, ref _offset_);
					
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
			XBuffer.WriteString(newName, _buffer_, ref _offset_);
			
			return _offset_;
		}
	}

	///<summary>改名字结果</summary>
    public class ResChangeName : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 7;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 111006;
		
		///<summary>为空表示改名成功，不为空则为提示内容</summary>
		public string msg { get{ return _msg_; } set{ _msg_ = value; _stateChanged = true; } }
		string _msg_;

		///<summary>新名字</summary>
		public string newName { get{ return _newName_; } set{ _newName_ = value; _stateChanged = true; } }
		string _newName_;


		
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
					msg = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 1){
					newName = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
			}while(false);
			
			return _offset_;
		}
		
		///<summary>序列化，写入数据</summary>
        public override int Write(byte[] _buffer_, int _offset_)
        {	
			_offset_ = base.Write(_buffer_, _offset_);
			
			//写入字段数量,最多支持255个
			XBuffer.WriteByte(2, _buffer_, ref _offset_);
			
			//写入数据
			XBuffer.WriteString(msg, _buffer_, ref _offset_);
			XBuffer.WriteString(newName, _buffer_, ref _offset_);
			
			return _offset_;
		}
	}
}