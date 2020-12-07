/****************************************************************************
Copyright (c) Geek

https://github.com/leeveel/GeekServer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
//for xbuffer
//Auto generated, do not modify it
//限制：命名不能以下划线开头，不能跨命名空间继承
//兼容限制1、字段只能添加，添加后不能删除，字段只能添加到最后
//兼容限制2、不能修改字段类型（如从bool改为long）
//登陆消息

using System;
using System.Collections.Generic;
using Geek.Core.Net.Message;

namespace Message.Login
{
	public class LoginMsgCreator
	{
		public static BaseMessage Create(int msgId)
		{
			switch(msgId)
			{
				case 101201:
					return new ReqLogin();
				case 101101:
					return new ResLogin();
				case 101102:
					return new ResPrompt();
				case 101103:
					return new ResHeartBeat();
				case 101205:
					return new ReqReLogin();
				case 101105:
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
		///<summary>vip等级</summary>
		public int vipLevel;
		///<summary>战斗力</summary>
		public long fightPower;
		///<summary>公会id</summary>
		public long guildId;
		///<summary>公会名字</summary>
		public string guildName;
		///<summary>开服天数</summary>
		public int openServerDay;
		///<summary>世界等级</summary>
		public int serverLevel;
		///<summary>登陆时服务器时间</summary>
		public long loginTick;
		///<summary>注册时间</summary>
		public long startTime;
		///<summary>是不是gm玩家</summary>
		public bool isGMRole;

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
			vipLevel = 0;
			fightPower = 0L;
			guildId = 0L;
			guildName = null;
			openServerDay = 0;
			serverLevel = 0;
			loginTick = 0L;
			startTime = 0L;
			isGMRole = false;
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
								vipLevel = XBuffer.ReadInt(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 4)
					{
								fightPower = XBuffer.ReadLong(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 5)
					{
								guildId = XBuffer.ReadLong(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 6)
					{
								guildName = XBuffer.ReadString(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 7)
					{
								openServerDay = XBuffer.ReadInt(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 8)
					{
								serverLevel = XBuffer.ReadInt(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 9)
					{
								loginTick = XBuffer.ReadLong(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 10)
					{
								startTime = XBuffer.ReadLong(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 11)
					{
								isGMRole = XBuffer.ReadBool(_buffer_, ref _offset_);
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
				
				XBuffer.WriteByte(255, _buffer_, ref _offset_);
				XBuffer.WriteByte(31, _buffer_, ref _offset_);
				
						XBuffer.WriteLong(roleId, _buffer_, ref _offset_);
						XBuffer.WriteString(roleName, _buffer_, ref _offset_);
						XBuffer.WriteInt(level, _buffer_, ref _offset_);
						XBuffer.WriteInt(vipLevel, _buffer_, ref _offset_);
						XBuffer.WriteLong(fightPower, _buffer_, ref _offset_);
						XBuffer.WriteLong(guildId, _buffer_, ref _offset_);
						XBuffer.WriteString(guildName, _buffer_, ref _offset_);
						XBuffer.WriteInt(openServerDay, _buffer_, ref _offset_);
						XBuffer.WriteInt(serverLevel, _buffer_, ref _offset_);
						XBuffer.WriteLong(loginTick, _buffer_, ref _offset_);
						XBuffer.WriteLong(startTime, _buffer_, ref _offset_);
						XBuffer.WriteBool(isGMRole, _buffer_, ref _offset_);
				
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
        public const int MsgId = 101201;
		
		///<summary>登陆用户名</summary>
		public string userName;
		///<summary>游戏服务器Id</summary>
		public int serverId;
		///<summary>登陆标识</summary>
		public string sdkToken;
		///<summary>0无SDK</summary>
		public int sdkType;
		///<summary>渠道id</summary>
		public string channelId;
		///<summary>是否为后台重连</summary>
		public bool isRelogin;
		///<summary>登陆token,客户端启动游戏生成一次[相同代表是同一重连/不同则顶号]</summary>
		public long handToken;
		///<summary>0编辑器，1android, 2ios, 3ios越狱</summary>
		public int deviceType;
		///<summary>手机系统 android ios</summary>
		public string deviceOS;
		///<summary>设备型号</summary>
		public string deviceModel;
		///<summary>设备名字</summary>
		public string deviceName;

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
			sdkToken = null;
			sdkType = 0;
			channelId = null;
			isRelogin = false;
			handToken = 0L;
			deviceType = 0;
			deviceOS = null;
			deviceModel = null;
			deviceName = null;
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
								sdkToken = XBuffer.ReadString(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 3)
					{
								sdkType = XBuffer.ReadInt(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 4)
					{
								channelId = XBuffer.ReadString(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 5)
					{
								isRelogin = XBuffer.ReadBool(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 6)
					{
								handToken = XBuffer.ReadLong(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 7)
					{
								deviceType = XBuffer.ReadInt(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 8)
					{
								deviceOS = XBuffer.ReadString(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 9)
					{
								deviceModel = XBuffer.ReadString(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 10)
					{
								deviceName = XBuffer.ReadString(_buffer_, ref _offset_);
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
						XBuffer.WriteString(sdkToken, _buffer_, ref _offset_);
						XBuffer.WriteInt(sdkType, _buffer_, ref _offset_);
						XBuffer.WriteString(channelId, _buffer_, ref _offset_);
						XBuffer.WriteBool(isRelogin, _buffer_, ref _offset_);
						XBuffer.WriteLong(handToken, _buffer_, ref _offset_);
						XBuffer.WriteInt(deviceType, _buffer_, ref _offset_);
						XBuffer.WriteString(deviceOS, _buffer_, ref _offset_);
						XBuffer.WriteString(deviceModel, _buffer_, ref _offset_);
						XBuffer.WriteString(deviceName, _buffer_, ref _offset_);
				
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
        public const int MsgId = 101101;
		
		///<summary>登陆结果（1 = 成功，other = 失败）</summary>
		public int result;
		///<summary>登陆失败的原因</summary>
		public int reason;
		///<summary>角色信息</summary>
		public RoleInfo role;
		///<summary>登陆用户名</summary>
		public string userName;
		///<summary>是否为新角色</summary>
		public bool isNewCreate;

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
			role = null;
			userName = null;
			isNewCreate = false;
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
								var _real_type_ = XBuffer.ReadByte(_buffer_, ref _offset_);
								RoleInfo _value_ = new RoleInfo();
								_offset_ = _value_.Read(_buffer_, _offset_);
								role = _value_;
					} else { break; }
					
					if(_fieldNum_ > 3)
					{
								userName = XBuffer.ReadString(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 4)
					{
								isNewCreate = XBuffer.ReadBool(_buffer_, ref _offset_);
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
							if(role == null)
								LOGGER.Error("role is null");
							else
								_offset_ = role.WriteWithType(_buffer_, _offset_);
						XBuffer.WriteString(userName, _buffer_, ref _offset_);
						XBuffer.WriteBool(isNewCreate, _buffer_, ref _offset_);
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }
    }
	///<summary>服务器通知</summary>
    public class ResPrompt : BaseMessage
    {
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 101102;
		
		///<summary>通知内容</summary>
		public string msg;
		///<summary>通知内容语言包id</summary>
		public int msgLanId;
		///<summary>1tip, 2弹窗提示 3弹窗回到登陆，4弹窗退出游戏</summary>
		public short type;

        //构造函数
        public ResPrompt() : base()
        {
        }
		
		//不缓存可以不调用
		public override void Reset()
		{
			base.Reset();
			msg = null;
			msgLanId = 0;
			type = 0;
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
								msg = XBuffer.ReadString(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 1)
					{
								msgLanId = XBuffer.ReadInt(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 2)
					{
								type = XBuffer.ReadShort(_buffer_, ref _offset_);
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
				
				
				XBuffer.WriteByte(7, _buffer_, ref _offset_);
				
						XBuffer.WriteString(msg, _buffer_, ref _offset_);
						XBuffer.WriteInt(msgLanId, _buffer_, ref _offset_);
						XBuffer.WriteShort(type, _buffer_, ref _offset_);
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }
    }
	///<summary>服务器通知</summary>
    public class ResHeartBeat : BaseMessage
    {
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 101103;
		
		///<summary>服务器时间</summary>
		public long serverTime;

        //构造函数
        public ResHeartBeat() : base()
        {
        }
		
		//不缓存可以不调用
		public override void Reset()
		{
			base.Reset();
			serverTime = 0L;
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
								serverTime = XBuffer.ReadLong(_buffer_, ref _offset_);
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
				
						XBuffer.WriteLong(serverTime, _buffer_, ref _offset_);
				
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
        public const int MsgId = 101205;
		
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
        public const int MsgId = 101105;
		
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