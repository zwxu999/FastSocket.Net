namespace Sodao.FastSocket.SocketBase.Protocol.Abstractions {
    /// <summary>
    /// 异步发送消息跟踪信息(无需跟踪接收消息状态)
    /// </summary>
    public interface IAsyncMessageInfo<TMessage> : ISendMessageInfo<TMessage> {

        /// <summary>
        /// AllowRetry
        /// </summary>
        bool AllowRetry { get; }
    }
}