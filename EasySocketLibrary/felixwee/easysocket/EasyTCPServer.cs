using com.felixwee.easysocket.events;
using com.felixwee.easysocket.msg;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace com.felixwee.easysocket
{
    public delegate void SocketEventHandler(EasySocketEvent evt);
    public class EasyTCPServer : MonoBehaviour
    {


        #region Socket相关
        private Socket _socket;
        private string _host;
        private int _port;
        //发送字节数统计
        public long _bytesSended = 0;
        //接收自己数统计
        public long _bytesReceived = 0;

        //客户端数量
        public int UserCount
        {
            get
            {
                return _tokenDic.Count;
            }
        }
        #endregion

        #region 客户端连接监听线程
        private bool _listenLoop = true;
        private Thread _listenThread = null;
        #endregion


        #region 其他
        private bool _debug = false;
        //临时队列，用于存储新Socket
        private Queue<Socket> _tmpSocketQueue = new Queue<Socket>();
        //客户端字典,用于记录和快速查找
        public Dictionary<string, EasyTCPToken> _tokenDic = new Dictionary<string, EasyTCPToken>();
        //网络事件队列
        public Queue<EasySocketEvent> _socketEventQueue = new Queue<EasySocketEvent>();
        //对外状态事件
        public event SocketEventHandler OnServerEventHandler = null;

        private C2SMsgManager _msgMgr;

        public C2SMsgManager Msg
        {
            get
            {
                return _msgMgr;
            }
        }
        #endregion


        /// <summary>
        /// 新建TCP服务器
        /// </summary>
        /// <param name="address">ip地址</param>
        /// <param name="port">端口</param>
        /// <returns></returns>
        public static EasyTCPServer CreateServer(string address = "127.0.0.1", int port = 9339, bool debug = false)
        {
            GameObject go = new GameObject("【TCPServer】" + address + ":" + port);
            EasyTCPServer server = go.AddComponent<EasyTCPServer>();
            server._host = address;
            server._port = port;
            server._debug = debug;
            server._tmpSocketQueue = new Queue<Socket>();
            server._msgMgr = new C2SMsgManager("【C2S】" + address + ":" + port);
            server.transform.SetParent(EasySocket.Instance.transform);
            return server;
        }

        /// <summary>
        /// 开启服务器
        /// </summary>
        public bool StartServer()
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Bind(new IPEndPoint(IPAddress.Parse(_host), _port));
                _socket.Listen(0);

                _listenLoop = true;
                _listenThread = new Thread(new ThreadStart(OnListenConnectHandler));
                _listenThread.IsBackground = true;
                _listenThread.Start();


                //入列 服务器启动成功
                EasySocketEvent ese = new EasySocketEvent(EasySocketEvent.SERVER_START_SUCCESS, this);
                _socketEventQueue.Enqueue(ese);
                return true;
            }
            catch (Exception ex)
            {
                EasySocketEvent ese = new EasySocketEvent(EasySocketEvent.SERVER_START_ERROR, this, ex.Message);
                _socketEventQueue.Enqueue(ese);
                return false;
            }
        }

        /// <summary>
        /// 处理新用户连接线程
        /// </summary>
        private void OnListenConnectHandler()
        {
            while (_listenLoop)
            {
                Socket clientsk = _socket.Accept();
                _tmpSocketQueue.Enqueue(clientsk);
            }
        }

        void Update()
        {
            //处理新连接对象
            while (_tmpSocketQueue.Count > 0)
            {
                //取出新连接Socket,穿件客户端
                Socket sk = _tmpSocketQueue.Dequeue();

                //开始构建EasyTCPToken
                EasyTCPToken token = EasyTCPToken.CreateTCPToken(sk, this);
                token.OnSocketEventHandler += OnTCPTokenEventHandler;
                token.transform.SetParent(this.transform);

                //同步用户列表
                _tokenDic.Add(token.Ipe.ToString(), token);

                //客户端连接事件 
                EasySocketEvent se = new EasySocketEvent(EasySocketEvent.NEW_CONNECTION, token);
                _socketEventQueue.Enqueue(se);
            }

            //**********跳线用户检测***************/
            List<EasyTCPToken> offlinetokens = new List<EasyTCPToken>();
            //检测是否有用户掉线
            foreach (var token in _tokenDic.Values)
            {
                TimeSpan sp = System.DateTime.Now - token.LastHeartTime;
                if (sp.Seconds > 10)
                {
                    offlinetokens.Add(token);
                }
            }

            foreach (var token in offlinetokens)
            {
                EasySocketEvent se = new EasySocketEvent(EasySocketEvent.DISCONNECTED, token);
                _socketEventQueue.Enqueue(se);
                _tokenDic.Remove(token.Ipe.ToString());
                token.Close();
             }
            offlinetokens.Clear();
            //---------------------------------/


            //服务器端变化事件
            while (_socketEventQueue.Count > 0)
            {
                EasySocketEvent se = _socketEventQueue.Dequeue();
                OnSocketEventHandler(se);
            }
        }

        /// <summary>
        /// 客户端事件触发
        /// </summary>
        /// <param name="evt"></param>
        private void OnTCPTokenEventHandler(EasySocketEvent evt)
        {
            //将客户端事件入列
            _socketEventQueue.Enqueue(evt);
        }


        private void OnSocketEventHandler(EasySocketEvent evt)
        {
            if (OnServerEventHandler != null)
            {
                OnServerEventHandler(evt);
            }
            switch (evt.Type)
            {
                case EasySocketEvent.NEW_CONNECTION: //客户端连接服务器
                    Log("客户端" + evt.UserToken.Ipe.ToString()+ "连接到了服务器");
                    break;
                case EasySocketEvent.DISCONNECTED://客户端掉线
                    LogError("客户端" + evt.UserToken.Ipe.ToString() + "掉线了" + evt.Info);
                    break;
                case EasySocketEvent.DATA_RECEIVED: //收到客户端数据
                    Log("收到客户端" + evt.UserToken.Ipe.ToString() + "的消息" + evt.SocketData);
                    C2SMsgHandler handler=_msgMgr.FindHandlerById(evt.SocketData.MsgId);
                    if (handler != null)
                    {
                        handler.ProcessData(evt.UserToken, evt.SocketData);
                    }
                    break;
                case EasySocketEvent.READ_SOCKET_ERROR://读取数据出错
                    LogError("读取数据出错");
                    break;
                case EasySocketEvent.SOCKET_ERROR: //Socket异常
                    LogError("Socket异常");
                    break;
                case EasySocketEvent.SERVER_START_SUCCESS://服务器启动成功
                    Log("服务器"+ _host + ":"+_port+"启动成功");
                    break;
                case EasySocketEvent.SERVER_START_ERROR: //服务器启动失败
                    LogError("服务器启动失败 " + evt.Info);
                    break;
                case EasySocketEvent.SERVER_SHUTDOWN:
                    LogError("服务器ShutDown了");
                    break;
            }
        }

        public void SendNetMessageTo(EasyTCPToken token, NetMessage msg)
        {
            if (token._online)
            {
                token.SendNetMessage(msg);
                _bytesSended += msg.Bytes.Length;
            }
        }

        public void BroadcastNetMessage(msg.NetMessage msg)
        {
            foreach (var token in _tokenDic.Values)
            {
                token.SendNetMessage(msg);
                _bytesSended += msg.Bytes.Length;
            }
        }




        //定制服务器
        public void StopServer()
        {
            //停止连接线程
            _listenLoop = false;

            if (_listenThread != null)
            {
                _listenThread.Interrupt();
                _listenThread.Abort();
                _listenThread = null;
            }

            //终端所有与客户端的连接
            foreach (EasyTCPToken token in _tokenDic.Values)
            {
                EasySocketEvent se1 = new EasySocketEvent(EasySocketEvent.DISCONNECTED, token);
                _socketEventQueue.Enqueue(se1);
                token.Close();
            }
            _tokenDic.Clear();

            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }

            EasySocketEvent se = new EasySocketEvent(EasySocketEvent.SERVER_SHUTDOWN, this);
            _socketEventQueue.Enqueue(se);
        }


        void OnDestroy()
        {
            Log("服务器关闭了");
            StopServer();
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