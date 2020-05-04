using System.Threading.Tasks;

namespace Sodao.FastSocket.SocketBase.Protocol.Abstractions {
    /// <summary>
    /// 
    /// </summary>
    public interface IRecvivedMessageInfo<TMessage> {

        /// <summary>
        /// 
        /// </summary>
        bool IsSync { get; }

        /// <summary>
        /// 
        /// </summary>
        int SeqID { get; }

        /// <summary>
        /// 
        /// </summary>
        TMessage Message { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="connection"></param>
        Task<bool> Reply(TMessage message, IConnection connection);
    }
}