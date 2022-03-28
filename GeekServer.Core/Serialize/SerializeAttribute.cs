using System;

namespace Geek.Server
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SClassAttribute : Attribute
    {
        /// <summary>
        /// 是否将ClassId作为消息ID
        /// </summary>
        public bool IsMsg { get; private set; }
        /// <summary>
        /// 是否可以作为State回存数据库
        /// </summary>
        public bool IsState { get; private set; }
        public int Id { get; private set; }
        public SClassAttribute(int id, bool isMsg = true, bool isState = false)
        {
            Id = id;
            IsMsg = isMsg;
            IsState = isState;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class SFieldAttribute : Attribute
    {
        public int Id { get; private set; }
        public bool Optional { get; private set; }
        public SFieldAttribute(int id, bool optional=false)
        {
            Id = id;
            Optional = optional;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SPropertyAttribute : Attribute
    {
        public int Id { get; private set; }
        public bool Optional { get; private set; }
        public SPropertyAttribute(int id, bool optional = false)
        {
            Id = id;
            Optional = optional;
        }
    }


    /// <summary>
    /// serialize option
    /// </summary>
    public class SO
    {
        /// <summary>
        /// 标识该类为客户端和服务器通信的网络消息
        /// </summary>
        public const bool Msg = true;
        public const bool NotMsg = false;

        /// <summary>
        /// 标识改类可以作为State回存数据库
        /// </summary>
        public const bool State = true;
        public const bool NotState = false;

        /// <summary>
        /// 标识此字段是可选的
        /// </summary>
        public const bool Optional = true;
    }



}
