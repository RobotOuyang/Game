using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;
using LuaInterface;

namespace LuaFramework {
    public class ByteBuffer {
        MemoryStream stream = null;
        BinaryWriter writer = null;
        BinaryReader reader = null;

        public ByteBuffer() {
            stream = new MemoryStream();
            writer = new BinaryWriter(stream);
            reader = new BinaryReader(stream);
        }

        public ByteBuffer(byte[] data) {
            if (data != null) {
                stream = new MemoryStream(data);
            } else {
                stream = new MemoryStream();
            }
            writer = new BinaryWriter(stream);
            reader = new BinaryReader(stream);
        }

        public void Close() {
            if (writer != null) writer.Close();
            if (reader != null) reader.Close();

            stream.Close();
            writer = null;
            reader = null;
            stream = null;
        }

        public void WriteByte(byte v) {
            writer.Write(v);
        }

        public void WriteInt(int v) {
            writer.Write((int)v);
        }

        public void WriteShort(ushort v) {
            writer.Write((ushort)v);
        }

        public void WriteLong(long v) {
            writer.Write((long)v);
        }

        public void WriteFloat(float v) {
            byte[] temp = BitConverter.GetBytes(v);
            Array.Reverse(temp);
            writer.Write(BitConverter.ToSingle(temp, 0));
        }

        public void WriteDouble(double v) {
            byte[] temp = BitConverter.GetBytes(v);
            Array.Reverse(temp);
            writer.Write(BitConverter.ToDouble(temp, 0));
        }

        public void WriteString(string v) {
            byte[] bytes = Encoding.UTF8.GetBytes(v);
            writer.Write((int)bytes.Length);
            writer.Write(bytes);
        }

        public void WriteBytes(byte[] v) {
            writer.Write((int)v.Length);
            writer.Write(v);
        }

        public void WriteBuffer(LuaByteBuffer strBuffer) {
            WriteBytes(strBuffer.buffer);
        }

        public byte ReadByte() {
            return reader.ReadByte();
        }

        public int ReadInt() {
            return (int)reader.ReadInt32();
        }

        public ushort ReadShort() {
            return (ushort)reader.ReadInt16();
        }

        public long ReadLong() {
            return (long)reader.ReadInt64();
        }

        public float ReadFloat() {
            byte[] temp = BitConverter.GetBytes(reader.ReadSingle());
            Array.Reverse(temp);
            return BitConverter.ToSingle(temp, 0);
        }

        public double ReadDouble() {
            byte[] temp = BitConverter.GetBytes(reader.ReadDouble());
            Array.Reverse(temp);
            return BitConverter.ToDouble(temp, 0);
        }

        public string ReadString() {
            int len = ReadInt();
            byte[] buffer = new byte[len];
            buffer = reader.ReadBytes(len);
            return Encoding.UTF8.GetString(buffer);
        }

        public byte[] ReadBytes() {
            int len = ReadInt();
            return reader.ReadBytes(len);
        }

        public LuaByteBuffer ReadBuffer() {
            byte[] bytes = ReadBytes();
            return new LuaByteBuffer(bytes);
        }
        string BytesToString(byte[] buff)
        {
            string returnStr = string.Empty;
            for (int i = 0; i < buff.Length; i++)
            {
                returnStr += buff[i].ToString("X2");
            }
            return returnStr;
        }

        public LuaByteBuffer ReadBase64Buffer()
        {
            byte[] bytes = ReadBytes();
            // byte[] new_one = Convert.FromBase64String(Encoding.ASCII.GetString(bytes));
            byte[] new_one = Base64.Inst.Decode(bytes);
            // Debug.Log(BytesToString(new_one));
            return new LuaByteBuffer(new_one);
        }

        public void WriteBase64Buffer(LuaByteBuffer buffer)
        {
            byte[] bytes = buffer.buffer;
            //byte[] new_one = Encoding.ASCII.GetBytes(Convert.ToBase64String(bytes));
            byte[] new_one = Base64.Inst.Encode(bytes);
            WriteBytes(new_one);
        }

        public byte[] ToBytes() {
            writer.Flush();
            return stream.ToArray();
        }

        public void Flush() {
            writer.Flush();
        }
    }
}