//auto generated, do not modify it
//限制：命名不能以下划线结尾(可能冲突)
//限制：不能跨协议文件继承,不能跨文件使用继承关系
//限制：map的key只支持short, int, long, string；list/map不能optional,list/map不能嵌套
//兼容限制：字段只能添加，添加后不能删除，添加字段只能添加到最后,添加消息类型只能添加到最后
//兼容限制：不能修改字段类型（如从bool改为long）
//兼容限制：消息类型(含msdId)不能作为其他消息的成员类型
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Attributes;

///<summary>玩家快照</summary>
namespace Geek.Server.Message.Login
{
	internal class LoginMsgFactory
	{
		///<summary>通过msgIdx构造msg</summary>
		public static BaseMessage Create(int msgIdx)
		{
			switch(msgIdx)
			{
				case 1: return new RoleInfo();
				case 2: return new ReqLogin();
				case 3: return new ReqReLogin();
				case 4: return new ResLogin();
				case 5: return new ResReLogin();
				case 6: return new HearBeat();
				case 7: return new ResPrompt();
				case 8: return new ResUnlockScreen();
				default: return default;
			}
		}
	}
	
	
	///<summary>玩家信息</summary>
    public class RoleInfo : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 1;//最多支持255个消息类型
		
		///<summary>id</summary>
		public long roleId { get{ return _roleId_; } set{ _roleId_ = value; _stateChanged = true; } }
		long _roleId_;

		///<summary>名字</summary>
		public string roleName { get{ return _roleName_; } set{ _roleName_ = value; _stateChanged = true; } }
		string _roleName_;

		///<summary>角色等级</summary>
		public int level { get{ return _level_; } set{ _level_ = value; _stateChanged = true; } }
		int _level_;

		///<summary>vip等级</summary>
		public int vipLevel { get{ return _vipLevel_; } set{ _vipLevel_ = value; _stateChanged = true; } }
		int _vipLevel_;

		///<summary>战斗力</summary>
		public long fightPower { get{ return _fightPower_; } set{ _fightPower_ = value; _stateChanged = true; } }
		long _fightPower_;

		///<summary>公会id</summary>
		public long guildId { get{ return _guildId_; } set{ _guildId_ = value; _stateChanged = true; } }
		long _guildId_;

		///<summary>公会名</summary>
		public string guildName { get{ return _guildName_; } set{ _guildName_ = value; _stateChanged = true; } }
		string _guildName_;

		///<summary>开服天数</summary>
		public int openServerDays { get{ return _openServerDays_; } set{ _openServerDays_ = value; _stateChanged = true; } }
		int _openServerDays_;

		///<summary>世界等级</summary>
		public int serverLevel { get{ return _serverLevel_; } set{ _serverLevel_ = value; _stateChanged = true; } }
		int _serverLevel_;

		///<summary>登陆时间</summary>
		public long loginTick { get{ return _loginTick_; } set{ _loginTick_ = value; _stateChanged = true; } }
		long _loginTick_;

		///<summary>创角时间</summary>
		public long createTick { get{ return _createTick_; } set{ _createTick_ = value; _stateChanged = true; } }
		long _createTick_;

