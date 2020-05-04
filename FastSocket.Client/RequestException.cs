using System;

namespace Sodao.FastSocket.Client {
    /// <summary>
    /// socket message exception
    /// </summary>
    public sealed class MessageException : ApplicationException {
        /// <summary>
        /// error
        /// </summary>
        public readonly Errors Error;
        /// <summary>
        /// message name
        /// </summary>
        public readonly string MessageName;

        /// <summary>
        /// new
        /// </summary>
        /// <param name="error"></param>
        /// <param name="id"></param>
        public MessageException(Errors error, int id)
            : base(string.Concat("errorType:", error.ToString(), " name:", id.ToString() ?? string.Empty)) {
            this.Error = error;
            this.MessageName = id.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        public MessageException(Errors error) {
            this.Error = error;
        }

        /// <summary>
        /// new
        /// </summary>
        /// <param name="error"></param>
        /// <param name="name"></param>
        public MessageException(Errors error, string name)
            : base(string.Concat("errorType:", error.ToString(), " name:", name ?? string.Empty)) {
            this.Error = error;
            this.MessageName = name;
        }

        /// <summary>
        /// error type enum
        /// </summary>
        public enum Errors : byte {
            /// <summary>
            /// 未知
            /// </summary>
            Unknow = 0,
            /// <summary>
            /// 等待发送超时
            /// </summary>
            PendingSendTimeout = 1,
            /// <summary>
            /// 接收超时
            /// </summary>
            ReceiveTimeout = 2,
            /// <summary>
            /// 发送失败
            /// </summary>
            SendFaild = 3,
        }
    }
}