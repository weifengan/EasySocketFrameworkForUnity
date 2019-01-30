/*************************************
			
		EasyUDPClient 
		
   @desction:
   @author:felixwee
   @email:felixwee@163.com
   @website:www.felixwee.com
  
***************************************/
using com.felixwee.easysocket;
using com.felixwee.easysocket.msg;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class EasyUDPClient : MonoBehaviour {

 
    private enum ClientType
    {
        P2P,BROADCAST
    }
    private Socket _receiveSocket;
    private Thread _receiveThread;
    private bool _loop = true;
    private string _host;
    private int _port;
    private IPEndPoint _receiveEndPoint;
    private byte[] _receiveBuffer = new byte[8192];
    private Socket _sendSocket;

    private Socket _p2preceiveSocket;
    private IPEndPoint _p2preceiveEndpoint;
    private Thread _p2preceiveThread;
    private bool _p2preceiveLoop = true;


    private Socket _p2psendSocket;
    private IPEndPoint _p2psendIpe;


    private ClientType clientType;


   

    public static EasyUDPClient CreateSimpleUDPClient(string host="127.0.0.1",int _receivePort = 9339)
    {
        GameObject go = new GameObject("[UDPClient]" + host+":"+_receivePort);
        EasyUDPClient client = go.AddComponent<EasyUDPClient>();
        client._host = host;
        client._port = _receivePort;
        client.clientType = ClientType.BROADCAST;
        client._receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        client._receiveEndPoint = new IPEndPoint(IPAddress.Any, _receivePort);
        client._receiveSocket.Bind(client._receiveEndPoint);
        return client;
    }	 

   /// <summary>
   /// 创建UDP点对点客户端
   /// </summary>
   /// <param name="host">消息接收主机IP</param>
   /// <param name="port">消息接收端口</param>
   /// <param name="_sendPort">数据发送端口  0 为系统可用端口</param>
   /// <returns></returns>
    public static EasyUDPClient CreateP2PClient(string host = "127.0.0.1", int port = 9339,int _sendPort=0)
    {
        GameObject go = new GameObject("[UDPP2PClient]");
        EasyUDPClient client = go.AddComponent<EasyUDPClient>();
        go.transform.SetParent(EasySocket.Instance.transform);

        client.clientType = ClientType.P2P;
        client._p2preceiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        client._p2preceiveEndpoint = new IPEndPoint(IPAddress.Parse(host), port);
        client._p2preceiveSocket.Bind(client._p2preceiveEndpoint);


        client._p2psendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        client._p2psendIpe = new IPEndPoint(IPAddress.Any, _sendPort);

        client.name = "[P2PClient]" + port + "+" + client._p2psendIpe.Port;
        return client;
    }


    public void Init()
    {
        switch (clientType)
        {
            case  ClientType.P2P:
                //开启接收线程
                _p2preceiveThread = new Thread(new ThreadStart(P2PReceiveMessage));
                _p2preceiveThread.IsBackground = true;
                _p2preceiveThread.Start();

                break;
            case ClientType.BROADCAST:

                break;
        }
     }

    /// <summary>
    /// 接收点对点数据
    /// </summary>
    private void P2PReceiveMessage()
    {
        EndPoint _ep = (EndPoint)_p2preceiveEndpoint;
        while (_p2preceiveLoop)
        {
            try
            {
                int receiveSize = _p2preceiveSocket.ReceiveFrom(_receiveBuffer, ref _ep);

                if (receiveSize >= 8)
                {

                    byte[] buffers = new byte[receiveSize];
                    Buffer.BlockCopy(_receiveBuffer, 0, buffers, 0, receiveSize);

                    NetMessage msg = new NetMessage(buffers);
                    Debug.Log("收到消息" + msg.ReadUTFString());
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error  " + ex.Message);
                Shutdown();
            }
        }
    }



    public void SendP2PNetMessageTo(NetMessage msg,IPEndPoint remoteEndPoint)
    {
        _p2psendSocket.SendTo(msg.Bytes, remoteEndPoint);
    }

    public void SendP2PNetMessageTo(NetMessage msg, string host,int port)
    {
        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
        _p2psendSocket.SendTo(msg.Bytes, remoteEndPoint);
        Debug.Log("发送消息");
    }


    private void ReceiveMsgThread()
    {
        EndPoint ep = (EndPoint)_receiveEndPoint;
        while (_loop)
        {

            try
            {
                int receiveSize = _receiveSocket.ReceiveFrom(_receiveBuffer, ref ep);

                if (receiveSize >= 8)
                {

                    byte[] buffers = new byte[receiveSize];
                    Buffer.BlockCopy(_receiveBuffer, 0, buffers, 0, receiveSize);

                    NetMessage msg = new NetMessage(buffers);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error  " + ex.Message);
                Shutdown();
            }

        }
    }

    
    public void SendNetMessage(NetMessage msg)
    {
        IPEndPoint remmoteIpe = new IPEndPoint(IPAddress.Parse(_host), _port);
    }

    public void Shutdown()
    {
        _p2preceiveLoop = false;

        if (_p2preceiveSocket != null)
        {
            _p2preceiveSocket.Close();
        }

        _loop = false;

        if (_receiveThread != null)
        {
            _receiveThread.Interrupt();
            _receiveThread.Abort();
            _receiveThread = null;
        }

        if (_receiveSocket != null)
        {
            _receiveSocket.Close();
        }
    }

    public void OnDestroy()
    {
        
        Shutdown();
    }
}
