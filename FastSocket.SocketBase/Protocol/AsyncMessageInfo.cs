using System;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.SocketBase.Protocol {
    /// <summary>
    /// 异步发送消息跟踪信息(无需跟踪接收消息状态)
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class AsyncMessageInfo<TMessage> : SendMessageInfo<TMessage>, IAsyncMessageInfo<TMessage> {
        #region Members
        /// <summary>
        /// default is don't allow retry send.
        /// </summary>
        public bool AllowRetry { get; }

        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isSync"></param>
        /// <param name="allowRetry"></param>
        /// <exception cref="ArgumentNullException">onException is null</exception>
        /// <exception cref="ArgumentNullException">onResult is null</exception>
        public AsyncMessageInfo(TMessage message, bool isSync, bool allowRetry)
            : base(message, isSync) {
            this.AllowRetry = allowRetry;
        }

        #endregion
    }
}