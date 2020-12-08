//for xbuffer
//Auto generated, do not modify it
//限制：命名不能以下划线开头，不能跨命名空间继承
//兼容限制1、字段只能添加，添加后不能删除，字段只能添加到最后
//兼容限制2、不能修改字段类型（如从bool改为long）
//角色消息

using System;
using System.Collections.Generic;
using Geek.Core.Net.Message;

namespace Geek.Message.Role
{
	public class RoleMsgCreator
	{
		public static BaseMessage Create(int msgId)
		{
			switch(msgId)
			{
				case 105201:
					return new ReqClientInitOver();
				case 105202:
					return new ReqHeartBeat();
				case 105203:
					return new ReqCheckResCode();
				case 105103:
					return new ResPrompt();
				case 105102:
					return new ResCheckResCode();
				case 105104:
					return new ResHeartBeat();
				case 105107:
					return new ResLevelUp();
				case 105119:
					return new ResUnlockScreen();
				case 105120:
					return new ResAcrossDay();
				default:
					return null;
			}
		}
	}
	
						
	///<summary>客户端初始化完成</summary>
    public class ReqClientInitOver : BaseMessage
    {
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 105201;
		

        //构造函数
        public ReqClientInitOver() : base()
        {
        }
		
		//不缓存可以不调用
		public override void Reset()
		{
			base.Reset();
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
				
				
				XBuffer.WriteByte(0, _buffer_, ref _offset_);
				
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }
    }
	///<summary>客户端回复心跳</summary>
    public class ReqHeartBeat : BaseMessage
    {
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 105202;
		
		///<summary>发送时间</summary>
		public long clientTime;

        //构造函数
        public ReqHeartBeat() : base()
        {
        }
		
		//不缓存可以不调用
		public override void Reset()
		{
			base.Reset();
			clientTime = 0L;
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
								clientTime = XBuffer.ReadLong(_buffer_, ref _offset_);
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
				
						XBuffer.WriteLong(clientTime, _buffer_, ref _offset_);
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }
    }
	///<summary>检查resCode</summary>
    public class ReqCheckResCode : BaseMessage
    {
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 105203;
		
		///<summary>当前code</summary>
		public int code;

        //构造函数
        public ReqCheckResCode() : base()
        {
        }
		
		//不缓存可以不调用
		public override void Reset()
		{
			base.Reset();
			code = 0;
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
								code = XBuffer.ReadInt(_buffer_, ref _offset_);
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
				
						XBuffer.WriteInt(code, _buffer_, ref _offset_);
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }
    }
	///<summary>提示信息</summary>
    public class ResPrompt : BaseMessage
    {
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 105103;
		
		///<summary>提示信息类型（1Tip提示，2跑马灯，3插队跑马灯，4弹窗，5弹窗回到登陆，6弹窗退出游戏）</summary>
		public int type;
		///<summary>提示信息(语言包id 0时直接显示content)</summary>
		public int lanId;
		///<summary>填充字段</summary>
		public string content;

        //构造函数
        public ResPrompt() : base()
        {
        }
		
		//不缓存可以不调用
		public override void Reset()
		{
			base.Reset();
			type = 0;
			lanId = 0;
			content = null;
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
								type = XBuffer.ReadInt(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 1)
					{
								lanId = XBuffer.ReadInt(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 2)
					{
								content = XBuffer.ReadString(_buffer_, ref _offset_);
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
				
						XBuffer.WriteInt(type, _buffer_, ref _offset_);
						XBuffer.WriteInt(lanId, _buffer_, ref _offset_);
						XBuffer.WriteString(content, _buffer_, ref _offset_);
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }
    }
	///<summary>检查resCode</summary>
    public class ResCheckResCode : BaseMessage
    {
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 105102;
		
		///<summary>code是否匹配</summary>
		public bool isMatch;
		///<summary>服务器时间</summary>
		public long tick;

        //构造函数
        public ResCheckResCode() : base()
        {
        }
		
		//不缓存可以不调用
		public override void Reset()
		{
			base.Reset();
			isMatch = false;
			tick = 0L;
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
								isMatch = XBuffer.ReadBool(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 1)
					{
								tick = XBuffer.ReadLong(_buffer_, ref _offset_);
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
				
				
				XBuffer.WriteByte(3, _buffer_, ref _offset_);
				
						XBuffer.WriteBool(isMatch, _buffer_, ref _offset_);
						XBuffer.WriteLong(tick, _buffer_, ref _offset_);
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }
    }
	///<summary>服务器发送心跳</summary>
    public class ResHeartBeat : BaseMessage
    {
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 105104;
		
		///<summary>发送时间</summary>
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
	///<summary>升级</summary>
    public class ResLevelUp : BaseMessage
    {
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 105107;
		
		///<summary>等级</summary>
		public int level;

        //构造函数
        public ResLevelUp() : base()
        {
        }
		
		//不缓存可以不调用
		public override void Reset()
		{
			base.Reset();
			level = 0;
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
								level = XBuffer.ReadInt(_buffer_, ref _offset_);
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
				
						XBuffer.WriteInt(level, _buffer_, ref _offset_);
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }
    }
	///<summary>服务器解锁客户端屏幕</summary>
    public class ResUnlockScreen : BaseMessage
    {
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 105119;
		
		///<summary>错误码0:无错误</summary>
		public int errCode;
		///<summary>错误描述</summary>
		public string desc;

        //构造函数
        public ResUnlockScreen() : base()
        {
        }
		
		//不缓存可以不调用
		public override void Reset()
		{
			base.Reset();
			errCode = 0;
			desc = null;
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
								errCode = XBuffer.ReadInt(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 1)
					{
								desc = XBuffer.ReadString(_buffer_, ref _offset_);
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
				
				
				XBuffer.WriteByte(3, _buffer_, ref _offset_);
				
						XBuffer.WriteInt(errCode, _buffer_, ref _offset_);
						XBuffer.WriteString(desc, _buffer_, ref _offset_);
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }
    }
	///<summary>跨天</summary>
    public class ResAcrossDay : BaseMessage
    {
		static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override int GetMsgId() { return MsgId; }
        public const int MsgId = 105120;
		
		///<summary>世界等级</summary>
		public int worldLevel;
		///<summary>开服天数</summary>
		public int openServerDays;

        //构造函数
        public ResAcrossDay() : base()
        {
        }
		
		//不缓存可以不调用
		public override void Reset()
		{
			base.Reset();
			worldLevel = 0;
			openServerDays = 0;
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
								worldLevel = XBuffer.ReadInt(_buffer_, ref _offset_);
					} else { break; }
					
					if(_fieldNum_ > 1)
					{
								openServerDays = XBuffer.ReadInt(_buffer_, ref _offset_);
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
				
				
				XBuffer.WriteByte(3, _buffer_, ref _offset_);
				
						XBuffer.WriteInt(worldLevel, _buffer_, ref _offset_);
						XBuffer.WriteInt(openServerDays, _buffer_, ref _offset_);
				
            }
            catch(Exception ex)
            {
                throw ex;
            }
			return _offset_;
        }
    }
}