using System;
using System.Threading.Tasks;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.SocketBase.Protocol {
    /// <summary>
    /// 发送消息跟踪信息
    /// </summary>
    public class SendMessageInfo<TMessage> : ISendMessageInfo<TMessage> {
        #region Members
        /// <summary>
        /// CreatedTime
        /// </summary>
        public DateTime CreatedTime { get; } = DateTime.Now;
        /// <summary>
        /// get or set sent size.
        /// </summary>
        public int SentSize { get; set; } = 0;
        /// <summary>
        /// get payload
        /// </summary>
        public byte[] Payload { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isSync"></param>
        /// <param name="seqId"></param>
        /// <exception cref="ArgumentNullException">payload is null.</exception>
        public SendMessageInfo(TMessage message, bool isSync, int seqId = 0) {
            this.SeqID = seqId;
            this.IsSync = isSync;
            this.Message = message;
            this.TaskCompletionSource = new TaskCompletionSource<bool>(this);
            this.Task = TaskCompletionSource.Task;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// get or set tag object
        /// </summary>
        public object Tag { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// SentTime
        /// </summary>
        public DateTime SentTime { get; set; }
        /// <summary>
        /// 获取一个值，该值指示当前message是否已发送完毕.
        /// </summary>
        /// <returns>true表示已发送完毕</returns>
        public bool IsSent() {
            return this.SentSize == this.Payload.Length;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        protected TaskCompletionSource<bool> TaskCompletionSource { get; }

        /// <summary>
        /// Task
        /// </summary>
        public Task<bool> Task { get; }

        /// <summary>
        /// SetResult
        /// </summary>
        /// <param name="result"></param>
        public void SetSendResult(bool result) {
            if (result) {
                this.SentTime = DateTime.Now;
            }
            TaskCompletionSource.TrySetResult(result);
        }

        /// <summary>
        /// SetException
        /// </summary>
        /// <param name="ex"></param>
        public void SetSendException(Exception ex) {
            TaskCompletionSource.TrySetException(ex);
        }

        /// <summary>
        /// Message
        /// </summary>
        public TMessage Message { get; }

        /// <summary>
        /// 
        /// </summary>
        public int SeqID { get; private set; }

        /// <summary>
        /// SetPayload
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="seqId"></param>
        public void SetPayload(byte[] payload, int seqId) {
            this.Payload = payload;
            this.SeqID = seqId;
        }


        /// <summary>
        /// 
        /// </summary>
        public bool IsSync { get; }

        /// <summary>
        /// 
        /// </summary>
        public SendMessageInfoProcess Process { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="process"></param>
        public void SetProcess(SendMessageInfoProcess process) {
            Process |= process;
        }
    }
}