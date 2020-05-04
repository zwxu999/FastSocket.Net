using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Messaging;
using Sodao.FastSocket.SocketBase.Protocol;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.Server.Protocol.CommandLine {

    /// <summary>
    /// CommandProtocolHandlerFactory
    /// </summary>
    public class ServerCommandLineProtocolHandlerFactory : IServerProtocolHandlerFactory<ISendMessageInfo<CommandLineMessage>, CommandLineMessage> {

        /// <summary>
        /// CreateProtocolHandler
        /// </summary>
        /// <returns></returns>
        public IProtocolHandler<ISendMessageInfo<CommandLineMessage>, CommandLineMessage> CreateProtocolHandler() {
            return new ServerCommandLineProtocolHandler();
        }
    }
}