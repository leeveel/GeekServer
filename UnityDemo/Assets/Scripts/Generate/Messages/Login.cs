//auto generated, do not modify it
//限制：命名不能以下划线结尾(可能冲突)
//限制：不能跨协议文件继承,不能跨文件使用继承关系
//限制：map的key只支持short, int, long, string；list/map不能optional,list/map不能嵌套
//兼容限制：字段只能添加，添加后不能删除，添加字段只能添加到最后,添加消息类型只能添加到最后
//兼容限制：不能修改字段类型（如从bool改为long）
//兼容限制：消息类型(含msdId)不能作为其他消息的成员类型
using System.Collections.Generic;

///<summary>登陆</summary>
namespace Geek.Client.Message.Login
{
	internal class LoginMsgFactory
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
				case 8: return new HearBeat();
				case 9: return new ResErrorCode();
				default: return default;
			}
		}
	}
	
	
	///<summary>玩家基础信息</summary>
    public class UserInfo : BaseMessage
	{
		internal virtual byte _msgIdx_ => 1;//最多支持255个消息类型
		
		///<summary>角色名</summary>
		public string roleName { get; set;}

		///<summary>角色id</summary>
		public long roleId { get; set;}

		///<summary>等级</summary>
		public int level { get; set;}

		///<summary>创建时间</summary>
		public long createTime { get; set;}

		///<summary>vip等级</summary>
		public int vipLevel { get; set;}


		
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
		internal virtual byte _msgIdx_ => 2;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 111001;
		
		///<summary>账号</summary>
		public string userName { get; set;}

		///<summary>平台</summary>
		public string platform { get; set;}

		///<summary>sdk类型</summary>
		public int sdkType { get; set;}

		///<summary>sdk token</summary>
		public string sdkToken { get; set;}

		///<summary>设备id</summary>
		public string device { get; set;}


		
		///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
		{
			UniId = XBuffer.ReadInt(_buffer_, ref _offset_);
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
			XBuffer.WriteInt(UniId, _buffer_, ref _offset_);
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
		internal virtual byte _msgIdx_ => 3;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 111002;
		
		///<summary>登陆结果，0成功，其他时候为错误码</summary>
		public int code { get; set;}

		///<summary>角色信息</summary>
		public UserInfo userInfo { get; set;}


		
		///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
		{
			UniId = XBuffer.ReadInt(_buffer_, ref _offset_);
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
			XBuffer.WriteInt(UniId, _buffer_, ref _offset_);
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
		internal virtual byte _msgIdx_ => 4;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 111003;
		
		///<summary>玩家等级</summary>
		public int level { get; set;}


		
		///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
		{
			UniId = XBuffer.ReadInt(_buffer_, ref _offset_);
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
			XBuffer.WriteInt(UniId, _buffer_, ref _offset_);
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
		internal virtual byte _msgIdx_ => 5;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 111004;
		
		///<summary>提示内容</summary>
		public string tip { get; set;}


		
		///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
		{
			UniId = XBuffer.ReadInt(_buffer_, ref _offset_);
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
			XBuffer.WriteInt(UniId, _buffer_, ref _offset_);
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
		internal virtual byte _msgIdx_ => 6;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 111005;
		
		///<summary>新名字</summary>
		public string newName { get; set;}


		
		///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
		{
			UniId = XBuffer.ReadInt(_buffer_, ref _offset_);
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
			XBuffer.WriteInt(UniId, _buffer_, ref _offset_);
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
		internal virtual byte _msgIdx_ => 7;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 111006;
		
		///<summary>为空表示改名成功，不为空则为提示内容</summary>
		public string msg { get; set;}

		///<summary>新名字</summary>
		public string newName { get; set;}


		
		///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
		{
			UniId = XBuffer.ReadInt(_buffer_, ref _offset_);
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
			XBuffer.WriteInt(UniId, _buffer_, ref _offset_);
			_offset_ = base.Write(_buffer_, _offset_);
			
			//写入字段数量,最多支持255个
			XBuffer.WriteByte(2, _buffer_, ref _offset_);
			
			//写入数据
			XBuffer.WriteString(msg, _buffer_, ref _offset_);
			XBuffer.WriteString(newName, _buffer_, ref _offset_);
			
			return _offset_;
		}
	}

	///<summary>双向心跳/收到恢复同样的消息</summary>
    public class HearBeat : BaseMessage
	{
		internal virtual byte _msgIdx_ => 8;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 111007;
		
		///<summary>当前时间</summary>
		public long timeTick { get; set;}


		
		///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
		{
			UniId = XBuffer.ReadInt(_buffer_, ref _offset_);
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
			XBuffer.WriteInt(UniId, _buffer_, ref _offset_);
			_offset_ = base.Write(_buffer_, _offset_);
			
			//写入字段数量,最多支持255个
			XBuffer.WriteByte(1, _buffer_, ref _offset_);
			
			//写入数据
			XBuffer.WriteLong(timeTick, _buffer_, ref _offset_);
			
			return _offset_;
		}
	}

	///<summary>客户端每次请求都会回复错误码</summary>
    public class ResErrorCode : BaseMessage
	{
		internal virtual byte _msgIdx_ => 9;//最多支持255个消息类型
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 111008;
		
		///<summary>0:表示无错误</summary>
		public int errCode { get; set;}

		///<summary>错误描述（不为0时有效）</summary>
		public string desc { get; set;}


		
		///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
		{
			UniId = XBuffer.ReadInt(_buffer_, ref _offset_);
			_offset_ = base.Read(_buffer_, _offset_);
			
			//字段个数,最多支持255个
			var _fieldNum_ = XBuffer.ReadByte(_buffer_, ref _offset_);
			
			do {
				if(_fieldNum_ > 0){
					errCode = XBuffer.ReadInt(_buffer_, ref _offset_);
					
				}else break;
				if(_fieldNum_ > 1){
					desc = XBuffer.ReadString(_buffer_, ref _offset_);
					
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
			XBuffer.WriteByte(2, _buffer_, ref _offset_);
			
			//写入数据
			XBuffer.WriteInt(errCode, _buffer_, ref _offset_);
			XBuffer.WriteString(desc, _buffer_, ref _offset_);
			
			return _offset_;
		}
	}
}