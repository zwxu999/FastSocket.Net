using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Sodao.FastSocket.Client.Messaging;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Messaging;
using Sodao.FastSocket.SocketBase.Protocol;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;
using Sodao.FastSocket.SocketBase.Protocol.CommandLine;

namespace Sodao.FastSocket.Client.Protocol.Handlers {
    /// <summary>
    /// 命令行协议
    /// </summary>
    public sealed class SyncClientCommandLineProtocolHandler : CommandLineProtocolHandler<ISyncMessageInfo<CommandLineMessage>>,
        ISyncClientProtocolHandler<ISyncMessageInfo<CommandLineMessage>, CommandLineMessage> {

        /// <summary>
        /// CTOR
        /// </summary>
        public SyncClientCommandLineProtocolHandler() {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public override void UpdateCryptKey(object key) {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override ISyncMessageInfo<CommandLineMessage> CreateMessageInfo(CommandLineMessage message) {
            return new SyncMessageInfo<CommandLineMessage>(message, true);
        }
    }
}