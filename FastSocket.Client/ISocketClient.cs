using System;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Messaging;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.Client {
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TMessageInfo"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public interface ISocketClient<TMessageInfo, TMessage> where TMessageInfo : ISendMessageInfo<TMessage> {

        /// <summary>
        /// 
        /// </summary>
        int MillisecondsSendTimeout { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageInfo"></param>
        void OnPendingSendTimeout(TMessageInfo messageInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        void TryGetConnection2SendNext(Func<TMessageInfo> p);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TMessageInfo"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public interface ISyncSocketClient<TMessageInfo, TMessage> : ISocketClient<TMessageInfo, TMessage>
        where TMessageInfo : ISyncMessageInfo<TMessage> {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageInfo"></param>
        void OnReceiveTimeout(TMessageInfo messageInfo);
    }
}