﻿/**************************************
          MessageManager

    
    author:felixwee
	email:felixwee@163.com
	blog: www.felixwee.com
    desc:
	
	
****************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.felixwee.easysocket.msg
{

    public class C2SMsgManager
    {
        public string Name;
        private Dictionary<int, C2SMsgHandler> _msgDic = new Dictionary<int, C2SMsgHandler>();
        public C2SMsgManager(string name)
        {
            this.Name = name;
        }


        public bool AddMsgId(int msgId)
        {
            if (!_msgDic.ContainsKey(msgId))
            {
                _msgDic.Add(msgId, new C2SMsgHandler(msgId));
                return true;
            }
            else
            {
                Debug.Log("添加失败: 消息" + msgId + "已经注册过!");
            }
            return false;
        }

        public bool RemoveMsgId(int msgId)
        {
            if (!_msgDic.ContainsKey(msgId))
            {
                EasySocket.LogWarning("删除失败: 消息" + msgId + "未注册过!");
                return false;
            }
            _msgDic.Remove(msgId);
            return true;
        }


        public C2SMsgHandler FindHandlerById(int msgId)
        {
            if (!_msgDic.ContainsKey(msgId))
            {
                return null;
            }
            return _msgDic[msgId];
        }


        public void DisplayList()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in _msgDic.Keys)
            {
                sb.Append("" + item+" ");
            }
            Debug.Log(sb.ToString());
        }
    }
}
