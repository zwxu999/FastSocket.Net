using System;
using Sodao.FastSocket.Client.Messaging;
using Sodao.FastSocket.Client.Protocol;
using Sodao.FastSocket.Client.Protocol.Handlers;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.Client {
    /// <summary>
    /// thrift client
    /// </summary>
    public class ThriftClient : SocketClient<ISyncMessageInfo<ClientThriftMessage>, ClientThriftMessage> {

        /// <summary>
        /// new
        /// </summary>
        public ThriftClient() : this(new SyncClientThriftProtocolHandlerFactory()) {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protocolFactory"></param>
        public ThriftClient(ISyncClientProtocolHandlerFactory<ISyncClientProtocolHandler<ISyncMessageInfo<ClientThriftMessage>, ClientThriftMessage>, ISyncMessageInfo<ClientThriftMessage>, ClientThriftMessage> protocolFactory) 
            : this(8192, 8192, protocolFactory) {

        }
        /// <summary>
        /// new
        /// </summary>
        /// <param name="socketBufferSize"></param>
        /// <param name="messageBufferSize"></param>
        /// <param name="protocolFactory"></param>
        public ThriftClient(int socketBufferSize, int messageBufferSize, ISyncClientProtocolHandlerFactory<ISyncClientProtocolHandler<ISyncMessageInfo<ClientThriftMessage>, ClientThriftMessage>, ISyncMessageInfo<ClientThriftMessage>, ClientThriftMessage> protocolFactory)
            : this(socketBufferSize, messageBufferSize, 3000, 3000, protocolFactory) {
        }

        /// <summary>
        /// new
        /// </summary>
        /// <param name="socketBufferSize"></param>
        /// <param name="messageBufferSize"></param>
        /// <param name="millisecondsSendTimeout"></param>
        /// <param name="millisecondsReceiveTimeout"></param>
        /// <param name="protocolFactory"></param>
        public ThriftClient(int socketBufferSize,
            int messageBufferSize,
            int millisecondsSendTimeout,
            int millisecondsReceiveTimeout,
            ISyncClientProtocolHandlerFactory<ISyncClientProtocolHandler<ISyncMessageInfo<ClientThriftMessage>, ClientThriftMessage>, ISyncMessageInfo<ClientThriftMessage>, ClientThriftMessage> protocolFactory)
            : base(protocolFactory,
            socketBufferSize,
            messageBufferSize,
            millisecondsSendTimeout,
            millisecondsReceiveTimeout) {
        }
    }
}