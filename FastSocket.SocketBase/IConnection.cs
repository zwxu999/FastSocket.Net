using System;
using System.Net;
using System.Threading.Tasks;
using Sodao.FastSocket.SocketBase.Protocol;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.SocketBase {
    /// <summary>
    /// a connection interface.
    /// </summary>
    public interface IConnection {
        /// <summary>
        /// disconnected event
        /// </summary>
        event DisconnectedHandler Disconnected;

        /// <summary>
        /// return the connection is active.
        /// </summary>
        bool Active { get; }
        /// <summary>
        /// get the connection latest active time.
        /// </summary>
        DateTime LatestActiveTime { get; }
        /// <summary>
        /// get the connection id.
        /// </summary>
        long ConnectionID { get; }
        /// <summary>
        /// 获取本地IP地址
        /// </summary>
        IPEndPoint LocalEndPoint { get; }
        /// <summary>
        /// 获取远程IP地址
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }
        /// <summary>
        /// 获取或设置与用户数据
        /// </summary>
        object UserData { get; set; }

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="message"></param>
        Task<bool> BeginSend(object message);
        /// <summary>
        /// 异步接收数据
        /// </summary>
        void BeginReceive();
        /// <summary>
        /// 异步断开连接
        /// </summary>
        /// <param name="ex"></param>
        void BeginDisconnect(Exception ex = null);

        /// <summary>
        /// UpdateCryptKey
        /// </summary>
        /// <param name="key"></param>
        void UpdateCryptKey(object key);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TMessageInfo"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public interface IConnection<TMessageInfo, TMessage> : IConnection where TMessageInfo : ISendMessageInfo<TMessage> 
        {

        /// <summary>
        /// SetProtoHandler
        /// </summary>
        /// <param name="protocolHandler"></param>
        void SetProtoHandler(IProtocolHandler<TMessageInfo, TMessage> protocolHandler);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageInfo"></param>
        /// <returns></returns>
        void BeginSendMessage(TMessageInfo messageInfo);
    }
}