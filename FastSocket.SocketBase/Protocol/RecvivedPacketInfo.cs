using System.Threading.Tasks;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.SocketBase.Protocol {

    /// <summary>
    /// 
    /// </summary>
    public class RecvivedMessageInfo<TMessage> : IRecvivedMessageInfo<TMessage> {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isSync"></param>
        /// <param name="seqID"></param>
        public RecvivedMessageInfo(TMessage message, bool isSync, int seqID = 0) {
            this.Message = message;
            this.IsSync = isSync;
            this.SeqID = seqID;
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsSync { get; }

        /// <summary>
        /// 
        /// </summary>
        public int SeqID { get; }

        /// <summary>
        /// 
        /// </summary>
        public TMessage Message { get; }

        /// <summary>
        /// ResponseSyncMessage
        /// </summary>
        /// <param name="message"></param>
        /// <param name="connection"></param>
        public Task<bool> Reply(TMessage message, IConnection connection) {
            return connection.BeginSend(new SendMessageInfo<TMessage>(message, IsSync, SeqID));
        }
    }
}