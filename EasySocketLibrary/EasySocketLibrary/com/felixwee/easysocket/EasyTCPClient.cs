/*************************************
			
		EasyTCPClient 
		
   @desction:
   @author:felixwee
   @email:felixwee@163.com
   @website:www.felixwee.com
  
***************************************/
using com.felixwee.easysocket.events;
using com.felixwee.easysocket.msg;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using UnityEngine;

namespace com.felixwee.easysocket
{
    public class EasyTCPClient : MonoBehaviour
    {


        #region Socket相关
        private Socket _socket;
        private string _host;
        private int _port;
        public long bytesSended = 0;
        public long bytesReceived = 0;
        //在线状态
        public bool _isOnline = false;
        //缓冲区大小
        private byte[] _receiveBuffer = new byte[4096];

        #endregion


        #region 接收相关
        public bool _receiveLoop = true;
        private Thread _receiveThread;
        #endregion


        #region 心跳相关
        public bool _heartbeatLoop = false;
        private Thread _heartbeatThread;
        private int _heartbeartInterval = 5;

        private System.DateTime _lastHeartBeatTime;
        public string m_LastHeartBeatTimeStamp = "0";
        #endregion


        #region 在线状态检测
        public bool _onlinecheckLoop = false;
        private Thread _onlineCheckThread;
        #endregion



        #region 其他
        public S2CMsgManager msgMgr;
        private bool _autoReconnect = false;
        private bool _debug = false;
        public event SocketEventHandler OnSocketEventHandler = null;
        //对外时间队列
        private Queue<EasySocketEvent> _socketEventQueue = new Queue<EasySocketEvent>();
        #endregion


        public static EasyTCPClient CreateClient(string host = "127.0.0.1", int port = 9339, bool debug = false)
        {
            GameObject go = new GameObject("[TCPClinet]" + host + ":" + port);
            EasyTCPClient client = go.AddComponent<EasyTCPClient>();
            client.transform.SetParent(EasySocket.Instance.transform);
            client._host = host;
            client._port = port;
            client._debug = debug;
            client.msgMgr = new S2CMsgManager("[MSG]" + host + ":" + port);
            client._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client._lastHeartBeatTime = System.DateTime.Now;
            return client;
        }

        /// <summary>
        /// 开始连接服务器
        /// </summary>
        /// <param name="autoReconnect">掉线后是否自动重连</param>
        public void StartConnect(bool autoReconnect = false)
        {
            this._autoReconnect = autoReconnect;
            _isOnline = false;
            _receiveLoop = false;
            _heartbeatLoop = false;

            InitNetworkt();
        }


        public void InitNetworkt()
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(_host, _port);
                _isOnline = true;

                //连接成功时间
                EasySocketEvent nse = new EasySocketEvent(EasySocketEvent.CONNECTED, this);
                _socketEventQueue.Enqueue(nse);


                //开启接收线程
                _receiveLoop = true;
                _receiveThread = new Thread(new ThreadStart(OnReceiveDataHanadler));
                _receiveThread.IsBackground = true;
                _receiveThread.Start();


                //开启心跳线程
                _heartbeatLoop = true;
                _heartbeatThread = new Thread(new ThreadStart(OnSendHeartBeatHandler));
                _heartbeatThread.IsBackground = true;
                _heartbeatThread.Start();

                //在线监测线程
                _onlinecheckLoop = true;
                _onlineCheckThread = new Thread(new ThreadStart(OnlineCheckThreadHandler));
                _onlineCheckThread.IsBackground = true;
                _onlineCheckThread.Start();

