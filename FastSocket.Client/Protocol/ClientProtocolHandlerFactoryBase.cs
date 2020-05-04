using System;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Messaging;
using Sodao.FastSocket.SocketBase.Protocol;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.Client.Protocol {
    /// <summary>
    /// ClientThriftProtocolHandlerFactory
    /// </summary>
    public abstract class SyncClientProtocolHandlerFactoryBase<TMessageInfo, TMessage>
        : ISyncClientProtocolHandlerFactory<ISyncClientProtocolHandler<TMessageInfo,TMessage>, TMessageInfo, TMessage>
        where TMessageInfo : ISyncMessageInfo<TMessage> {

        /// <summary>
        /// CTOR
        /// </summary>
        public SyncClientProtocolHandlerFactoryBase() {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public abstract ISyncClientProtocolHandler<TMessageInfo, TMessage> CreateProtocolHandler(IConnection connection);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="allowRetry"></param>
        /// <returns></returns>
        public abstract TMessageInfo CreateClientAsyncMessageInfo(TMessage message, bool allowRetry);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="millisecondsReceiveTimeout"></param>
        /// <param name="onException"></param>
        /// <param name="onResult"></param>
        /// <param name="allowRetry"></param>
        /// <returns></returns>
        public abstract TMessageInfo CreateClientSyncMessageInfo(TMessage message, int millisecondsReceiveTimeout, Action<Exception> onException, Action<TMessage> onResult, bool allowRetry = true);
    }
}