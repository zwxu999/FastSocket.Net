using System;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.Server {
    /// <summary>
    /// abstract socket service interface.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class AbsSocketService<TMessage> : ISocketService<TMessage> {
        /// <summary>
        /// 当建立socket连接时，会调用此方法
        /// </summary>
        /// <param name="connection"></param>
        public virtual void OnConnected(SocketBase.IConnection connection) {
        }
        /// <summary>
        /// 发送回调
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="message"></param>
        /// <param name="isSuccess"></param>
        public virtual void OnSendCallback(SocketBase.IConnection connection, TMessage message, bool isSuccess) {
        }
        /// <summary>
        /// 当接收到客户端新消息时，会调用此方法.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="recvivedMessageInfo"></param>
        public virtual void OnReceived(SocketBase.IConnection connection, IRecvivedMessageInfo<TMessage> recvivedMessageInfo) {

        }
        /// <summary>
        /// 当socket连接断开时，会调用此方法
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        public virtual void OnDisconnected(SocketBase.IConnection connection, Exception ex) {
        }
        /// <summary>
        /// 当发生异常时，会调用此方法
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        public virtual void OnException(SocketBase.IConnection connection, Exception ex) {
        }
    }
}