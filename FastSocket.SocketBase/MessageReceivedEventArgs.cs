using System;

namespace Sodao.FastSocket.SocketBase {

    /// <summary>
    /// message received eventArgs
    /// </summary>
    public sealed class MessageReceivedEventArgs<TMessageInfo> {

        /// <summary>
        /// 
        /// </summary>
        public TMessageInfo MessageInfo { get; }

        /// <summary>
        /// new
        /// </summary>
        /// <param name="messageInfo"></param>
        /// <exception cref="ArgumentNullException">processCallback is null</exception>
        public MessageReceivedEventArgs(TMessageInfo messageInfo) {
            this.MessageInfo = messageInfo;
        }
    }
}