		///<summary>知否是gm玩家</summary>
		public bool isGMRole { get{ return _isGMRole_; } set{ _isGMRole_ = value; _stateChanged = true; } }
		bool _isGMRole_;


		
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
					roleId = XBuffer.ReadLong(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 1){
					roleName = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 2){
					level = XBuffer.ReadInt(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 3){
					vipLevel = XBuffer.ReadInt(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 4){
					fightPower = XBuffer.ReadLong(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 5){
					guildId = XBuffer.ReadLong(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 6){
					guildName = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 7){
					openServerDays = XBuffer.ReadInt(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 8){
					serverLevel = XBuffer.ReadInt(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 9){
					loginTick = XBuffer.ReadLong(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 10){
					createTick = XBuffer.ReadLong(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 11){
					isGMRole = XBuffer.ReadBool(_buffer_, ref _offset_);
					
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
			XBuffer.WriteByte(12, _buffer_, ref _offset_);
			
			//写入数据
			XBuffer.WriteLong(roleId, _buffer_, ref _offset_);
			XBuffer.WriteString(roleName, _buffer_, ref _offset_);
			XBuffer.WriteInt(level, _buffer_, ref _offset_);
			XBuffer.WriteInt(vipLevel, _buffer_, ref _offset_);
			XBuffer.WriteLong(fightPower, _buffer_, ref _offset_);
			XBuffer.WriteLong(guildId, _buffer_, ref _offset_);
			XBuffer.WriteString(guildName, _buffer_, ref _offset_);
			XBuffer.WriteInt(openServerDays, _buffer_, ref _offset_);
			XBuffer.WriteInt(serverLevel, _buffer_, ref _offset_);
			XBuffer.WriteLong(loginTick, _buffer_, ref _offset_);
			XBuffer.WriteLong(createTick, _buffer_, ref _offset_);
			XBuffer.WriteBool(isGMRole, _buffer_, ref _offset_);
			
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
        public const int MsgId = 101201;
		
		///<summary>登陆用户名</summary>
		public string userName { get{ return _userName_; } set{ _userName_ = value; _stateChanged = true; } }
		string _userName_;

		///<summary>游戏服务器Id</summary>
		public int serverId { get{ return _serverId_; } set{ _serverId_ = value; _stateChanged = true; } }
		int _serverId_;

		///<summary>sdk登陆标识</summary>
		public string sdkToken { get{ return _sdkToken_; } set{ _sdkToken_ = value; _stateChanged = true; } }
		string _sdkToken_;

		///<summary>sdk类型 0无sdk</summary>
		public int sdkType { get{ return _sdkType_; } set{ _sdkType_ = value; _stateChanged = true; } }
		int _sdkType_;

		///<summary>渠道id</summary>
		public string channelId { get{ return _channelId_; } set{ _channelId_ = value; _stateChanged = true; } }
		string _channelId_;

		///<summary>是否为后台重连</summary>
		public bool isRelogin { get{ return _isRelogin_; } set{ _isRelogin_ = value; _stateChanged = true; } }
		bool _isRelogin_;

		///<summary>登陆token,客户端启动游戏生成一次[相同代表是同一重连/不同则顶号]</summary>
		public long handToken { get{ return _handToken_; } set{ _handToken_ = value; _stateChanged = true; } }
		long _handToken_;

		///<summary>0编辑器，1android, 2ios, 3ios越狱</summary>
		public int deviceType { get{ return _deviceType_; } set{ _deviceType_ = value; _stateChanged = true; } }
		int _deviceType_;

		///<summary>手机系统 android ios</summary>
		public string deviceOS { get{ return _deviceOS_; } set{ _deviceOS_ = value; _stateChanged = true; } }
		string _deviceOS_;

		///<summary>设备型号</summary>
		public string deviceModel { get{ return _deviceModel_; } set{ _deviceModel_ = value; _stateChanged = true; } }
		string _deviceModel_;

		///<summary>设备名字</summary>
		public string deviceName { get{ return _deviceName_; } set{ _deviceName_ = value; _stateChanged = true; } }
		string _deviceName_;

		///<summary>设备id</summary>
		public string deviceId { get{ return _deviceId_; } set{ _deviceId_ = value; _stateChanged = true; } }
		string _deviceId_;


		
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
					serverId = XBuffer.ReadInt(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 2){
					sdkToken = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 3){
					sdkType = XBuffer.ReadInt(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 4){
					channelId = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 5){
					isRelogin = XBuffer.ReadBool(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 6){
					handToken = XBuffer.ReadLong(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 7){
					deviceType = XBuffer.ReadInt(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 8){
					deviceOS = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 9){
					deviceModel = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 10){
					deviceName = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 11){
					deviceId = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
			}while(false);
			
			return _offset_;
		}
		
		///<summary>序列化，写入数据</summary>
        public override int Write(byte[] _buffer_, int _offset_)
        {	
			_offset_ = base.Write(_buffer_, _offset_);
			
			//写入字段数量,最多支持255个
			XBuffer.WriteByte(12, _buffer_, ref _offset_);
			
			//写入数据
			XBuffer.WriteString(userName, _buffer_, ref _offset_);
			XBuffer.WriteInt(serverId, _buffer_, ref _offset_);
			XBuffer.WriteString(sdkToken, _buffer_, ref _offset_);
			XBuffer.WriteInt(sdkType, _buffer_, ref _offset_);
			XBuffer.WriteString(channelId, _buffer_, ref _offset_);
			XBuffer.WriteBool(isRelogin, _buffer_, ref _offset_);
			XBuffer.WriteLong(handToken, _buffer_, ref _offset_);
			XBuffer.WriteInt(deviceType, _buffer_, ref _offset_);
			XBuffer.WriteString(deviceOS, _buffer_, ref _offset_);
			XBuffer.WriteString(deviceModel, _buffer_, ref _offset_);
			XBuffer.WriteString(deviceName, _buffer_, ref _offset_);
			XBuffer.WriteString(deviceId, _buffer_, ref _offset_);
			
			return _offset_;
		}
	}

	///<summary>请求重连</summary>
    public class ReqReLogin : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 3;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 101202;
		
		public string sdkToken { get{ return _sdkToken_; } set{ _sdkToken_ = value; _stateChanged = true; } }
		string _sdkToken_;

		public long handToken { get{ return _handToken_; } set{ _handToken_ = value; _stateChanged = true; } }
		long _handToken_;


		
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
					sdkToken = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 1){
					handToken = XBuffer.ReadLong(_buffer_, ref _offset_);
					
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
			XBuffer.WriteString(sdkToken, _buffer_, ref _offset_);
			XBuffer.WriteLong(handToken, _buffer_, ref _offset_);
			
			return _offset_;
		}
	}

	///<summary>登陆结果</summary>
    public class ResLogin : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 4;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 101101;
		
		///<summary>登陆结果1成功，其他失败</summary>
		public int result { get{ return _result_; } set{ _result_ = value; _stateChanged = true; } }
		int _result_;

		///<summary>登陆失败的原因</summary>
		public int reason { get{ return _reason_; } set{ _reason_ = value; _stateChanged = true; } }
		int _reason_;

		///<summary>角色信息</summary>
		public RoleInfo role{ get{ return _role_; } set{ _role_ = value; _stateChanged = true; hasRole = value != default; }}
		RoleInfo _role_;
		public bool hasRole { get; private set; }

		///<summary>登陆用户名</summary>
		public string userName { get{ return _userName_; } set{ _userName_ = value; _stateChanged = true; } }
		string _userName_;

		///<summary>是否为新角色</summary>
		public bool isNewCreate { get{ return _isNewCreate_; } set{ _isNewCreate_ = value; _stateChanged = true; } }
		bool _isNewCreate_;


		
		///<summary>状态是否改变</summary>
		public override bool IsChanged
		{
			get
			{
				if(_stateChanged)
					return true;
				if(role != null && role.IsChanged)
					return true;
				return base.IsChanged;
			}
		}
		
		///<summary>清除所有改变[含子项]</summary>
		public override void ClearChanges()
		{
			base.ClearChanges();
			//_stateChanged = false;
			if(role != null) role.ClearChanges();
		}
		
		
		///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
		{
			_offset_ = base.Read(_buffer_, _offset_);
			
			//字段个数,最多支持255个
			var _fieldNum_ = XBuffer.ReadByte(_buffer_, ref _offset_);
			
			do {
				if(_fieldNum_ > 0){
					result = XBuffer.ReadInt(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 1){
					reason = XBuffer.ReadInt(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 2){
					hasRole = XBuffer.ReadBool(_buffer_, ref _offset_);
					if(hasRole){
					var _idx_ = XBuffer.ReadByte(_buffer_, ref _offset_);
					role = new RoleInfo();
					_offset_ = role.Read(_buffer_, _offset_);
					}
				}else break;
				if(_fieldNum_ > 3){
					userName = XBuffer.ReadString(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 4){
					isNewCreate = XBuffer.ReadBool(_buffer_, ref _offset_);
					
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
			XBuffer.WriteInt(result, _buffer_, ref _offset_);
			XBuffer.WriteInt(reason, _buffer_, ref _offset_);
			XBuffer.WriteBool(hasRole, _buffer_, ref _offset_);
			if(hasRole)
			{
				XBuffer.WriteByte(role._msgIdx_, _buffer_, ref _offset_);
				_offset_ = role.Write(_buffer_, _offset_);
			}
				
			XBuffer.WriteString(userName, _buffer_, ref _offset_);
			XBuffer.WriteBool(isNewCreate, _buffer_, ref _offset_);
			
			return _offset_;
		}
	}

	///<summary>断线重连</summary>
    public class ResReLogin : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 5;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 101102;
		
		///<summary>重连结果</summary>
		public bool success { get{ return _success_; } set{ _success_ = value; _stateChanged = true; } }
		bool _success_;


		
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
					success = XBuffer.ReadBool(_buffer_, ref _offset_);
					
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
			XBuffer.WriteBool(success, _buffer_, ref _offset_);
			
			return _offset_;
		}
	}

	///<summary>双向心跳/收到恢复同样的消息</summary>
    public class HearBeat : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 6;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 101303;
		
		///<summary>当前时间</summary>
		public long timeTick { get{ return _timeTick_; } set{ _timeTick_ = value; _stateChanged = true; } }
		long _timeTick_;


		
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
					timeTick = XBuffer.ReadLong(_buffer_, ref _offset_);
					
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
			XBuffer.WriteLong(timeTick, _buffer_, ref _offset_);
			
			return _offset_;
		}
	}

	///<summary>服务器通知</summary>
    public class ResPrompt : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 7;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 101103;
		
		///<summary>通知内容</summary>
		public string msg { get{ return _msg_; } set{ _msg_ = value; _stateChanged = true; } }
		string _msg_;

		///<summary>通知内容语言包id</summary>
		public int msgLanId { get{ return _msgLanId_; } set{ _msgLanId_ = value; _stateChanged = true; } }
		int _msgLanId_;

		///<summary>1tip, 2弹窗提示 3弹窗回到登陆，4弹窗退出游戏</summary>
		public short type { get{ return _type_; } set{ _type_ = value; _stateChanged = true; } }
		short _type_;


		
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
					msgLanId = XBuffer.ReadInt(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 2){
					type = XBuffer.ReadShort(_buffer_, ref _offset_);
					
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
			XBuffer.WriteString(msg, _buffer_, ref _offset_);
			XBuffer.WriteInt(msgLanId, _buffer_, ref _offset_);
			XBuffer.WriteShort(type, _buffer_, ref _offset_);
			
			return _offset_;
		}
	}

	///<summary>解屏消息</summary>
    public class ResUnlockScreen : BaseMessage
	{
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		internal virtual byte _msgIdx_ => 8;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 101104;
		

		
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
}