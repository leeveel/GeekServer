using System;
using System.Collections.Generic;

using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;

namespace Geek.Server
{
    /// <summary>
    /// 消息解码器
    /// </summary>
    public class TcpServerDecoder : ByteToMessageDecoder
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        /// 从客户端接收的包大小最大值（单位：字节 1M）
        const int MAX_RECV_SIZE = 1024 * 1024;

        /// 上次从客户端接收到一个完整包的的时间
        static readonly AttributeKey<OneParam<long>> LAST_RECV_TIME = AttributeKey<OneParam<long>>.ValueOf("last_recv_time");

        /// 上次从客户端接收到一个完整包的序列号
        static readonly AttributeKey<OneParam<int>> LAST_RECV_ORDER = AttributeKey<OneParam<int>>.ValueOf("last_recv_order");

        /// <summary>
        /// 解析消息包
        /// </summary>
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                //数据包长度为一个int至少4个字节
                if (input.ReadableBytes < 4)
                    return;
                
                //仅仅是Get没有Read
                int msgLen = input.GetInt(input.ReaderIndex);
                if (!CheckMsgLen(msgLen))
                {
                    context.CloseAsync();
                    return;
                }

                //如果接受数据不够，等待下次
                if (input.ReadableBytes < msgLen)
                {
                    return;
                }

                //在buffer中读取掉消息长度
                input.ReadInt();

                // 时间合法性检查
                long time = input.ReadLong();
                if (!CheckTime(context, time))
                {
                    context.CloseAsync();
                    return;
                }

                // 序列号检查
                int order = input.ReadInt();
                if (!CheckMagicNumber(context, order, msgLen))
                {
                    context.CloseAsync();
                    return;
                }

                //消息id
                int msgId = input.ReadInt();

                byte[] msgData = null;
                msgData = new byte[msgLen - 20];
                input.ReadBytes(msgData);

                var msg = TcpHandlerFactory.GetMsg(msgId);
                if (msg == null)
                {
                    LOGGER.Error("消息ID:{} 找不到对应的Msg.", msgId);
                    return;
                }
                else
                {
                    if (msg.GetMsgId() == msgId)
                    {
                        msg.Deserialize(msgData);
                    }
                    else
                    {
                        LOGGER.Error("后台解析消息失败，注册消息id和消息无法对应.real:{0}, register:{1}", msg.GetMsgId(), msgId);
                        return;
                    }
                }
                output.Add(msg);
            }
            catch (Exception e)
            {
                LOGGER.Error(e, "解析数据异常," + e.Message + "\n" + e.StackTrace);
            }
        }

        bool CheckMagicNumber(IChannelHandlerContext context, int order, int msgLen)
        {
            order ^= (0x1234 << 8);
            order ^= msgLen;
            var _order = context.Channel.GetAttribute(LAST_RECV_ORDER).Get();
            if (_order == null)
                _order = new OneParam<int>(0x1234);
            int lastOrder = _order.value;
            if (order != lastOrder + 1)
            {
                LOGGER.Error("包序列出错, order=" + order + ", lastOrder=" + lastOrder);
                return false;
            }
            _order.value = order;
            context.Channel.GetAttribute(LAST_RECV_ORDER).Set(_order);
            return true;
        }

        /// <summary>
        /// 检查消息长度是否合法
        /// </summary>
        /// <param name="msgLen"></param>
        /// <returns></returns>
        bool CheckMsgLen(int msgLen)
        {
            //消息长度+时间戳+magic+消息id+数据
            //4 + 8 + 4 + 4 + data
            if (msgLen <= 20)
            {
                LOGGER.Error("从客户端接收的包大小异常:" + msgLen + ":至少20个字节");
                return false;
            }
            else if (msgLen > MAX_RECV_SIZE)
            {
                LOGGER.Error("从客户端接收的包大小超过限制：" + msgLen + "字节，最大值：" + MAX_RECV_SIZE/1024 + "字节");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 时间戳检查(可以防止客户端游戏过程中修改时间)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        bool CheckTime(IChannelHandlerContext context, long time)
        {
            var _time = context.Channel.GetAttribute(LAST_RECV_TIME).Get();
            if (_time != null)
            {
                long lastTime = _time.value;
                if (lastTime > time)
                {
                    LOGGER.Error("时间戳出错，time=" + time + ", lastTime=" + lastTime + " " + context);
                    return false;
                }
                _time.value = time;
            }
            else
            {
                _time = new OneParam<long>(time);
            }
            context.Channel.GetAttribute(LAST_RECV_TIME).Set(_time);
            return true;
        }
    }
}