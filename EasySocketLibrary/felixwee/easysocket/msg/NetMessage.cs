/*************************************
			
		NetMessage 
		
   @desction: 网络消息基础类
   @author:felixwee
   @email:weifengan@163.com
   @website:www.felixwee.com
  
***************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
namespace com.felixwee.easysocket.msg
{
    public class NetMessage
    {
        /// <summary>
        /// 整个消息字节数（包括头和体)
        /// </summary>
        private byte[] _totalBytes;
        private int _id;
        private int _length;
        //消息体字节数组（不包括消息头字节）
        private byte[] _bodybytes;
        private int _position = 0;
        public NetMessage(byte[] bytes)
        {
            _totalBytes = bytes;
            processMessage();
        }

        /// <summary>
        /// 创建消息对象
        /// </summary>
        /// <param name="msgId"></param>
        public NetMessage(int msgId)
        {
            _totalBytes = BitConverter.GetBytes(msgId);
            _id = msgId;
            processMessage();
        }

        private void processMessage()
        {
            //提取消息id
            byte[] idbytes = new byte[sizeof(int)];
            Buffer.BlockCopy(_totalBytes, 0, idbytes, 0, idbytes.Length);
            _id = BitConverter.ToInt32(idbytes, 0);

            //提取消息体
            byte[] bodybytes = new byte[_totalBytes.Length - idbytes.Length];
            Buffer.BlockCopy(_totalBytes, idbytes.Length, bodybytes, 0, bodybytes.Length);
            _bodybytes = bodybytes;

            Position = 0;
        }


        public int Length
        {
            get
            {
                return _totalBytes.Length;
            }
        }

        public int MsgId
        {
            get
            {
                return _id;
            }
        }

        public byte[] MsgBody
        {
            get
            {
                return _bodybytes;
            }
        }

        /// <summary>
        /// 字节数组浮动指针，外部值为0时，内部实际为4
        /// </summary>
        public int Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value + 4;
            }
        }


        /// <summary>
        /// 写入Int值
        /// </summary>
        /// <param name="value"></param>
        public void WriteInt(int value)
        {
            byte[] byte2write = BitConverter.GetBytes(value);
            byte[] newbytes = new byte[_totalBytes.Length + byte2write.Length];

            //将原数据复制到新数组中
            Buffer.BlockCopy(_totalBytes, 0, newbytes, 0, _totalBytes.Length);

            //写入新值
            Buffer.BlockCopy(byte2write, 0, newbytes, _totalBytes.Length, byte2write.Length);
            //更新总数组
            _totalBytes = newbytes;

            processMessage();
        }

        /// <summary>
        /// 写入一个float类型数据
        /// </summary>
        /// <param name="value"></param>
        public void WriteFloat(float value)
        {
            byte[] byte2write = BitConverter.GetBytes(value);
            byte[] newbytes = new byte[_totalBytes.Length + byte2write.Length];

            //将原数据复制到新数组中
            Buffer.BlockCopy(_totalBytes, 0, newbytes, 0, _totalBytes.Length);

            //写入新值
            Buffer.BlockCopy(byte2write, 0, newbytes, _totalBytes.Length, byte2write.Length);
            //更新总数组
            _totalBytes = newbytes;

            processMessage();
        }

        /// <summary>
        /// 写入Double值
        /// </summary>
        /// <param name="value"></param>
        public void WriteDouble(double value)
        {
            byte[] byte2write = BitConverter.GetBytes(value);
            byte[] newbytes = new byte[_totalBytes.Length + byte2write.Length];

            //将原数据复制到新数组中
            Buffer.BlockCopy(_totalBytes, 0, newbytes, 0, _totalBytes.Length);

            //写入新值
            Buffer.BlockCopy(byte2write, 0, newbytes, _totalBytes.Length, byte2write.Length);
            //更新总数组
            _totalBytes = newbytes;

            processMessage();

        }

        /// <summary>
        /// 写入一个long类型数据
        /// </summary>
        /// <param name="value"></param>
        public void WriteLong(long value)
        {
            EasySocket.Log("long-size-" + sizeof(long));
            byte[] byte2write = BitConverter.GetBytes(value);
            byte[] newbytes = new byte[_totalBytes.Length + byte2write.Length];

            //将原数据复制到新数组中
            Buffer.BlockCopy(_totalBytes, 0, newbytes, 0, _totalBytes.Length);

            //写入新值
            Buffer.BlockCopy(byte2write, 0, newbytes, _totalBytes.Length, byte2write.Length);
            //更新总数组
            _totalBytes = newbytes;

            processMessage();

        }


        public void WriteBoolean(bool value)
        {
            byte[] byte2write = BitConverter.GetBytes(value);
            byte[] newbytes = new byte[_totalBytes.Length + byte2write.Length];

            //将原数据复制到新数组中
            Buffer.BlockCopy(_totalBytes, 0, newbytes, 0, _totalBytes.Length);

            //写入新值
            Buffer.BlockCopy(byte2write, 0, newbytes, _totalBytes.Length, byte2write.Length);
            //更新总数组
            _totalBytes = newbytes;

            processMessage();
        }


        public void WriteUTFString(string value)
        {

            byte[] byte2write = Encoding.UTF8.GetBytes(value);
            byte[] byteslen = BitConverter.GetBytes(byte2write.Length);
            byte[] newbytes = new byte[_totalBytes.Length + byteslen.Length + byte2write.Length];

            //将原数据复制到新数组中
            Buffer.BlockCopy(_totalBytes, 0, newbytes, 0, _totalBytes.Length);

            //写入新值
            Buffer.BlockCopy(byteslen, 0, newbytes, _totalBytes.Length, byteslen.Length);
            Buffer.BlockCopy(byte2write, 0, newbytes, _totalBytes.Length + byteslen.Length, byte2write.Length);
            //更新总数组
            _totalBytes = newbytes;

            processMessage();

        }

        public void WriteBytes(byte[] bytes)
        {
            byte[] newbytes = new byte[_totalBytes.Length + bytes.Length];

            //将原数据复制到新数组中
            Buffer.BlockCopy(_totalBytes, 0, newbytes, 0, _totalBytes.Length);

            //写入新值
            Buffer.BlockCopy(bytes, 0, newbytes, _totalBytes.Length, bytes.Length);
            //更新总数组
            _totalBytes = newbytes;

            processMessage();
        }

        /// <summary>
        /// 读取Int类型
        /// </summary>
        /// <returns></returns>
        public int ReadInt()
        {
            byte[] value = new byte[sizeof(int)];
            Buffer.BlockCopy(_totalBytes, _position, value, 0, value.Length);
            _position += value.Length;
            return BitConverter.ToInt32(value, 0);
        }

        /// <summary>
        /// 读取float类型
        /// </summary>
        /// <returns></returns>
        public float ReadFloat()
        {
            byte[] value = new byte[sizeof(float)];
            Buffer.BlockCopy(_totalBytes, _position, value, 0, value.Length);
            _position += value.Length;
            return BitConverter.ToSingle(value, 0);
        }

        /// <summary>
        /// 读取Double类型
        /// </summary>
        /// <returns></returns>
        public double ReadDouble()
        {
            byte[] value = new byte[sizeof(double)];
            Buffer.BlockCopy(_totalBytes, _position, value, 0, value.Length);
            _position += value.Length;
            return BitConverter.ToDouble(value, 0);
        }

        /// <summary>
        /// 读取Long类型
        /// </summary>
        /// <returns></returns>
        public long ReadLong()
        {
            byte[] value = new byte[sizeof(long)];
            Buffer.BlockCopy(_totalBytes, _position, value, 0, value.Length);
            _position += value.Length;
            return BitConverter.ToInt64(value, 0);
        }

        /// <summary>
        /// 读取Boolean
        /// </summary>
        /// <returns></returns>
        public bool ReadBoolean()
        {
            byte[] value = new byte[sizeof(byte)];
            Buffer.BlockCopy(_totalBytes, _position, value, 0, value.Length);
            _position += value.Length;
            return BitConverter.ToBoolean(value, 0);
        }

        public string ReadUTFString()
        {
            byte[] byteslen = new byte[sizeof(int)];
            Buffer.BlockCopy(_totalBytes, _position, byteslen, 0, byteslen.Length);
            _position += byteslen.Length;
            int len = BitConverter.ToInt32(byteslen, 0);

            byte[] bytesstr = new byte[len];
            Buffer.BlockCopy(_totalBytes, _position, bytesstr, 0, bytesstr.Length);
            _position += bytesstr.Length;
            return Encoding.UTF8.GetString(bytesstr);
        }


        public byte[] ReadBytes(int length)
        {
            //读取长度
            byte[] bytes = new byte[sizeof(int)];
            Buffer.BlockCopy(_totalBytes, _position, bytes, 0, bytes.Length);
            _position += bytes.Length;
            return bytes;
        }

        public byte[] ToArray()
        {
            return _totalBytes;
        }

        public byte[] Bytes
        {
            get
            {
                return _totalBytes;
            }
        }


        public override string ToString()
        {
            return this.GetType().Name + " {Id:" + MsgId + ",Length:" + _totalBytes.Length + ", Bytes:" + _totalBytes + "}";
        }


    }
}
