using System.Collections.Generic;

namespace Sodao.FastSocket.SocketBase.Messaging {
    /// <summary>
    /// 
    /// </summary>
    public class CommandLineMessageTypeConverter : TextMessageTypeConverter<CommandLineMessage> {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override string GetText(CommandLineMessage message) => message.FullCommandLine;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected override CommandLineMessage CreateMessage(string text) => new CommandLineMessage(new Queue<string>(text.Split(' ')));
    }
}