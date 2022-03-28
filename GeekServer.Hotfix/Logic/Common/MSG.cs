using Geek.Server;
using System.Collections.Generic;

namespace Geek.Server
{

    /// <summary>
    /// 服务器错误码
    /// </summary>
    public enum ErrCode
    {
        Success = 0,
        ConfigErr = 400, //配置表错误
        ParamErr, //客户端传递参数错误
        CostNotEnough, //消耗不足

        //Login
        AccountCannotBeNull,
        UnknownPlatform,



        Notice = 100000, //正常通知
        FuncNotOpen, //功能未开启，主消息屏蔽
        Other //其他
    }


    /// <summary>
    /// 错误信息
    /// </summary>
    public struct ErrInfo
    {
        public ErrCode Code;
        public string Desc;

        public static ErrInfo Success { get; } = new ErrInfo(ErrCode.Success);

        public ErrInfo(ErrCode errCode, string desc = null)
        {
            Code = errCode;
            Desc = desc;
        }

        public static ErrInfo Create(ErrCode errCode = ErrCode.Success, string desc = null)
        {
            var res = new ErrInfo();
            res.Code = errCode;
            res.Desc = desc;
            return res;
        }

        public override string ToString()
        {
            return Code.ToString() + ">" + (Desc == null ? "" : Desc);
        }
    }


    public class MSG
    {
        public byte[] ByteArr {
            get
            {
                if (msg != null)
                    return msg.Serialize();
                return null;
            }
        }
        public int MsgId {
            get
            {
                if (msg != null)
                    return msg.MsgId;
                return 0;
            }
        }

        public int UniId { get; private set; }

        public ErrInfo Info { get; private set; }

        public IMessage msg { get; private set; }


        public static MSG Create(ErrCode errCode, string desc, IMessage msg = null)
        {
            var res = new MSG();
            res.Info = new ErrInfo(errCode, desc);
            res.msg = msg;
            return res;
        }

        public static MSG Create(ErrCode errCode, int uniId, string desc, IMessage msg = null)
        {
            var res = new MSG();
            res.Info = new ErrInfo(errCode, desc);
            res.UniId = uniId;
            res.msg = msg;
            return res;
        }

        public static MSG Create(ErrInfo errInfo, int uniId = 0, IMessage msg = null)
        {
            var res = new MSG();
            res.UniId = uniId;
            res.Info = errInfo;
            res.msg = msg;
            return res;
        }

        public static MSG Create(ErrCode errCode, IMessage msg = null, int uniId = 0)
        {
            var res = new MSG();
            res.UniId = uniId;
            res.Info = ErrInfo.Create(errCode);
            res.msg = msg;
            return res;
        }

        public static MSG Create(IMessage msg = null, int uniId = 0)
        {
            var res = new MSG();
            res.UniId = uniId;
            res.Info = ErrInfo.Success;
            res.msg = msg;
            return res;
        }

    }
}
