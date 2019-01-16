/*************************************
			
		TextMessage 
		
   @desction:  网络传输文本类消息
   @author:felixwee
   @email:felixwee@163.com
   @website:www.felixwee.com
  
***************************************/
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace com.felixwee.easysocket.msg
{
    public class TextMessage : NetMessage
    {
        public TextMessage(byte[] bytes) : base(bytes)
        {

        }

        public TextMessage(int msgId, string msgBody) : base(msgId)
        {
            this.WriteUTFString(msgBody);
        }

        public string Text
        {
            get
            {
                this.Position = 0;
                string text = this.ReadUTFString();
                return text;
            }
        }

        public override string ToString()
        {
            return this.GetType().Name + " {Id:" + MsgId + ",Length:" + this.Bytes.Length + ", Text:" + Text + "}";
        }
    }
}
