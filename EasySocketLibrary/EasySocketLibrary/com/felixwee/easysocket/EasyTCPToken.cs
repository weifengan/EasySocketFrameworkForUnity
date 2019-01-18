using com.felixwee.easysocket.events;
using com.felixwee.easysocket.msg;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace com.felixwee.easysocket
{
    public class EasyTCPToken : MonoBehaviour
    {
        #region Socket相关
        private Socket _socket;
        private IPEndPoint _ipe;
        public IPEndPoint Ipe
        {
            get
            {
                return _ipe;
            }
        }
        //在线状态
        public bool _online = false;
        #endregion


        #region 数据接收相关
        private bool _receiveLoop = true;
        private Thread _reciveThread;
        private byte[] _receiveBuffer = new byte[1024 * 32];
        #endregion


        #region 最后一次心跳时间
        public string m_LastHeartTime = "";
        private System.DateTime _lastHeartBeatTime;
        public System.DateTime LastHeartTime
        {
            get
            {
                return _lastHeartBeatTime;
            }
        }
        #endregion


        #region 其他
        private EasyTCPServer _server;
        public EasyTCPServer Server
        {
            get
            {
                return _server;
            }
        }

        //对外时间回调
        public event SocketEventHandler OnSocketEventHandler = null;
        //客户端事件队列
        private Queue<EasySocketEvent> _socketEventQueue = new Queue<EasySocketEvent>();
        #endregion




        /// <summary>
        /// 创建一个服务区端客户端处理对象EasyTCPUserToken
        /// </summary>
        /// <param name="socket">客户端套接字</param>
        /// <param name="server">所属服务器</param>
        /// <returns></returns>
        public static EasyTCPToken CreateTCPToken(Socket socket, EasyTCPServer server)
        {
            IPEndPoint ipe = (IPEndPoint)socket.RemoteEndPoint;
            GameObject go = new GameObject("【TCPClient】" + ipe.Address + ":" + ipe.Port);
            EasyTCPToken token = go.AddComponent<EasyTCPToken>();
            token._server = server;
            token._ipe = ipe;
            token._socket = socket;
            token._online = true;
            token._lastHeartBeatTime = System.DateTime.Now;

            //开启接接收线程
            token._receiveLoop = true;
            token._reciveThread = new Thread(new ThreadStart(token.ReciveMessageHandler));
            token._reciveThread.IsBackground = true;
            token._reciveThread.Start();
            return token;
        }

        /// <summary>
        /// 接收数据处理函数
        /// </summary>
        private void ReciveMessageHandler()
        {
            while (_receiveLoop)
            {
                int length = _socket.Receive(_receiveBuffer);
                try
                {
                    if (length > 0)
                    {
                        byte[] buffers = new byte[length];
                        Buffer.BlockCopy(_receiveBuffer, 0, buffers, 0, length);
                        //统计数据
                        _server._bytesReceived += length;
                        //转换消息
                        NetMessage msg = new NetMessage(buffers);

                        //收到消息即更心心跳时间
                        _lastHeartBeatTime = System.DateTime.Now;
                        //判断是否为心跳包
                        if (msg.MsgId == 0)
                        {
                            //回复客户端心跳消息
                            NetMessage hbm = new NetMessage(0);
                            this.SendNetMessage(hbm);
                        }
                        EasySocketEvent nse = new EasySocketEvent(EasySocketEvent.DATA_RECEIVED, this, msg);
                        _socketEventQueue.Enqueue(nse);

                    }
                    else
                    {
                        _receiveLoop = false;
                        //发送消息事件
                        EasySocketEvent nse = new EasySocketEvent(EasySocketEvent.DISCONNECTED, this, "客户端主动关闭了Socket连接");
                        _socketEventQueue.Enqueue(nse);
                    }
                }
                catch (SocketException se)
                {
                    EasySocketEvent nse = new EasySocketEvent(EasySocketEvent.SOCKET_ERROR, this, se.Message);
                    _socketEventQueue.Enqueue(nse);
                    _receiveLoop = false;
                }
                catch (Exception ex)
                {
                    //发送消息事件
                    EasySocketEvent nse = new EasySocketEvent(EasySocketEvent.READ_SOCKET_ERROR, this, ex.Message);
                    _socketEventQueue.Enqueue(nse);

                }
            }
        }

        public void SendNetMessage(NetMessage msg)
        {
            if (_socket.Connected)
            {
                _socket.Send(msg.Bytes);
                _server._bytesSended += msg.Bytes.Length;
            }
        }

        void Update()
        {
            //网络事件处理
            while (_socketEventQueue.Count > 0)
            {
                EasySocketEvent evt = _socketEventQueue.Dequeue();
                if (OnSocketEventHandler != null)
                {
                    OnSocketEventHandler(evt);
                }
            }
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            long timeStamp = (long)(_lastHeartBeatTime - startTime).TotalSeconds; // 相差秒数
            m_LastHeartTime = timeStamp.ToString();
        }

        public void Close()
        {
            _receiveLoop = false;
            if (_socket != null)
            {
                _socket.Close();
            }
            Destroy(this.gameObject);
        }
        void OnDestroy()
        {
            _receiveLoop = false;
        }
    }
}
