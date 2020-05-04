using System;
using System.Threading.Tasks;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.SocketBase.Protocol.Abstractions {


    /// <summary>
    /// 发送消息跟踪信息
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface ISendMessageInfo<TMessage> {
        /// <summary>
        /// CreatedTime
        /// </summary>
        DateTime CreatedTime { get; }

        /// <summary>
        /// SentSize
        /// </summary>
        int SentSize { get; set; }

        /// <summary>
        /// SentTime
        /// </summary>
        DateTime SentTime { get; set; }
        /// <summary>
        /// Payload
        /// </summary>
        byte[] Payload { get; }
        /// <summary>
        /// IsSent
        /// </summary>
        /// <returns></returns>
        bool IsSent();
        /// <summary>
        /// Task
        /// </summary>
        Task<bool> Task { get; }
        /// <summary>
        /// SetResult
        /// </summary>
        /// <param name="result"></param>
        void SetSendResult(bool result);

        /// <summary>
        /// SetException
        /// </summary>
        /// <param name="ex"></param>
        void SetSendException(Exception ex);
        /// <summary>
        /// Message
        /// </summary>
        TMessage Message { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="seqId"></param>
        void SetPayload(byte[] payload, int seqId);

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
        SendMessageInfoProcess Process { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="process"></param>
        void SetProcess(SendMessageInfoProcess process);
    }
}