                //更新最后一次心跳包时间
                _lastHeartBeatTime = System.DateTime.Now;

            }
            catch (Exception ex)
            {
                EasySocketEvent nse = new EasySocketEvent(EasySocketEvent.CONNECT_ERROR, this, ex.Message);
                _socketEventQueue.Enqueue(nse);
                _isOnline = false;
                _onlinecheckLoop = false;
                //更新最后一次心跳包时间
                _lastHeartBeatTime = System.DateTime.Now;
                if (_autoReconnect)
                {
                    LogError("5秒后将再次尝试连接服务器!     原因:" + nse.Info);
                    StopAllCoroutines();
                    //重连服务器
                    StartCoroutine(StartReconnectHandler());
                }
            }
        }

        /// <summary>
        /// 重连服务器协程
        /// </summary>
        /// <returns></returns>
        public IEnumerator StartReconnectHandler()
        {
            yield return new WaitForSeconds(5);
            _lastHeartBeatTime = System.DateTime.Now;
            InitNetworkt();
        }


        //数据接收线程
        private void OnReceiveDataHanadler()
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


                        //更新心跳时间
                        _lastHeartBeatTime = System.DateTime.Now;
                        //转换消息
                        NetMessage msg = new NetMessage(buffers);

                        bytesReceived += msg.Bytes.Length;

                        EasySocketEvent nse = new EasySocketEvent(EasySocketEvent.DATA_RECEIVED, this, msg);
                        _socketEventQueue.Enqueue(nse);
                    }
                    else
                    {
                        _receiveLoop = false;
                        _heartbeatLoop = false;
                        _onlinecheckLoop = false;
                        _isOnline = false;
                        EasySocketEvent nse = new EasySocketEvent(EasySocketEvent.DISCONNECTED, this, "服务器端主动关闭了此链接!");
                        _socketEventQueue.Enqueue(nse);
                    }
                }
                catch (SocketException se)
                {
                    EasySocketEvent nse = new EasySocketEvent(EasySocketEvent.SOCKET_ERROR, this, se.Message);
                    _socketEventQueue.Enqueue(nse);
                    _receiveLoop = false;
                    _heartbeatLoop = false;
                    _onlinecheckLoop = false;
                    _isOnline = false;
                }
                catch (Exception ex)
                {
                    //发送消息事件
                    EasySocketEvent nse = new EasySocketEvent(EasySocketEvent.READ_SOCKET_ERROR, this, ex.Message);
                    _socketEventQueue.Enqueue(nse);
                    _receiveLoop = false;
                    _heartbeatLoop = false;
                    _onlinecheckLoop = false;
                    _isOnline = false;
                }
            }
        }

        /// <summary>
        /// 心跳包发送线程
        /// </summary>
        private void OnSendHeartBeatHandler()
        {
            while (_heartbeatLoop)
            {
                Thread.Sleep(_heartbeartInterval * 1000);
                if (_isOnline)
                {
                    NetMessage msg = new NetMessage(0);
                    this.SendNetMessage(msg);
                }
            }
        }


        void Update()
        {
            //网络事件
            while (_socketEventQueue.Count > 0)
            {
                EasySocketEvent ese = _socketEventQueue.Dequeue();
                OnProcessSocketEventHandler(ese);
            }

            //心跳时差计算
            //更新心跳时间戳
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            long timeStamp = (long)(_lastHeartBeatTime - startTime).TotalSeconds; // 相差秒数
            m_LastHeartBeatTimeStamp = timeStamp.ToString();


        }



        private void OnProcessSocketEventHandler(EasySocketEvent evt)
        {
            if (OnSocketEventHandler != null)
            {
                OnSocketEventHandler(evt);
            }
            switch (evt.Type)
            {
                case EasySocketEvent.CONNECTED:
                    Log("连接服务器成功");
                    break;
                case EasySocketEvent.CONNECT_ERROR:
                    LogError("连接失败");
                    break;
                case EasySocketEvent.READ_SOCKET_ERROR:
                    break;
                case EasySocketEvent.SOCKET_ERROR:
                    break;
                case EasySocketEvent.DISCONNECTED:
                    LogError("你掉线了" + evt.Info);
                    if (_autoReconnect)
                    {
                        LogError("5秒后将再次尝试连接服务器!");
                        //重连服务器
                        StartCoroutine(StartReconnectHandler());
                    }
                    break;
                case EasySocketEvent.DATA_RECEIVED:
                    S2CMsgHandler handler = msgMgr.FindHandlerById(evt.SocketData.MsgId);
                    if (handler != null)
                    {
                        handler.ProcessData(this, evt.SocketData);
                    }
                    Log("收到数据" + evt.SocketData);
                    break;
                case EasySocketEvent.DATA_SENDED:
                    Log("发送数据" + evt.SocketData.MsgId);
                    break;
                case EasySocketEvent.CLIENT_SHUTDOWN:
                    LogError("客户端关闭");
                    break;
            }
        }

        /// <summary>
        /// 断开与服务器的连接
        /// </summary>
        public void ShutDown()
        {
            StopAllCoroutines();
            _receiveLoop = false;
            _heartbeatLoop = false;
            _onlinecheckLoop = false;
            _isOnline = false;
            if (_receiveThread != null)
            {
                _receiveThread.Interrupt();
                _receiveThread.Abort();
                _receiveThread = null;
            }

            if (_heartbeatThread != null)
            {
                _heartbeatThread.Interrupt();
                _heartbeatThread.Abort();
                _heartbeatThread = null;
            }

            if (_onlineCheckThread != null)
            {
                _onlineCheckThread.Interrupt();
                _onlineCheckThread.Abort();
                _onlineCheckThread = null;
            }
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }


            //关闭连接
            EasySocketEvent ese = new EasySocketEvent(EasySocketEvent.CLIENT_SHUTDOWN, this);
            _socketEventQueue.Enqueue(ese);


        }


        private void OnlineCheckThreadHandler()
        {
            while (_onlinecheckLoop)
            {
                //监测自己是否已经掉线
                TimeSpan sp = System.DateTime.Now - this._lastHeartBeatTime;
                if (sp.Seconds > 15)
                {
                    _onlinecheckLoop = false;
                    _receiveLoop = false;
                    _heartbeatLoop = false;
                    _isOnline = false;
                    EasySocketEvent ese = new EasySocketEvent(EasySocketEvent.DISCONNECTED, this, "因长时间未收到心跳包而断开网络连接");
                    _socketEventQueue.Enqueue(ese);
                }
            }
        }




        void OnDestroy()
        {
            ShutDown();
        }


        public void SendNetMessage(NetMessage msg)
        {
            if (_isOnline)
            {
                try
                {
                    _socket.Send(msg.Bytes);
                    EasySocketEvent nse = new EasySocketEvent(EasySocketEvent.DATA_SENDED, this, msg);
                    _socketEventQueue.Enqueue(nse);
                    bytesSended += msg.Bytes.Length;
                }
                catch (SocketException se)
                {
                    _onlinecheckLoop = false;
                    _receiveLoop = false;
                    _heartbeatLoop = false;
                    _isOnline = false;
                    EasySocketEvent nse = new EasySocketEvent(EasySocketEvent.DISCONNECTED, this, se.Message);
                    _socketEventQueue.Enqueue(nse);
                }
            }
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