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
using UnityEngine;

public delegate void S2CCallbackHandler(EasyTCPClient client, NetMessage msg);
namespace com.felixwee.easysocket.msg
{
    public class S2CMsgHandler
    {

        public int msgId=-1;
        public S2CMsgHandler(int msgId)
        {
            this.msgId = msgId;
        }
        public event S2CCallbackHandler OnCallback = null;
        public void ProcessData(EasyTCPClient client,NetMessage msg)
        {
            if (OnCallback != null)
            {
                OnCallback(client, msg);
            }
        }
       
    }
}
