//for xbuffer
//Auto generated, do not modify it
//限制：命名不能以下划线开头，不能跨命名空间继承
//兼容限制1、字段只能添加，添加后不能删除，字段只能添加到最后
//兼容限制2、不能修改字段类型（如从bool改为long）
//登陆消息

using System;
using System.Collections.Generic;
using Geek.Core.Net.Message;

namespace Geek.Message.Login
{
	public class LoginMsgCreator
	{
		public static BaseMessage Create(int msgId)
		{
			switch(msgId)
			{
				case 103201:
					return new ReqLogin();
				case 103101:
					return new ResLogin();
				case 103205:
					return new ReqReLogin();
				case 103105:
					return new ResReLogin();
				default:
					return null;
			}
		}
	}
	
    public enum _TypeEnum_
    {
        RoleInfo = 1,
    }
						
	///<summary>实体信息</summary>
    public class RoleInfo : BaseMessage
    {
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
		
		///<summary>实体Id</summary>
		public long roleId;
		///<summary>角色名字</summary>
		public string roleName;
		///<summary>角色等级</summary>
		public int level;
		///<summary>设置性别0隐藏 1男 2女 </summary>
		public int gender;

        //构造函数
        public RoleInfo() : base()
        {
        }
		
		//不缓存可以不调用
		public override void Reset()
		{
			base.Reset();
			roleId = 0L;
			roleName = null;
			level = 0;
			gender = 0;
		}
		
