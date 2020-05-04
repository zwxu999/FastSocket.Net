using System;
using System.Linq;
using Sodao.FastSocket.SocketBase.Messaging;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.SocketBase.Protocol.Thrift {

    /// 消息帧头部结构定义
    /// +------+-------------------+----------+----------+------+------+
    /// | Byte | 0-3               | 4        | 5-8      | 9    | 10-N |
    /// +------+-------------------+----------+----------+------+------+
    /// | 描述 | 帧长（不含本字段）| 同步标记 | 消息编号 | 保留 | 数据 |
    /// +------+-------------------+----------+----------+------+------+

    /// <summary>
    /// thrift protocol
    /// </summary>
    public abstract class ThriftProtocolHandler<TSendMessageInfo, TThriftMessage> : IProtocolHandler<TSendMessageInfo, TThriftMessage>
        where TSendMessageInfo : ISendMessageInfo<TThriftMessage>
        where TThriftMessage : ThriftMessage {

        int index = 0;

        /// <summary>
        /// 
        /// </summary>
        public ThriftProtocolHandler() {

        }

        /// <summary>
        /// parse
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="maxMessageSize"></param>
        /// <param name="recvivedMessageInfo"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        /// <exception cref="BadProtocolException">bad thrift protocol</exception>
        public bool TryParse(IConnection connection, ArraySegment<byte> buffer, int maxMessageSize, out IRecvivedMessageInfo<TThriftMessage> recvivedMessageInfo, out int readlength) {
            recvivedMessageInfo = null;
            readlength = 0;
            if (buffer.Count < 4) {
                return false;
            }

            //获取message length
            var messageLength = Utils.NetworkBitConverter.ToInt32(buffer.Array, buffer.Offset);
            if (messageLength < 10) throw new BadProtocolException("bad thrift protocol");
            if (messageLength > maxMessageSize) throw new BadProtocolException("message is too long");

            readlength = messageLength + 4;
            if (buffer.Count < readlength) {
                readlength = 0;
                return false;
            }

            bool isSync = BitConverter.ToBoolean(buffer.Array, buffer.Offset + 4);
            int seqId = Utils.NetworkBitConverter.ToInt32(buffer.Array, buffer.Offset + 5);

            var payload = new byte[messageLength - 6];
            Buffer.BlockCopy(buffer.Array, buffer.Offset + 10, payload, 0, payload.Length);
            var message = CreateThriftMessage(payload);
            recvivedMessageInfo = new RecvivedMessageInfo<TThriftMessage>(message, isSync, seqId);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageInfo"></param>
        /// <param name="seqId"></param>
        /// <returns></returns>
        public byte[] Serialize(TSendMessageInfo messageInfo, out int seqId) {
            var isSync = messageInfo.IsSync;
            var payload = messageInfo.Message.Payload;

            seqId = isSync ? messageInfo.SeqID > 0 ? messageInfo.SeqID : System.Threading.Interlocked.Increment(ref index) : 0;
            var bytes = new byte[payload.Length + 10];

            Array.Copy(Utils.NetworkBitConverter.GetBytes(bytes.Length - 4), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(isSync), 0, bytes, 4, 1);
            Array.Copy(Utils.NetworkBitConverter.GetBytes(seqId), 0, bytes, 5, 4);
            Array.Copy(payload, 0, bytes, 10, payload.Length);
            return bytes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected abstract TThriftMessage CreateThriftMessage(byte[] payload);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public void UpdateCryptKey(object key) {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract TSendMessageInfo CreateMessageInfo(TThriftMessage message);

        /// <summary>
        /// 
        /// </summary>
        public void Dispose() {

        }
    }
}