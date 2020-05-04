using System;
using System.Net;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Protocol;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.Server {
    /// <summary>
    /// socket listener
    /// </summary>
    public interface ISocketListener<TMessageInfo, TMessage> where TMessageInfo : ISendMessageInfo<TMessage> 
    {
        /// <summary>
        /// socket accepted event
        /// </summary>
        event Action<ISocketListener<TMessageInfo, TMessage>, SocketBase.IConnection<TMessageInfo, TMessage>> Accepted;

        /// <summary>
        /// get endpoint
        /// </summary>
        EndPoint EndPoint { get; }
        /// <summary>
        /// start listen
        /// </summary>
        void Start();
        /// <summary>
        /// stop listen
        /// </summary>
        void Stop();
    }
}