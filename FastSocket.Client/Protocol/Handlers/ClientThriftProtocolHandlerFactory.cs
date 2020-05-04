using System;
using Sodao.FastSocket.Client.Messaging;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Protocol;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.Client.Protocol.Handlers {

    /// <summary>
    /// 
    /// </summary>
    public class SyncClientThriftProtocolHandlerFactory : SyncClientProtocolHandlerFactoryBase<ISyncMessageInfo<ClientThriftMessage>, ClientThriftMessage> {

        /// <summary>
        /// CTOR
        /// </summary>
        public SyncClientThriftProtocolHandlerFactory() : base() {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="allowRetry"></param>
        /// <returns></returns>
        public override ISyncMessageInfo<ClientThriftMessage> CreateClientAsyncMessageInfo(ClientThriftMessage message, bool allowRetry) {
            return new SyncMessageInfo<ClientThriftMessage>(message, allowRetry);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="millisecondsReceiveTimeout"></param>
        /// <param name="onException"></param>
        /// <param name="onResult"></param>
        /// <param name="allowRetry"></param>
        /// <returns></returns>
        public override ISyncMessageInfo<ClientThriftMessage> CreateClientSyncMessageInfo(ClientThriftMessage message, int millisecondsReceiveTimeout, Action<Exception> onException, Action<ClientThriftMessage> onResult, bool allowRetry = true) {
            return new SyncMessageInfo<ClientThriftMessage>(message, millisecondsReceiveTimeout, onException, onResult, allowRetry);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override ISyncClientProtocolHandler<ISyncMessageInfo<ClientThriftMessage>, ClientThriftMessage> CreateProtocolHandler(IConnection connection) {
            return new SyncClientThriftProtocolHandler();
        }
    }
}