/**************************************
          EasyUDPServer

    
    author:felixwee
	email:felixwee@163.com
	blog: www.felixwee.com
    desc:
	
	
****************************************/

using com.felixwee.easysocket;
using com.felixwee.easysocket.events;
using com.felixwee.easysocket.msg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace com.felixwee.easysocket
{
    public class EasyUDPServer :MonoBehaviour
    {
        private enum UDPServerType
        {
            //点对点，广播
            P2P, BROADCAST
        }
        private bool _debug = false;
        private int _broadcastPort = -1;
        private UDPServerType _serverType;


        //广播相关参数
        private Socket _broadcastSocket;
        private IPEndPoint _boradcastIpe;

        //接收参数
        private Socket _receiveSocket;
        private int _receivePort;
        private bool _loop = true;
        private Thread _receiveThread;
        private byte[] _receiveBuffer = new byte[8196];
        private IPEndPoint _receiveIpe;

        //P2P 服务器相关
        private Socket _p2preceiveSocket;
        private Thread _p2preceiveThread;
        private bool _p2preceiveLoop = true;
        private byte[] _p2pReceiveBuffer = new byte[8196];
        private IPEndPoint _p2preceiveEndPoint;

        private Socket _p2psendSocket;
        private IPEndPoint _p2psendEndPoint;

        public C2SMsgManager msgMgr;
        //对外公开时间列表
        public event SocketEventHandler OnSocketEventHandler = null;
        private Queue<EasySocketEvent> _socketEventQueue = new Queue<EasySocketEvent>();

        public static EasyUDPServer CreateBroadcastServer(int broadcastPort = 9339, bool debug = true)
        {
            GameObject go = new GameObject("[UDP BroadcastServer]" + broadcastPort);
            EasyUDPServer server = go.AddComponent<EasyUDPServer>();
            server._serverType = UDPServerType.BROADCAST;
            server._debug = debug;

            server._broadcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //设置socket为广播模式
            server._broadcastSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            server._boradcastIpe = new IPEndPoint(IPAddress.Broadcast, server._broadcastPort);
            server.transform.SetParent(EasySocket.Instance.transform);

            server.Log("初始化" + go.name + "成功!");
            return server;
        }


        /// <summary>
        /// 创建P2P服务器
        /// </summary>
        /// <param name="host">服务器IP</param>
        /// <param name="receivePort">数据接口端口</param>
        /// <param name="sendPort">数据发送端口</param>
        /// <param name="debug">是否为调试模式</param>
        /// <returns></returns>
        public static EasyUDPServer CreateP2PServer(string host = "127.0.0.1", int receivePort = 9339, int sendPort = 0, bool debug = false)
        {
            GameObject go = new GameObject("[UDPP2PServer]" + receivePort);
            EasyUDPServer server = go.AddComponent<EasyUDPServer>();
            server._debug = debug;
            server._serverType = UDPServerType.P2P;
            go.transform.SetParent(EasySocket.Instance.transform);

            Debug.Log(server._debug);
            //发送字节
            server._p2psendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server._p2psendEndPoint = new IPEndPoint(IPAddress.Any, sendPort);
            go.name = "[UDP-P2PServer]" + receivePort + " - " + server._p2psendEndPoint.Port;
            server.Log("创建UDP服务器成功" + host + ":" + receivePort + "/" + server._p2psendEndPoint.Port);

            //接收字节
            server._p2preceiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server._p2preceiveEndPoint = new IPEndPoint(IPAddress.Parse(host), receivePort);
            server._p2preceiveSocket.Bind(server._p2preceiveEndPoint);

            server.msgMgr = new C2SMsgManager("[MSG]" + host + ":" + receivePort);
            return server;
        }


        public void StartServer()
        {
            switch (_serverType)
            {
                case UDPServerType.BROADCAST:
                    break;
                case UDPServerType.P2P:
                    _p2preceiveLoop = true;
                    Log("UDP P2P服务器启动成功!" + _p2preceiveEndPoint.ToString());

                    _p2preceiveThread = new Thread(new ThreadStart(ReceiveP2PMessage));
                    _p2preceiveThread.IsBackground = true;
                    _p2preceiveThread.Start();

                    //创建事件
                    EasySocketEvent nse = new EasySocketEvent(EasySocketEvent.SERVER_START_SUCCESS,this, "UDP P2P服务器启动成功!" + _p2preceiveEndPoint.ToString());
                    _socketEventQueue.Enqueue(nse);
                    break;
            }
        }


        private void Update()
        {
            //网络事件
            while (_socketEventQueue.Count > 0)
            {
                EasySocketEvent ese = _socketEventQueue.Dequeue();
                OnProcessSocketEventHandler(ese);
            }
        }

        private void OnProcessSocketEventHandler(EasySocketEvent evt)
        {
            if (OnSocketEventHandler != null)
            {
                OnSocketEventHandler(evt);
            }
            switch (evt.Type)
            {
                case EasySocketEvent.SERVER_START_SUCCESS:
                    Log(evt.Info);
                    break;
                case EasySocketEvent.DATA_RECEIVED:
                    Log("收到数据" + evt.SocketData);
                    break;
            }
        }


        /*****************P2P相关****************************/

        private void ReceiveP2PMessage()
        {
            while (_p2preceiveLoop)
            {
                EndPoint _ipe = _p2preceiveEndPoint;
                int receiveSize = _p2preceiveSocket.ReceiveFrom(_p2pReceiveBuffer, ref _ipe);
                if (receiveSize >= 8)
                {

                    byte[] buffers = new byte[receiveSize];
                    Buffer.BlockCopy(_p2pReceiveBuffer, 0, buffers, 0, receiveSize);

                    NetMessage msg = new NetMessage(buffers);
                    EasySocketEvent nse = new EasySocketEvent(EasySocketEvent.DATA_RECEIVED, this, msg);
                    _socketEventQueue.Enqueue(nse);


                    Log("收到消息" + msg.ReadUTFString());
                }
            }
        }
        public void SendNetMessageTo(string host, int port, NetMessage msg)
        {
            EndPoint point = new IPEndPoint(IPAddress.Parse(host), port);
            _p2preceiveSocket.SendTo(msg.Bytes, point);
        }

        public void SendNetMessageTo(EndPoint point, NetMessage msg)
        {
            _p2preceiveSocket.SendTo(msg.Bytes, point);
        }


        /******************UDP广播相关***********************/
        // 广播消息
        public void BroadcastNetMessage(NetMessage msg)
        {
            _broadcastSocket.SendTo(msg.Bytes, _boradcastIpe);
        }


        public void SendNetMessageTo(NetMessage msg, IPEndPoint remoteEndPoint)
        {
            _p2preceiveSocket.SendTo(msg.Bytes, remoteEndPoint);
        }



        public void Shutdown()
        {
            _p2preceiveLoop = false;
            if (_p2preceiveThread != null)
            {
                _p2preceiveThread.Interrupt();
                _p2preceiveThread.Abort();
                _p2preceiveThread = null;
            }
        }


        void OnDestroy()
        {
            Shutdown();
        }



        public void LogError(params object[] args)
        {
            if (_debug)
            {
                EasySocket.LogError(args);
            }
        }

        public void Log(params object[] args)
        {
            if (_debug)
            {
                EasySocket.Log(args);
            }
        }

        public void LogWarnning(params object[] args)
        {
            if (_debug)
            {
                EasySocket.LogWarning(args);
            }
        }
    }
}
