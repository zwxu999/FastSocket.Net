using Sodao.FastSocket.SocketBase.Messaging;
using Sodao.FastSocket.SocketBase.Protocol;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;
using Sodao.FastSocket.SocketBase.Protocol.Thrift;

namespace Sodao.FastSocket.Server.Protocol.Thrift {
    /// <summary>
    /// 
    /// </summary>
    public class ServerThriftProtocolHandler : ThriftProtocolHandler<ISendMessageInfo<ThriftMessage>, ThriftMessage>
        , IServerProtocolHandler<ISendMessageInfo<ThriftMessage>, ThriftMessage> {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override ISendMessageInfo<ThriftMessage> CreateMessageInfo(ThriftMessage message) {
            return new SyncMessageInfo<ThriftMessage>(message, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected override ThriftMessage CreateThriftMessage(byte[] payload) {
            return new ThriftMessage(payload);
        }
    }
}