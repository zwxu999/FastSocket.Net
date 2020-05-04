using System;

namespace Sodao.FastSocket.SocketBase.Protocol.Abstractions {
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum SendMessageInfoProcess {
        /// <summary>
        /// 
        /// </summary>
        Enqueue = 1,
        /// <summary>
        /// 
        /// </summary>
        Dequeue = 1 << 1,

        /// <summary>
        /// 
        /// </summary>
        BeginSendMessage = 1 << 2,

        /// <summary>
        /// 
        /// </summary>
        Serialize = 1 << 3,

        /// <summary>
        /// 
        /// </summary>
        NoProtocolHandler = 1 << 4,

        /// <summary>
        /// 
        /// </summary>
        TrySend = 1 << 5,

        /// <summary>
        /// 
        /// </summary>
        MessageQueueClosed = 1 << 6,

        /// <summary>
        /// 
        /// </summary>
        MessageQueueEnqueue = 1 << 7,

        /// <summary>
        /// 
        /// </summary>
        MessageQueueTrySendNext = 1 << 8,

        /// <summary>
        /// 
        /// </summary>
        MessageQueueIDLE = 1 << 9,

        /// <summary>
        /// 
        /// </summary>
        MessageQueueSending = 1 << 10,

        /// <summary>
        /// 
        /// </summary>
        MessageQueueDequeue = 1 << 11,

        /// <summary>
        /// 
        /// </summary>
        SendMessageInternal_1 = 1 << 12,

        /// <summary>
        /// 
        /// </summary>
        SendMessageInternal_1_Error = 1 << 13,

        /// <summary>
        /// 
        /// </summary>
        OnStartSending = 1 << 14,

        /// <summary>
        /// 
        /// </summary>
        SendMessageInternal_2 = 1 << 15,

        /// <summary>
        /// 
        /// </summary>
        SendMessageInternal_2_Error = 1 << 16,

        /// <summary>
        /// 
        /// </summary>
        SendAsync = 1 << 17,

        /// <summary>
        /// 
        /// </summary>
        SendAsyncCompleted = 1 << 18,

        /// <summary>
        /// 
        /// </summary>
        SendAsyncCompleted_Error = 1 << 19,

        /// <summary>
        /// 
        /// </summary>
        SendAsyncCompleted_Success = 1 << 20,

        /// <summary>
        /// 
        /// </summary>
        SendAsyncCompleted_SendMessageInternal_2 = 1 << 21,

        /// <summary>
        /// 
        /// </summary>
        SendAsyncCompleted_NoConnected = 1 << 22,

        /// <summary>
        /// 
        /// </summary>
        OnSendCallback = 1 << 23,

        /// <summary>
        /// 
        /// </summary>
        SetSendResult = 1 << 24,

        /// <summary>
        /// 
        /// </summary>
        Retry = 1 << 25,
    }
}