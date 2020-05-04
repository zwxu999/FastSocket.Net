using System;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.SocketBase.Protocol {
    /// <summary>
    /// 同步发送消息跟踪信息(需要跟踪接收消息状态)
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class SyncMessageInfo<TMessage> : AsyncMessageInfo<TMessage>, ISyncMessageInfo<TMessage> {

        /// <summary>
        /// 异常回调
        /// </summary>
        private readonly Action<Exception> _onException = null;
        /// <summary>
        /// 结果回调
        /// </summary>
        private readonly Action<TMessage> _onResult = null;

        /// <summary>
        /// get or set receive time out
        /// </summary>
        public int MillisecondsReceiveTimeout { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="millisecondsReceiveTimeout"></param>
        /// <param name="onException"></param>
        /// <param name="onResult"></param>
        /// <param name="allowRetry"></param>
        public SyncMessageInfo(TMessage message, int millisecondsReceiveTimeout = 3000, Action<Exception> onException = null, Action<TMessage> onResult = null, bool allowRetry = true) : base(message, onResult != null, allowRetry) {
            if (IsSync) {
                this._onException = onException ?? throw new ArgumentNullException("onException");
                this._onResult = onResult ?? throw new ArgumentNullException("onResult");
                this.MillisecondsReceiveTimeout = millisecondsReceiveTimeout;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="allowRetry"></param>
        public SyncMessageInfo(TMessage message, bool allowRetry) : base(message, false, allowRetry) {

        }

        #region Public Methods
        /// <summary>
        /// set exception
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public bool SetReceiveException(Exception ex) {
            this._onException(ex);
            return true;
        }
        /// <summary>
        /// set result
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SetRecviceResult(TMessage message) {
            this._onResult(message);
            return true;
        }
        #endregion
    }
}