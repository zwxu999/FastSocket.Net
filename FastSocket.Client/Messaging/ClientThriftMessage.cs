
using Sodao.FastSocket.SocketBase.Messaging;

namespace Sodao.FastSocket.Client.Messaging {
    /// <summary>
    /// thrift message.
    /// </summary>
    public class ClientThriftMessage : ThriftMessage {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        public ClientThriftMessage(byte[] payload) : base(payload) {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        public ClientThriftMessage(string payload) : base(payload) {

        }
    }
}