//using System;
//using System.Buffers;

//namespace Geek.Server
//{
//    public interface ISerializable
//    {
//        int Read(byte[] buffer, int offset);

//        int Write(byte[] buffer, int offset);

//        int Read(Span<byte> buffer, int offset);

//        int Write(Span<byte> buffer, int offset);

//        byte[] Serialize();

//        void Serialize(Span<byte> span, int offset=0);

//        void Deserialize(byte[] data);

//        int GetSerializeLength();
//    }
//}
