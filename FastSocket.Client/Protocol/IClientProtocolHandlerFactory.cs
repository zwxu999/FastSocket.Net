using System;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Messaging;
using Sodao.FastSocket.SocketBase.Protocol;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.Client.Protocol {
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TProtocolHandler"></typeparam>
    /// <typeparam name="TMessageInfo"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public interface IClientProtocolHandlerFactory<TProtocolHandler, TMessageInfo, TMessage>
        where TProtocolHandler : IClientProtocolHandler<TMessageInfo, TMessage>
        where TMessageInfo : IAsyncMessageInfo<TMessage> {

        /// <summary>
        /// CreateProtocolHandler
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        TProtocolHandler CreateProtocolHandler(IConnection connection);

        /// <summary>
        /// 创建异步发送跟踪包信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="allowRetry"></param>
        /// <returns></returns>
        TMessageInfo CreateClientAsyncMessageInfo(TMessage message, bool allowRetry);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TProtocolHandler"></typeparam>
    /// <typeparam name="TMessageInfo"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public interface ISyncClientProtocolHandlerFactory<TProtocolHandler, TMessageInfo, TMessage> : IClientProtocolHandlerFactory<TProtocolHandler, TMessageInfo, TMessage>
        where TProtocolHandler : ISyncClientProtocolHandler<TMessageInfo, TMessage>
        where TMessageInfo : ISyncMessageInfo<TMessage> {

        /// <summary>
        /// 创建同步发送跟踪包信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="millisecondsReceiveTimeout"></param>
        /// <param name="onException"></param>
        /// <param name="onResult"></param>
        /// <param name="allowRetry"></param>
        /// <returns></returns>
        TMessageInfo CreateClientSyncMessageInfo(TMessage message, int millisecondsReceiveTimeout, Action<Exception> onException, Action<TMessage> onResult, bool allowRetry = true);
    }
}