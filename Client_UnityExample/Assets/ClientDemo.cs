/*************************************
			
		ClientDemo 
		
   @desction:
   @author:felixwee
   @email:felixwee@163.com
   @website:www.felixwee.com
  
***************************************/
using com.felixwee.easysocket;
using com.felixwee.easysocket.events;
using com.felixwee.easysocket.msg;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientDemo : MonoBehaviour {

    private EasyTCPClient client;

    private string msgStr = "";
	// Use this for initialization
	void Start () {
        client = EasyTCPClient.CreateClient("127.0.0.1", 9339,true);
        client.OnSocketEventHandler += Client_OnSocketEventHandler;
        client.msgMgr.AddMsgId(1);
        client.msgMgr.FindHandlerById(1).OnCallback += ClientDemo_OnCallback;
	}

    private void ClientDemo_OnCallback(EasyTCPClient client, NetMessage msg)
    {
        //读取字符串
        msgStr+= "\n"+msg.ReadUTFString();
        
    }

    private void Client_OnSocketEventHandler(EasySocketEvent evt)
    {
        switch (evt.Type)
        {
            case EasySocketEvent.CONNECTED:
                msgStr+="\n connected to server";
                break;
            case EasySocketEvent.CONNECT_ERROR:
                msgStr += "\n connect failed to server"+evt.Info;
                break;
            case EasySocketEvent.DISCONNECTED:
                msgStr += "\n 掉线了";
                break;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}

    void OnGUI()
    {
        if(GUI.Button(new Rect(0, 0, 120, 30), "连接"))
        {
           client.StartConnect(true);
        }
        if (GUI.Button(new Rect(0, 30, 120, 30), "发送消息"))
        {
            NetMessage msg = new NetMessage(1);
            msg.WriteUTFString("我就客户端发来的消息");
            client.SendNetMessage(msg);
        }


        GUI.Label(new Rect(140, 0, 800, 600), msgStr);



    }
}
