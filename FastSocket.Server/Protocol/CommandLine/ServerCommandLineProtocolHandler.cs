using System;
using System.Collections.Generic;
using System.Text;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Messaging;
using Sodao.FastSocket.SocketBase.Protocol;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;
using Sodao.FastSocket.SocketBase.Protocol.CommandLine;

namespace Sodao.FastSocket.Server.Protocol.CommandLine {

    /// <summary>
    /// 
    /// </summary>
    public class ServerCommandLineProtocolHandler : CommandLineProtocolHandler<ISendMessageInfo<CommandLineMessage>>,
        IServerProtocolHandler<ISendMessageInfo<CommandLineMessage>, CommandLineMessage> {

        /// <summary>
        /// 
        /// </summary>
        public ServerCommandLineProtocolHandler() {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override ISendMessageInfo<CommandLineMessage> CreateMessageInfo(CommandLineMessage message) => new SendMessageInfo<CommandLineMessage>(message, false);



        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public override void UpdateCryptKey(object key) {

        }
    }
}