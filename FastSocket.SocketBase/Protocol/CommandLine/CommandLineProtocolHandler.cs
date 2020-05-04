using System;
using System.Linq;
using System.Text;
using Sodao.FastSocket.SocketBase.Messaging;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;
using System.Collections.Generic;
using System.Threading;

namespace Sodao.FastSocket.SocketBase.Protocol.CommandLine {

    /// <summary>
    /// 命令行协议
    /// </summary>
    public abstract class CommandLineProtocolHandler<TSendMessageInfo> : IProtocolHandler<TSendMessageInfo, CommandLineMessage>
        where TSendMessageInfo : ISendMessageInfo<CommandLineMessage> {

        static readonly string[] SPLITER = new string[] { " " };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract TSendMessageInfo CreateMessageInfo(CommandLineMessage message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageInfo"></param>
        /// <param name="seqId"></param>
        /// <returns></returns>
        public byte[] Serialize(TSendMessageInfo messageInfo, out int seqId) {
            var msg = messageInfo.Message;
            var sb = new StringBuilder();
            seqId = messageInfo.IsSync ? (messageInfo.SeqID > 0 ? messageInfo.SeqID : Interlocked.Increment(ref index)) : 0;
            if (messageInfo.IsSync) {
                sb.Append($"- {seqId} ");
            }
            sb.Append(msg.FullCommandLine);
            sb.Append(Environment.NewLine);
            var payload = Encoding.UTF8.GetBytes(sb.ToString());
            return payload;
        }
        int index = 0;

        /// <summary>
        /// parse
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="maxMessageSize"></param>
        /// <param name="recvMessageInfo"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        /// <exception cref="BadProtocolException">bad command line protocol</exception>
        public bool TryParse(IConnection connection, ArraySegment<byte> buffer, int maxMessageSize, out IRecvivedMessageInfo<CommandLineMessage> recvMessageInfo, out int readlength) {
            recvMessageInfo = null;
            readlength = 0;

            if (buffer.Count < 2) {
                return false;
            }

            //查找\r\n标记符
            for (int i = buffer.Offset, len = buffer.Offset + buffer.Count; i < len; i++) {
                if (buffer.Array[i] == 13 && i + 1 < len && buffer.Array[i + 1] == 10) {
                    readlength = i + 2 - buffer.Offset;

                    if (readlength == 2) { recvMessageInfo = new RecvivedMessageInfo<CommandLineMessage>(new CommandLineMessage(string.Empty), false); return true; }
                    if (readlength > maxMessageSize) throw new BadProtocolException("message is too long");

                    string text = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, readlength - 2);
                    var array = text.Split(SPLITER, StringSplitOptions.RemoveEmptyEntries);
                    var queue = new Queue<string>(array);
                    recvMessageInfo = new RecvivedMessageInfo<CommandLineMessage>(ParseMessageText(queue, out var isSync, out int seqId), isSync, seqId);
                    return true;
                }
            }
            readlength = 0;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public abstract void UpdateCryptKey(object key);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="isSync"></param>
        /// <param name="seqId"></param>
        /// <returns></returns>
        protected CommandLineMessage ParseMessageText(Queue<string> queue, out bool isSync, out int seqId) {
            seqId = 0;
            var first = queue.Dequeue();
            isSync = first.Length == 1 && first[0] == '-';
            if (isSync && !int.TryParse(queue.Dequeue(), out seqId)) {
                throw new BadProtocolException();
            }
            return isSync ? new CommandLineMessage(queue) : new CommandLineMessage(first, queue);
        }


        /// <summary>
        /// 
        /// </summary>
        public void Dispose() {

        }
    }
}