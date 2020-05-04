using System;

namespace Sodao.FastSocket.SocketBase.Protocol.Abstractions {
    /// <summary>
    /// upd protocol
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IUdpProtocolHandler<TMessage> {
        /// <summary>
        /// parse protocol message
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool TryParse(ArraySegment<byte> buffer, out TMessage message);
    }
}