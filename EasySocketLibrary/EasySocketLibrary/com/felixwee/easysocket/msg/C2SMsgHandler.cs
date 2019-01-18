/**************************************
          S2CMsgHandler

    
    author:felixwee
	email:felixwee@163.com
	blog: www.felixwee.com
    desc: 用于处理服务器端消息类
	
	
****************************************/

using com.felixwee.easysocket;
using com.felixwee.easysocket.msg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public delegate void C2SCallbackHandler(EasyTCPToken token, NetMessage msg);

namespace com.felixwee.easysocket.msg
{
    public class C2SMsgHandler
    {

        public int msgId=-1;

        public C2SMsgHandler(int msgId)
        {
            this.msgId = msgId;
        }

        public event C2SCallbackHandler OnCallback =null;

        public void ProcessData(EasyTCPToken token, NetMessage msg)
        {
            if (OnCallback != null)
            {
                OnCallback(token, msg);
            }
        }
    }
}
