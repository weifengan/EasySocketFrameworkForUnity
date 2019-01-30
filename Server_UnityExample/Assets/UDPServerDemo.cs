/*************************************
			
		UDPServerDemo 
		
   @desction:
   @author:felixwee
   @email:felixwee@163.com
   @website:www.felixwee.com
  
***************************************/
using com.felixwee.easysocket;
using com.felixwee.easysocket.msg;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class UDPServerDemo : MonoBehaviour {

    private EasyUDPServer server;
	// Use this for initialization
	void Start () {
        server = EasyUDPServer.CreateP2PServer("127.0.0.1", 9339,0);
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 120, 30), "Start"))
        {

            server.StartServer();
        }


        if (GUI.Button(new Rect(0, 35, 120, 30), "Send"))
        {
            NetMessage msg = new NetMessage(1);
            msg.WriteUTFString("hello");

            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9330);
            server.SendNetMessageTo(msg,ipe);
        }
    }


    void OnDestory()
    {
        server.Shutdown();
    }
}
