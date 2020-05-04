using System;
using Sodao.FastSocket.Client.Messaging;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Messaging;
using Sodao.FastSocket.SocketBase.Protocol;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;
using Sodao.FastSocket.SocketBase.Protocol.Thrift;

namespace Sodao.FastSocket.Client.Protocol.Handlers {

    /// <summary>
    /// thrift protocol
    /// [message len,4][version,4][cmd len,4][cmd][seqID,4][data...,N]
    /// </summary>
    public sealed class SyncClientThriftProtocolHandler : ThriftProtocolHandler<ISyncMessageInfo<ClientThriftMessage>, ClientThriftMessage>, ISyncClientProtocolHandler<ISyncMessageInfo<ClientThriftMessage>, ClientThriftMessage> {

        /// <summary>
        /// CTOR
        /// </summary>
        public SyncClientThriftProtocolHandler() {

        }

        /// <summary>
        /// CreateMessageInfo
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override ISyncMessageInfo<ClientThriftMessage> CreateMessageInfo(ClientThriftMessage message) => new SyncMessageInfo<ClientThriftMessage>(message, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected override ClientThriftMessage CreateThriftMessage(byte[] payload) {
            return new ClientThriftMessage(payload);
        }
    }
}