/**************************************
          EasySocketEvent

    
    author:felixwee
	email:felixwee@163.com
	blog: www.felixwee.com
    desc:
	
	
****************************************/

using com.felixwee.easysocket.msg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.felixwee.easysocket.events
{
    public class EasySocketEvent
    {
        //客户端连接到服务器
        public const string NEW_CONNECTION = "USER_CONNECTED";
        //服务器启动成功
        public const string SERVER_START_SUCCESS = "SERVER_STARTED";
        //服务器关闭
        public const string SERVER_SHUTDOWN = "SERVER_SHUTDOWN";

        //客户端关闭
        public const string CLIENT_SHUTDOWN = "CLIENT_SHUTDOWN";

        //服务器启动失败
        public const string SERVER_START_ERROR = "SERVER_START_ERROR";
        //连接成功
        public const string CONNECTED = "CONNECTED";
        //连接出错
        public const string CONNECT_ERROR = "CONNECT_ERROR";
        //连接断开
        public const string DISCONNECTED = "DISCONNECTED";

        //发送数据
        public const string DATA_SENDED = "DATA_SENDED";
        //收到服务器数据
        public const string DATA_RECEIVED = "DATA_RECEIVED";
        //读取Socket数据出错
        public const string READ_SOCKET_ERROR = "READ_SOCKET_ERROR";
        //Socket错误
        public const string SOCKET_ERROR = "SOCKET_ERROR";

        //相关变量
        public string Type;
        public EasyTCPToken UserToken;
        public EasyTCPClient Client;
        public EasyTCPServer Server;
        public NetMessage SocketData;
        public string Info;
        public object[] Parameters;
        ///服务器端
        public EasySocketEvent(string type, EasyTCPServer server, string msg = "")
        {
            this.Type = type;
            this.Server = server;
            this.Info = msg;
        }

        //客户端
        public EasySocketEvent(string type, EasyTCPClient client, NetMessage msg)
        {
            this.Type = type;
            this.Client = client;
            this.SocketData = msg;
        }

        public EasySocketEvent(string type, EasyTCPClient client, string msg = "")
        {
            this.Type = type;
            this.Client = client;
            this.Info = msg;
        }

        public EasySocketEvent(string type, EasyTCPToken token, NetMessage msg)
        {
            this.Type = type;
            this.UserToken = token;
            this.SocketData = msg;
        }


        public EasySocketEvent(string type, EasyTCPToken token, string msg = "")
        {
            this.Type = type;
            this.UserToken = token;
            this.Info = msg;
        }


        public EasySocketEvent(string type, EasyTCPToken token, params object[] parameters)
        {
            this.Type = type;
            this.UserToken = token;
            this.Parameters = parameters;
        }

    }
}
