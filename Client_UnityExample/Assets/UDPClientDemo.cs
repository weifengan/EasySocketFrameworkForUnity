/*************************************
			
		UDPClientDemo 
		
   @desction:
   @author:felixwee
   @email:felixwee@163.com
   @website:www.felixwee.com
  
***************************************/
using com.felixwee.easysocket.msg;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UDPClientDemo : MonoBehaviour {

    EasyUDPClient client;
	// Use this for initialization
	void Start () {
        client = EasyUDPClient.CreateP2PClient("127.0.0.1",9330,9000);
        client.Init();

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnDestroy()
    {
        client.Shutdown();
    }


    void OnGUI()
    {
        if(GUI.Button(new Rect(0, 0, 120, 30), "发送"))
        {
            NetMessage msg = new NetMessage(2);
            msg.WriteUTFString("你好，Server");

            client.SendP2PNetMessageTo(msg, "127.0.0.1", 9339);
        }
    }
}
