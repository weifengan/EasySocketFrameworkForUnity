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

public class ServerDemo : MonoBehaviour {

    private EasyTCPServer server;

    public string msgStr = "";
	// Use this for initialization
	void Start () {
        server = EasyTCPServer.CreateServer("127.0.0.1", 9339);
        server.OnServerEventHandler += Client_OnSocketEventHandler;
        server.Msg.AddMsgId(1);
        server.Msg.FindHandlerById(1).OnCallback += ClientDemo_OnCallback;
	}

    private void ClientDemo_OnCallback(EasyTCPToken client, NetMessage msg)
    {
        msgStr+="\n" + msg.ReadUTFString();
    }

    private void Client_OnSocketEventHandler(EasySocketEvent evt)
    {
        switch (evt.Type)
        {
            case EasySocketEvent.SERVER_START_SUCCESS:
                msgStr += "\nserver start success";
                break;
            case EasySocketEvent.NEW_CONNECTION:
                msgStr += "\nnew client connected";
                break;
            case EasySocketEvent.DISCONNECTED:
                msgStr += "\nclient lost" + evt.UserToken.Ipe.ToString();
                break;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}

    void OnGUI()
    {
        if(GUI.Button(new Rect(0, 0, 120, 30), "启动服务器"))
        {
            server.StartServer();
        }
        if (GUI.Button(new Rect(0, 30, 120, 30), "发送消息"))
        {
            NetMessage msg = new NetMessage(1);
            msg.WriteUTFString("我就服务器端发来的消息Server");
            server.BroadcastNetMessage(msg);
        }

        GUI.Label(new Rect(140, 0, 800, 600), msgStr);


    }
}
