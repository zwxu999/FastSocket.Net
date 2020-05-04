using System;
using System.Text;

namespace Sodao.FastSocket.SocketBase.Messaging {
    /// <summary>
    /// thrift message.
    /// </summary>
    public class ThriftMessage {
        /// <summary>
        /// payload
        /// </summary>
        public byte[] Payload { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        public ThriftMessage(byte[] payload) {
            Payload = payload;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        public ThriftMessage(string payload) {
            this.Payload = Encoding.UTF8.GetBytes(payload);
        }

        /// <summary>
        /// 
        /// </summary>
        public string Text => Encoding.UTF8.GetString(Payload);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Text;
    }
}