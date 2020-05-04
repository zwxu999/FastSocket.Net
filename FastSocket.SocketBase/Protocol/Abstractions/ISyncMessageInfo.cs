using System;
using Sodao.FastSocket.SocketBase.Messaging;

namespace Sodao.FastSocket.SocketBase.Protocol.Abstractions {
    /// <summary>
    /// 同步发送消息跟踪信息(需要跟踪接收消息状态)
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface ISyncMessageInfo<TMessage> : IAsyncMessageInfo<TMessage> {

        /// <summary>
        /// SetResult
        /// </summary>
        /// <param name="message"></param>
        bool SetRecviceResult(TMessage message);

        /// <summary>
        /// SetReceiveException
        /// </summary>
        /// <param name="ex"></param>
        bool SetReceiveException(Exception ex);

        /// <summary>
        /// MillisecondsReceiveTimeout
        /// </summary>
        int MillisecondsReceiveTimeout { get; }
    }
}