using Sodao.FastSocket.SocketBase.Messaging;
using Sodao.FastSocket.SocketBase.Protocol;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.Server.Protocol.Thrift {

    /// <summary>
    /// CommandProtocolHandlerFactory
    /// </summary>
    public class ServerThriftProtocolHandlerFactory : IServerProtocolHandlerFactory<ISendMessageInfo<ThriftMessage>, ThriftMessage> {

        /// <summary>
        /// CreateProtocolHandler
        /// </summary>
        /// <returns></returns>
        public IProtocolHandler<ISendMessageInfo<ThriftMessage>, ThriftMessage> CreateProtocolHandler() {
            return new ServerThriftProtocolHandler();
        }
    }
}