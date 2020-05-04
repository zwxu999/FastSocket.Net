using System;
using Sodao.FastSocket.Client.Messaging;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Messaging;
using Sodao.FastSocket.SocketBase.Protocol;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.Client.Protocol.Handlers {

    /// <summary>
    /// ClientCommandLineProtocolHandlerFactory
    /// </summary>
    public class SyncClientCommandLineProtocolHandlerFactory : SyncClientProtocolHandlerFactoryBase<ISyncMessageInfo<CommandLineMessage>, CommandLineMessage> {

        /// <summary>
        /// CTOR
        /// </summary>
        public SyncClientCommandLineProtocolHandlerFactory() : base() {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="allowRetry"></param>
        /// <returns></returns>
        public override ISyncMessageInfo<CommandLineMessage> CreateClientAsyncMessageInfo(CommandLineMessage message, bool allowRetry) {
            return new SyncMessageInfo<CommandLineMessage>(message, allowRetry);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="millisecondsReceiveTimeout"></param>
        /// <param name="onException"></param>
        /// <param name="allowRetry"></param>
        /// <param name="onResult"></param>
        /// <returns></returns>
        public override ISyncMessageInfo<CommandLineMessage> CreateClientSyncMessageInfo(CommandLineMessage message, int millisecondsReceiveTimeout, Action<Exception> onException, Action<CommandLineMessage> onResult, bool allowRetry = true) {
            return new SyncMessageInfo<CommandLineMessage>(message, millisecondsReceiveTimeout, onException, onResult);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public override ISyncClientProtocolHandler<ISyncMessageInfo<CommandLineMessage>, CommandLineMessage> CreateProtocolHandler(IConnection connection) {
            return new SyncClientCommandLineProtocolHandler();
        }
    }
}