        ///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
        {
            try
            {
                _offset_ = base.Read(_buffer_, _offset_);
			
				int _toReadLenOffset_ = _offset_;
				int _toReadLength_ = XBuffer.ReadInt(_buffer_, ref _offset_);
				
				int _fieldNum_ = 0;
				while(true)
				{
					var _fieldMark_ = XBuffer.ReadByte(_buffer_, ref _offset_);
					for(int i = 0; i < 7; ++i)
					{
						var _h_ = 1 << i;
						bool _mark_ = (_fieldMark_ & _h_) == _h_;
						if(_mark_) _fieldNum_++;
						else break;
					}
					var _e_ = 1 << 7;
					if((_fieldMark_ & _e_) == 0)
						break;
				}
				
				while(true)
				{
					if(_fieldNum_ > 0)
					{
								roleId = XBuffer.ReadLong(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 1)
					{
								roleName = XBuffer.ReadString(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 2)
					{
								level = XBuffer.ReadInt(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 3)
					{
								gender = XBuffer.ReadInt(_buffer_, ref _offset_);
					} else { break; }
					
					break;
				}
				
				//剔除未知数据
				while(_offset_ - _toReadLenOffset_ < _toReadLength_)
					XBuffer.ReadByte(_buffer_, ref _offset_);
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }

        public override int WriteWithType(byte[] _buffer_, int _offset_)
        {
            XBuffer.WriteByte((byte)_TypeEnum_.RoleInfo, _buffer_, ref _offset_);
            _offset_ = Write(_buffer_, _offset_);
			return _offset_;
        }

        ///<summary>序列化，写入数据</summary>
        public override int Write(byte[] _buffer_, int _offset_)
        {
            try
            {
                _offset_ = base.Write(_buffer_, _offset_);
				
				int _toWriteLenOffset_ = _offset_;
				XBuffer.WriteInt(0, _buffer_, ref _offset_);
				
				XBuffer.WriteByte(15, _buffer_, ref _offset_);
				
						XBuffer.WriteLong(roleId, _buffer_, ref _offset_);
						XBuffer.WriteString(roleName, _buffer_, ref _offset_);
						XBuffer.WriteInt(level, _buffer_, ref _offset_);
						XBuffer.WriteInt(gender, _buffer_, ref _offset_);
				
				XBuffer.WriteInt(_offset_ - _toWriteLenOffset_, _buffer_, ref _toWriteLenOffset_);
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }
    }
	///<summary>登陆</summary>
    public class ReqLogin : BaseMessage
    {
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 103201;
		
		///<summary>登陆用户名</summary>
		public string userName;
		///<summary>游戏服务器Id</summary>
		public int serverId;
		///<summary>登陆时间</summary>
		public long loginTime;
		///<summary>渠道id</summary>
		public string channelId;
		///<summary>0无SDK</summary>
		public int sdkType;
		protected byte _osVersion = 0;
		protected string __osVersion;
		public bool hasOsVersion() { return this._osVersion == 1; }
		///<summary>手机系统版本</summary>
		public string osVersion { set { _osVersion = 1; __osVersion = value; } get { return __osVersion; } }
		protected byte _mobileRand = 0;
		protected string __mobileRand;
		public bool hasMobileRand() { return this._mobileRand == 1; }
		///<summary>手机品牌(在IOS中为：ipad iPhone ipod)</summary>
		public string mobileRand { set { _mobileRand = 1; __mobileRand = value; } get { return __mobileRand; } }
		protected byte _mobileModel = 0;
		protected string __mobileModel;
		public bool hasMobileModel() { return this._mobileModel == 1; }
		///<summary>手机型号</summary>
		public string mobileModel { set { _mobileModel = 1; __mobileModel = value; } get { return __mobileModel; } }
		protected byte _mobileIOS = 0;
		protected string __mobileIOS;
		public bool hasMobileIOS() { return this._mobileIOS == 1; }
		///<summary>手机系统 android ios</summary>
		public string mobileIOS { set { _mobileIOS = 1; __mobileIOS = value; } get { return __mobileIOS; } }
		///<summary>是否为后台重连</summary>
		public bool isRelogin;
		///<summary>登陆token,启动游戏生成一次</summary>
		public long handToken;

        //构造函数
        public ReqLogin() : base()
        {
        }
		
		//不缓存可以不调用
		public override void Reset()
		{
			base.Reset();
			userName = null;
			serverId = 0;
			loginTime = 0L;
			channelId = null;
			sdkType = 0;
			_osVersion = 0;
			__osVersion = null;
			_mobileRand = 0;
			__mobileRand = null;
			_mobileModel = 0;
			__mobileModel = null;
			_mobileIOS = 0;
			__mobileIOS = null;
			isRelogin = false;
			handToken = 0L;
		}
		
        ///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
        {
            try
            {
				UniId = XBuffer.ReadInt(_buffer_, ref _offset_);
				//ErrCode = XBuffer.ReadInt(_buffer_, ref _offset_);
				//ErrDesc = XBuffer.ReadString(_buffer_, ref _offset_);
                _offset_ = base.Read(_buffer_, _offset_);
			
				
				int _fieldNum_ = 0;
				while(true)
				{
					var _fieldMark_ = XBuffer.ReadByte(_buffer_, ref _offset_);
					for(int i = 0; i < 7; ++i)
					{
						var _h_ = 1 << i;
						bool _mark_ = (_fieldMark_ & _h_) == _h_;
						if(_mark_) _fieldNum_++;
						else break;
					}
					var _e_ = 1 << 7;
					if((_fieldMark_ & _e_) == 0)
						break;
				}
				
				while(true)
				{
					if(_fieldNum_ > 0)
					{
								userName = XBuffer.ReadString(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 1)
					{
								serverId = XBuffer.ReadInt(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 2)
					{
								loginTime = XBuffer.ReadLong(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 3)
					{
								channelId = XBuffer.ReadString(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 4)
					{
								sdkType = XBuffer.ReadInt(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 5)
					{
							_osVersion = XBuffer.ReadByte(_buffer_, ref _offset_);
							if(_osVersion == 1)
							{
								osVersion = XBuffer.ReadString(_buffer_, ref _offset_);
							}
					} else { break; }
					
					if(_fieldNum_ > 6)
					{
							_mobileRand = XBuffer.ReadByte(_buffer_, ref _offset_);
							if(_mobileRand == 1)
							{
								mobileRand = XBuffer.ReadString(_buffer_, ref _offset_);
							}
					} else { break; }
					
					if(_fieldNum_ > 7)
					{
							_mobileModel = XBuffer.ReadByte(_buffer_, ref _offset_);
							if(_mobileModel == 1)
							{
								mobileModel = XBuffer.ReadString(_buffer_, ref _offset_);
							}
					} else { break; }
					
					if(_fieldNum_ > 8)
					{
							_mobileIOS = XBuffer.ReadByte(_buffer_, ref _offset_);
							if(_mobileIOS == 1)
							{
								mobileIOS = XBuffer.ReadString(_buffer_, ref _offset_);
							}
					} else { break; }
					
					if(_fieldNum_ > 9)
					{
								isRelogin = XBuffer.ReadBool(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 10)
					{
								handToken = XBuffer.ReadLong(_buffer_, ref _offset_);
					} else { break; }
					
					break;
				}
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }


        ///<summary>序列化，写入数据</summary>
        public override int Write(byte[] _buffer_, int _offset_)
        {
            try
            {
				XBuffer.WriteInt(UniId, _buffer_, ref _offset_);
				//XBuffer.WriteInt(ErrCode, _buffer_, ref _offset_);
				//XBuffer.WriteString(ErrDesc, _buffer_, ref _offset_);
                _offset_ = base.Write(_buffer_, _offset_);
				
				
				XBuffer.WriteByte(255, _buffer_, ref _offset_);
				XBuffer.WriteByte(15, _buffer_, ref _offset_);
				
						XBuffer.WriteString(userName, _buffer_, ref _offset_);
						XBuffer.WriteInt(serverId, _buffer_, ref _offset_);
						XBuffer.WriteLong(loginTime, _buffer_, ref _offset_);
						XBuffer.WriteString(channelId, _buffer_, ref _offset_);
						XBuffer.WriteInt(sdkType, _buffer_, ref _offset_);
					XBuffer.WriteByte(_osVersion, _buffer_, ref _offset_);
					if(_osVersion == 1)
					{
						XBuffer.WriteString(osVersion, _buffer_, ref _offset_);
					}
					XBuffer.WriteByte(_mobileRand, _buffer_, ref _offset_);
					if(_mobileRand == 1)
					{
						XBuffer.WriteString(mobileRand, _buffer_, ref _offset_);
					}
					XBuffer.WriteByte(_mobileModel, _buffer_, ref _offset_);
					if(_mobileModel == 1)
					{
						XBuffer.WriteString(mobileModel, _buffer_, ref _offset_);
					}
					XBuffer.WriteByte(_mobileIOS, _buffer_, ref _offset_);
					if(_mobileIOS == 1)
					{
						XBuffer.WriteString(mobileIOS, _buffer_, ref _offset_);
					}
						XBuffer.WriteBool(isRelogin, _buffer_, ref _offset_);
						XBuffer.WriteLong(handToken, _buffer_, ref _offset_);
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }
    }
	///<summary>登陆</summary>
    public class ResLogin : BaseMessage
    {
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 103101;
		
		///<summary>登陆结果（1 = 成功，other = 失败）</summary>
		public int result;
		///<summary>登陆失败的原因</summary>
		public int reason;
		///<summary>登陆用户名</summary>
		public string username;
		///<summary>角色信息</summary>
		public RoleInfo role;
		///<summary>是否为新角色</summary>
		public bool isNewCreat;

        //构造函数
        public ResLogin() : base()
        {
        }
		
		//不缓存可以不调用
		public override void Reset()
		{
			base.Reset();
			result = 0;
			reason = 0;
			username = null;
			role = null;
			isNewCreat = false;
		}
		
        ///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
        {
            try
            {
				UniId = XBuffer.ReadInt(_buffer_, ref _offset_);
				//ErrCode = XBuffer.ReadInt(_buffer_, ref _offset_);
				//ErrDesc = XBuffer.ReadString(_buffer_, ref _offset_);
                _offset_ = base.Read(_buffer_, _offset_);
			
				
				int _fieldNum_ = 0;
				while(true)
				{
					var _fieldMark_ = XBuffer.ReadByte(_buffer_, ref _offset_);
					for(int i = 0; i < 7; ++i)
					{
						var _h_ = 1 << i;
						bool _mark_ = (_fieldMark_ & _h_) == _h_;
						if(_mark_) _fieldNum_++;
						else break;
					}
					var _e_ = 1 << 7;
					if((_fieldMark_ & _e_) == 0)
						break;
				}
				
				while(true)
				{
					if(_fieldNum_ > 0)
					{
								result = XBuffer.ReadInt(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 1)
					{
								reason = XBuffer.ReadInt(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 2)
					{
								username = XBuffer.ReadString(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 3)
					{
								var _real_type_ = XBuffer.ReadByte(_buffer_, ref _offset_);
								RoleInfo _value_ = new RoleInfo();
								_offset_ = _value_.Read(_buffer_, _offset_);
								role = _value_;
					} else { break; }
					
					if(_fieldNum_ > 4)
					{
								isNewCreat = XBuffer.ReadBool(_buffer_, ref _offset_);
					} else { break; }
					
					break;
				}
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }


        ///<summary>序列化，写入数据</summary>
        public override int Write(byte[] _buffer_, int _offset_)
        {
            try
            {
				XBuffer.WriteInt(UniId, _buffer_, ref _offset_);
				//XBuffer.WriteInt(ErrCode, _buffer_, ref _offset_);
				//XBuffer.WriteString(ErrDesc, _buffer_, ref _offset_);
                _offset_ = base.Write(_buffer_, _offset_);
				
				
				XBuffer.WriteByte(31, _buffer_, ref _offset_);
				
						XBuffer.WriteInt(result, _buffer_, ref _offset_);
						XBuffer.WriteInt(reason, _buffer_, ref _offset_);
						XBuffer.WriteString(username, _buffer_, ref _offset_);
							if(role == null)
								LOGGER.Error("role is null");
							else
								_offset_ = role.WriteWithType(_buffer_, _offset_);
						XBuffer.WriteBool(isNewCreat, _buffer_, ref _offset_);
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }
    }
	///<summary>请求重连</summary>
    public class ReqReLogin : BaseMessage
    {
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 103205;
		
		///<summary>token</summary>
		public string token;

        //构造函数
        public ReqReLogin() : base()
        {
        }
		
		//不缓存可以不调用
		public override void Reset()
		{
			base.Reset();
			token = null;
		}
		
        ///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
        {
            try
            {
				UniId = XBuffer.ReadInt(_buffer_, ref _offset_);
				//ErrCode = XBuffer.ReadInt(_buffer_, ref _offset_);
				//ErrDesc = XBuffer.ReadString(_buffer_, ref _offset_);
                _offset_ = base.Read(_buffer_, _offset_);
			
				
				int _fieldNum_ = 0;
				while(true)
				{
					var _fieldMark_ = XBuffer.ReadByte(_buffer_, ref _offset_);
					for(int i = 0; i < 7; ++i)
					{
						var _h_ = 1 << i;
						bool _mark_ = (_fieldMark_ & _h_) == _h_;
						if(_mark_) _fieldNum_++;
						else break;
					}
					var _e_ = 1 << 7;
					if((_fieldMark_ & _e_) == 0)
						break;
				}
				
				while(true)
				{
					if(_fieldNum_ > 0)
					{
								token = XBuffer.ReadString(_buffer_, ref _offset_);
					} else { break; }
					
					break;
				}
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }


        ///<summary>序列化，写入数据</summary>
        public override int Write(byte[] _buffer_, int _offset_)
        {
            try
            {
				XBuffer.WriteInt(UniId, _buffer_, ref _offset_);
				//XBuffer.WriteInt(ErrCode, _buffer_, ref _offset_);
				//XBuffer.WriteString(ErrDesc, _buffer_, ref _offset_);
                _offset_ = base.Write(_buffer_, _offset_);
				
				
				XBuffer.WriteByte(1, _buffer_, ref _offset_);
				
						XBuffer.WriteString(token, _buffer_, ref _offset_);
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }
    }
	///<summary>重连连结果</summary>
    public class ResReLogin : BaseMessage
    {
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 103105;
		
		///<summary>登陆结果</summary>
		public bool success;

        //构造函数
        public ResReLogin() : base()
        {
        }
		
		//不缓存可以不调用
		public override void Reset()
		{
			base.Reset();
			success = false;
		}
		
        ///<summary>反序列化，读取数据</summary>
        public override int Read(byte[] _buffer_, int _offset_)
        {
            try
            {
				UniId = XBuffer.ReadInt(_buffer_, ref _offset_);
				//ErrCode = XBuffer.ReadInt(_buffer_, ref _offset_);
				//ErrDesc = XBuffer.ReadString(_buffer_, ref _offset_);
                _offset_ = base.Read(_buffer_, _offset_);
			
				
				int _fieldNum_ = 0;
				while(true)
				{
					var _fieldMark_ = XBuffer.ReadByte(_buffer_, ref _offset_);
					for(int i = 0; i < 7; ++i)
					{
						var _h_ = 1 << i;
						bool _mark_ = (_fieldMark_ & _h_) == _h_;
						if(_mark_) _fieldNum_++;
						else break;
					}
					var _e_ = 1 << 7;
					if((_fieldMark_ & _e_) == 0)
						break;
				}
				
				while(true)
				{
					if(_fieldNum_ > 0)
					{
								success = XBuffer.ReadBool(_buffer_, ref _offset_);
					} else { break; }
					
					break;
				}
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }


        ///<summary>序列化，写入数据</summary>
        public override int Write(byte[] _buffer_, int _offset_)
        {
            try
            {
				XBuffer.WriteInt(UniId, _buffer_, ref _offset_);
				//XBuffer.WriteInt(ErrCode, _buffer_, ref _offset_);
				//XBuffer.WriteString(ErrDesc, _buffer_, ref _offset_);
                _offset_ = base.Write(_buffer_, _offset_);
				
				
				XBuffer.WriteByte(1, _buffer_, ref _offset_);
				
						XBuffer.WriteBool(success, _buffer_, ref _offset_);
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }
    }
}