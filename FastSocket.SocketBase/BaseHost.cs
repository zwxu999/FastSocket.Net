using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Sodao.FastSocket.SocketBase.Messaging;
using Sodao.FastSocket.SocketBase.Protocol;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.SocketBase {
    /// <summary>
    /// base host
    /// </summary>
    public abstract class BaseHost<TSendMessageInfo, TMessage> : IHost<TSendMessageInfo, TMessage> where TSendMessageInfo : class, ISendMessageInfo<TMessage> {

        #region Members
        private long _connectionID = 1000L;
        private readonly ConnectionCollection _listConnections = new ConnectionCollection();
        private readonly SocketAsyncEventArgsPool _saePool = null;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="socketBufferSize"></param>
        /// <param name="messageBufferSize"></param>
        /// <exception cref="ArgumentOutOfRangeException">socketBufferSize</exception>
        /// <exception cref="ArgumentOutOfRangeException">messageBufferSize</exception>
        protected BaseHost(int socketBufferSize, int messageBufferSize) {
            if (socketBufferSize < 1) throw new ArgumentOutOfRangeException("socketBufferSize");
            if (messageBufferSize < 1) throw new ArgumentOutOfRangeException("messageBufferSize");

            this.SocketBufferSize = socketBufferSize;
            this.MessageBufferSize = messageBufferSize;
            this._saePool = new SocketAsyncEventArgsPool(messageBufferSize);
        }
        #endregion

        #region IHost Members
        /// <summary>
        /// get socket buffer size
        /// </summary>
        public int SocketBufferSize {
            get;
            private set;
        }
        /// <summary>
        /// get message buffer size
        /// </summary>
        public int MessageBufferSize {
            get;
            private set;
        }

        /// <summary>
        /// create new <see cref="IConnection"/>
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">socket is null</exception>
        public virtual IConnection<TSendMessageInfo, TMessage> NewConnection(Socket socket) {
            if (socket == null) throw new ArgumentNullException("socket");

            socket.NoDelay = true;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            socket.ReceiveBufferSize = this.SocketBufferSize;
            socket.SendBufferSize = this.SocketBufferSize;
            return new DefaultConnection(this.NextConnectionID(), socket, this);
        }
        /// <summary>
        /// get <see cref="IConnection"/> by connectionID
        /// </summary>
        /// <param name="connectionID"></param>
        /// <returns></returns>
        public IConnection GetConnectionByID(long connectionID) {
            return this._listConnections.Get(connectionID);
        }
        /// <summary>
        /// list all <see cref="IConnection"/>
        /// </summary>
        /// <returns></returns>
        public IConnection[] ListAllConnection() {
            return this._listConnections.ToArray();
        }
        /// <summary>
        /// get connection count.
        /// </summary>
        /// <returns></returns>
        public int CountConnection() {
            return this._listConnections.Count();
        }

        /// <summary>
        /// 启动
        /// </summary>
        public virtual void Start() {
        }
        /// <summary>
        /// 停止
        /// </summary>
        public virtual void Stop() {
            this._listConnections.DisconnectAll();
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// 生成下一个连接ID
        /// </summary>
        /// <returns></returns>
        protected long NextConnectionID() {
            return Interlocked.Increment(ref this._connectionID);
        }
        /// <summary>
        /// register connection
        /// </summary>
        /// <param name="connection"></param>
        /// <exception cref="ArgumentNullException">connection is null</exception>
        protected void RegisterConnection(IConnection connection) {
            if (connection == null) throw new ArgumentNullException("connection");
            if (connection.Active) {
                this._listConnections.Add(connection);
                this.OnConnected(connection);
            }
        }
        /// <summary>
        /// OnConnected
        /// </summary>
        /// <param name="connection"></param>
        protected virtual void OnConnected(IConnection connection) {
            Log.Trace.Debug(string.Concat("socket connected, id:", connection.ConnectionID.ToString(),
                ", remot endPoint:", connection.RemoteEndPoint == null ? "unknow" : connection.RemoteEndPoint.ToString(),
                ", local endPoint:", connection.LocalEndPoint == null ? "unknow" : connection.LocalEndPoint.ToString()));
        }
        /// <summary>
        /// OnStartSending
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="messageInfo"></param>
        protected virtual void OnStartSending(IConnection connection, TSendMessageInfo messageInfo) {

        }

        /// <summary>
        /// OnSendCallback
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="messageInfo"></param>
        /// <param name="isSuccess"></param>
        protected abstract void OnSendCallback(IConnection<TSendMessageInfo, TMessage> connection, TSendMessageInfo messageInfo, bool isSuccess);

        /// <summary>
        /// OnMessageReceived
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="e"></param>
        protected abstract void OnMessageReceived(IConnection<TSendMessageInfo, TMessage> connection, MessageReceivedEventArgs<IRecvivedMessageInfo<TMessage>> e);
        /// <summary>
        /// OnDisconnected
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        /// <exception cref="ArgumentNullException">connection is null</exception>
        protected virtual void OnDisconnected(IConnection<TSendMessageInfo, TMessage> connection, Exception ex) {
            this._listConnections.Remove(connection.ConnectionID);
            Log.Trace.Debug(string.Concat("socket disconnected, id:", connection.ConnectionID.ToString(),
                ", remot endPoint:", connection.RemoteEndPoint == null ? "unknow" : connection.RemoteEndPoint.ToString(),
                ", local endPoint:", connection.LocalEndPoint == null ? "unknow" : connection.LocalEndPoint.ToString(),
                ex == null ? string.Empty : string.Concat(", reason is: ", ex.ToString())));
        }
        /// <summary>
        /// OnError
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        protected virtual void OnConnectionError(IConnection connection, Exception ex) {
            Log.Trace.Error(ex.Message, ex);
        }
        #endregion

        /// <summary>
        /// <see cref="SocketAsyncEventArgs"/> pool
        /// </summary>
        private class SocketAsyncEventArgsPool {
            #region Private Members
            private readonly int _messageBufferSize;
            private readonly ConcurrentStack<SocketAsyncEventArgs> _pool =
                new ConcurrentStack<SocketAsyncEventArgs>();
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            /// <param name="messageBufferSize"></param>
            public SocketAsyncEventArgsPool(int messageBufferSize) {
                this._messageBufferSize = messageBufferSize;
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// acquire
            /// </summary>
            /// <returns></returns>
            public SocketAsyncEventArgs Acquire() {
                SocketAsyncEventArgs e = null;
                if (this._pool.TryPop(out e)) return e;

                e = new SocketAsyncEventArgs();
                e.SetBuffer(new byte[this._messageBufferSize], 0, this._messageBufferSize);
                return e;
            }
            /// <summary>
            /// release
            /// </summary>
            /// <param name="e"></param>
            public void Release(SocketAsyncEventArgs e) {
                if (this._pool.Count < 10000) {
                    this._pool.Push(e);
                    return;
                }

                e.Dispose();
            }
            #endregion
        }

        #region DefaultConnection
        /// <summary>
        /// default socket connection
        /// </summary>
        private class DefaultConnection : IConnection<TSendMessageInfo, TMessage> {
            #region Private Members
            private int _active = 1;
            private DateTime _latestActiveTime = Utils.Date.Now;
            private readonly int _messageBufferSize;
            private readonly BaseHost<TSendMessageInfo, TMessage> _host = null;

            private readonly Socket _socket = null;

            private SocketAsyncEventArgs _saeSend = null;
            private readonly MessageQueue _messageQueue = null;

            private SocketAsyncEventArgs _saeReceive = null;
            private MemoryStream _tsStream = null;
            private int _isReceiving = 0;
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            /// <param name="connectionID"></param>
            /// <param name="socket"></param>
            /// <param name="host"></param>
            /// <exception cref="ArgumentNullException">socket is null</exception>
            /// <exception cref="ArgumentNullException">host is null</exception>
            public DefaultConnection(long connectionID, Socket socket, BaseHost<TSendMessageInfo, TMessage> host) {

                if (host == null) throw new ArgumentNullException("host");

                this.ConnectionID = connectionID;
                this._socket = socket ?? throw new ArgumentNullException("socket");
                this._messageBufferSize = host.MessageBufferSize;
                this._host = host;

                try {
                    this.LocalEndPoint = (IPEndPoint)socket.LocalEndPoint;
                    this.RemoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;
                } catch (Exception ex) { Log.Trace.Error("get socket endPoint error.", ex); }

                //init send
                this._saeSend = host._saePool.Acquire();
                this._saeSend.Completed += this.SendAsyncCompleted;
                this._messageQueue = new MessageQueue(this.SendMessageInternal);

                //init receive
                this._saeReceive = host._saePool.Acquire();
                this._saeReceive.Completed += this.ReceiveAsyncCompleted;
            }
            #endregion

            #region IConnection Members
            /// <summary>
            /// 连接断开事件
            /// </summary>
            public event DisconnectedHandler Disconnected;

            /// <summary>
            /// return the connection is active.
            /// </summary>
            public bool Active {
                get { return Thread.VolatileRead(ref this._active) == 1; }
            }
            /// <summary>
            /// get the connection latest active time.
            /// </summary>
            public DateTime LatestActiveTime {
                get { return this._latestActiveTime; }
            }
            /// <summary>
            /// get the connection id.
            /// </summary>
            public long ConnectionID { get; }
            /// <summary>
            /// 获取本地IP地址
            /// </summary>
            public IPEndPoint LocalEndPoint { get; private set; }
            /// <summary>
            /// 获取远程IP地址
            /// </summary>
            public IPEndPoint RemoteEndPoint { get; private set; }
            /// <summary>
            /// 获取或设置与用户数据
            /// </summary>
            public object UserData { get; set; }

            /// <summary>
            /// 异步发送数据
            /// </summary>
            /// <param name="message"></param>
            public Task<bool> BeginSend(object message) {
                if (message == null) {
                    throw new ArgumentNullException(nameof(message));
                }

                var protocolHandler = this.ProtocolHandler;
                if (protocolHandler != null) {
                    var messageInfo = message as TSendMessageInfo;
                    if (messageInfo == null) {
                        if (message is TMessage tmessage || TryConvert(message, out tmessage)) {
                            messageInfo = protocolHandler.CreateMessageInfo(tmessage);
                        } else {
                            throw new NotSupportedException($"{nameof(message)}，需要：{typeof(TMessage)}，当前：{message.GetType()}");
                        }
                    }
                    BeginSendMessage(messageInfo);
                    return messageInfo.Task;
                } else {
                    return Task<bool>.Factory.StartNew(() => false);
                }
            }

            bool TryConvert(object source, out TMessage message) {
                message = default(TMessage);
                var destType = typeof(TMessage);
                var sourceType = source.GetType();
                var destTypeConvert = TypeDescriptor.GetConverter(destType);
                if (destTypeConvert != null && destTypeConvert.CanConvertFrom(sourceType) && destTypeConvert.IsValid(source)) {
                    message = (TMessage)destTypeConvert.ConvertFrom(source);
                    return true;
                }

                var sourceTypeConvert = TypeDescriptor.GetConverter(sourceType);
                if (sourceTypeConvert != null && sourceTypeConvert.CanConvertTo(destType)) {
                    message = (TMessage)sourceTypeConvert.ConvertTo(source, destType);
                    return true;
                }

                return false;
            }


            public void BeginSendMessage(TSendMessageInfo messageInfo) {
                messageInfo.SetProcess(SendMessageInfoProcess.BeginSendMessage);
                var protocolHandler = this.ProtocolHandler;
                if (protocolHandler != null) {

                    messageInfo.SetPayload(protocolHandler.Serialize(messageInfo, out var seqId), seqId);
                    messageInfo.SetProcess(SendMessageInfoProcess.Serialize);

                    if (!this._messageQueue.TrySend(messageInfo))
                        this.OnSendCallback(messageInfo, false);

                } else {
                    messageInfo.SetProcess(SendMessageInfoProcess.NoProtocolHandler);
                    OnSendCallback(messageInfo, false);
                    //messageInfo.SetSendResult(false);
                }
            }

            /// <summary>
            /// 异步接收数据
            /// </summary>
            public void BeginReceive() {
                if (Interlocked.CompareExchange(ref this._isReceiving, 1, 0) == 0)
                    this.ReceiveInternal();
            }
            /// <summary>
            /// 异步断开连接
            /// </summary>
            /// <param name="ex"></param>
            public void BeginDisconnect(Exception ex = null) {
                if (Interlocked.CompareExchange(ref this._active, 0, 1) == 1)
                    this.DisconnectInternal(ex);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            public void UpdateCryptKey(object key) {
                this.ProtocolHandler.UpdateCryptKey(key);
            }
            #endregion

            #region Private Methods

            #region Free
            /// <summary>
            /// free send queue
            /// </summary>
            private void FreeSendQueue() {
                var result = this._messageQueue.Close();
                if (result.BeforeState == MessageQueue.CLOSED) return;

                if (result.Messages != null)
                    foreach (var p in result.Messages) {
                        p.SetProcess(SendMessageInfoProcess.MessageQueueClosed);
                        this.OnSendCallback(p, false);
                    }

                if (result.BeforeState == MessageQueue.IDLE) this.FreeSend();
            }
            /// <summary>
            /// free for send.
            /// </summary>
            private void FreeSend() {
                var se = this._saeSend;
                if (se != null) {
                    se.Completed -= this.SendAsyncCompleted;
                    this._host._saePool.Release(se);
                    this._saeSend = null;
                }
                var protocolHandler = ProtocolHandler;
                if (protocolHandler != null) {
                    protocolHandler.Dispose();
                    this.ProtocolHandler = null;
                }
            }
            /// <summary>
            /// free fo receive.
            /// </summary>
            private void FreeReceive() {
                var re = this._saeReceive;
                if (re != null) {
                    re.Completed -= this.ReceiveAsyncCompleted;
                    this._host._saePool.Release(re);
                    this._saeReceive = null;
                }
                if (this._tsStream != null) {
                    this._tsStream.Close();
                    this._tsStream = null;
                }
            }
            #endregion

            #region Fire Events
            /// <summary>
            /// fire StartSending
            /// </summary>
            /// <param name="messageInfo"></param>
            private void OnStartSending(TSendMessageInfo messageInfo) {
                messageInfo.SetProcess(SendMessageInfoProcess.OnStartSending);
                this._host.OnStartSending(this, messageInfo);
            }
            /// <summary>
            /// fire SendCallback
            /// </summary>
            /// <param name="messageInfo"></param>
            /// <param name="isSuccess"></param>
            private void OnSendCallback(TSendMessageInfo messageInfo, bool isSuccess) {
                messageInfo.SetProcess(SendMessageInfoProcess.OnSendCallback);
                if (isSuccess) {
                    this._latestActiveTime = Utils.Date.Now;
                } else {
                    messageInfo.SentSize = 0;
                }

                this._host.OnSendCallback(this, messageInfo, isSuccess);
            }

            /// <summary>
            /// fire MessageReceived
            /// </summary>
            /// <param name="e"></param>
            private void OnPacketReceived(PacketReceivedEventArgs e) {
                var protocolHandler = this.ProtocolHandler;
                if (protocolHandler != null) {
                    try {
                        var readlengthTotal = 0;
                        var payload = e.Buffer;
                        //为防止可能的深度递归导致堆栈溢出
                        while (protocolHandler.TryParse(this, payload, _messageBufferSize, out var message, out int readlength)) {
                            this._latestActiveTime = Utils.Date.Now;
                            this._host.OnMessageReceived(this, new MessageReceivedEventArgs<IRecvivedMessageInfo<TMessage>>(message));
                            readlengthTotal += readlength;
                            payload = new ArraySegment<byte>(payload.Array, payload.Offset + readlength, payload.Count - readlength);
                        }
                        e.SetReadlength(readlengthTotal);
                    } catch (Exception ex) {
                        this._host.OnConnectionError(this, ex);
                        BeginDisconnect(ex);
                        e.SetReadlength(e.Buffer.Count);
                    }
                }
            }
            /// <summary>
            /// fire Disconnected
            /// </summary>
            private void OnDisconnected(Exception ex) {
                this.Disconnected?.Invoke(this, ex);
                this._host.OnDisconnected(this, ex);
            }
            /// <summary>
            /// fire Error
            /// </summary>
            /// <param name="ex"></param>
            private void OnError(Exception ex) {
                this._host.OnConnectionError(this, ex);
            }
            #endregion

            #region Send
            /// <summary>
            /// internal send message.
            /// </summary>
            /// <param name="messageInfo"></param>
            /// <exception cref="ArgumentNullException">message is null</exception>
            private void SendMessageInternal(TSendMessageInfo messageInfo) {
                messageInfo.SetProcess(SendMessageInfoProcess.SendMessageInternal_1);
                var e = this._saeSend;
                var socket = this._socket;
                if (socket.Connected && e != null) {
                    this.OnStartSending(messageInfo);
                    this.SendMessageInternal(socket, e, messageInfo);
                } else {
                    messageInfo.SetProcess(SendMessageInfoProcess.SendMessageInternal_1_Error);
                    OnSendCallback(messageInfo, false);
                }
            }

            /// <summary>
            /// internal send message.
            /// </summary>
            /// <param name="socket"></param>
            /// <param name="e"></param>
            /// <param name="messageInfo"></param>
            private void SendMessageInternal(Socket socket, SocketAsyncEventArgs e, TSendMessageInfo messageInfo) {

                messageInfo.SetProcess(SendMessageInfoProcess.SendMessageInternal_2);
                //按messageBufferSize大小分块传输
                var length = Math.Min(messageInfo.Payload.Length - messageInfo.SentSize, this._messageBufferSize);

                try {
                    //copy data to send buffer
                    Buffer.BlockCopy(messageInfo.Payload, messageInfo.SentSize, e.Buffer, 0, length);
                    e.SetBuffer(0, length);
                    e.UserToken = messageInfo;
                    messageInfo.SetProcess(SendMessageInfoProcess.SendAsync);
                    if (!socket.SendAsync(e))
                        this.SendAsyncCompleted(socket, e);
                } catch (Exception ex) {
                    messageInfo.SetProcess(SendMessageInfoProcess.SendMessageInternal_2_Error);
                    messageInfo.SetSendException(ex);
                    this.BeginDisconnect(ex);
                    this.FreeSend();
                    this.OnSendCallback(messageInfo, false);
                    this.OnError(ex);
                }

            }
            /// <summary>
            /// async send callback
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void SendAsyncCompleted(object sender, SocketAsyncEventArgs e) {
                var socket = sender as Socket;
                var messageInfo = e.UserToken as TSendMessageInfo;

                messageInfo.SetProcess(SendMessageInfoProcess.SendAsyncCompleted);
                //send error!
                if (e.SocketError != SocketError.Success) {
                    messageInfo.SetProcess(SendMessageInfoProcess.SendAsyncCompleted_Error);
                    this.BeginDisconnect(new SocketException((int)e.SocketError));
                    this.FreeSend();
                    this.OnSendCallback(messageInfo, false);
                    return;
                }

                messageInfo.SentSize += e.BytesTransferred;

                {
                    if (messageInfo.IsSent()) {
                        messageInfo.SetProcess(SendMessageInfoProcess.SendAsyncCompleted_Success);
                        this.OnSendCallback(messageInfo, true);

                        //try send next message
                        if (!this._messageQueue.TrySendNext()) this.FreeSend();
                    } else if (socket.Connected) {
                        messageInfo.SetProcess(SendMessageInfoProcess.SendAsyncCompleted_SendMessageInternal_2);
                        this.SendMessageInternal(socket, e, messageInfo);//continue send this message
                    } else {
                        messageInfo.SetProcess(SendMessageInfoProcess.SendAsyncCompleted_NoConnected);
                        this.OnSendCallback(messageInfo, false);
                    }

                }
            }
            #endregion

            #region Receive
            /// <summary>
            /// receive
            /// </summary>
            private void ReceiveInternal() {
                var e = this._saeReceive;
                var socket = this._socket;
                if (socket.Connected && e != null) {
                    bool completed = true;
                    try { completed = socket.ReceiveAsync(e); } catch (Exception ex) {
                        this.BeginDisconnect(ex);
                        this.FreeReceive();
                        this.OnError(ex);
                    }

                    if (!completed)
                        this.ReceiveAsyncCompleted(socket, e);//ThreadPool.QueueUserWorkItem(_ => );
                }
            }

            /// <summary>
            /// async receive callback
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void ReceiveAsyncCompleted(object sender, SocketAsyncEventArgs e) {
                if (e.SocketError != SocketError.Success) {
                    this.BeginDisconnect(new SocketException((int)e.SocketError));
                    this.FreeReceive();
                } else if (e.BytesTransferred < 1) {
                    this.BeginDisconnect();
                    this.FreeReceive();
                } else {
                    ArraySegment<byte> buffer;
                    var ts = this._tsStream;
                    if (ts == null || ts.Length == 0) {
                        buffer = new ArraySegment<byte>(e.Buffer, 0, e.BytesTransferred);
                    } else {
                        ts.Write(e.Buffer, 0, e.BytesTransferred);
                        buffer = new ArraySegment<byte>(ts.GetBuffer(), 0, (int)ts.Length);
                    }
                    this.OnPacketReceived(new PacketReceivedEventArgs(buffer, this.PacketProcessCallback));
                }
            }
            /// <summary>
            /// message process callback
            /// </summary>
            /// <param name="payload"></param>
            /// <param name="readlength"></param>
            /// <exception cref="ArgumentOutOfRangeException">readlength less than 0 or greater than payload.Count.</exception>
            private void PacketProcessCallback(ArraySegment<byte> payload, int readlength) {
                if (readlength < 0 || readlength > payload.Count) {
                    throw new ArgumentOutOfRangeException("readlength", "readlength less than 0 or greater than payload.Count.");
                }

                var ts = this._tsStream ?? (this._tsStream = new MemoryStream(this._messageBufferSize));
                ts.SetLength(0);

                ts.Write(payload.Array, payload.Offset + readlength, payload.Count - readlength);
                this.ReceiveInternal();
            }
            #endregion

            #region Disconnect
            /// <summary>
            /// disconnect
            /// </summary>
            /// <param name="reason"></param>
            private void DisconnectInternal(Exception reason) {
                var e = new SocketAsyncEventArgs();
                e.Completed += this.DisconnectAsyncCompleted;
                e.UserToken = reason;

                var completedAsync = true;
                var socket = this._socket;
                try {
                    //this._socket.Shutdown(SocketShutdown.Both);
                    completedAsync = socket.DisconnectAsync(e);
                } catch (Exception ex) {
                    Log.Trace.Error(ex.Message, ex);
                    //ThreadPool.QueueUserWorkItem(_ => this.DisconnectAsyncCompleted(this, e));
                    //return;
                }

                if (!completedAsync)
                    this.DisconnectAsyncCompleted(socket, e);// ThreadPool.QueueUserWorkItem(_ => );
            }
            /// <summary>
            /// async disconnect callback
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void DisconnectAsyncCompleted(object sender, SocketAsyncEventArgs e) {
                var socket = sender as Socket;
                //dispose socket
                try { socket.Close(); } catch (Exception ex) { Log.Trace.Error(ex.Message, ex); }

                //dispose socketAsyncEventArgs
                var reason = e.UserToken as Exception;
                e.Completed -= this.DisconnectAsyncCompleted;
                e.Dispose();

                //fire disconnected
                this.OnDisconnected(reason);
                //close send queue
                this.FreeSendQueue();
            }
            #endregion

            /// <summary>
            /// SetProtoHandler
            /// </summary>
            /// <param name="protocolHandler"></param>
            public void SetProtoHandler(IProtocolHandler<TSendMessageInfo, TMessage> protocolHandler) {
                ProtocolHandler = protocolHandler;
            }
            IProtocolHandler<TSendMessageInfo, TMessage> ProtocolHandler { get; set; }
            #endregion

            #region MessageQueue
            ///// <summary>
            ///// message queue
            ///// </summary>
            //private class MessageQueue {
            //    #region Private Members
            //    public const long IDLE = 1;     //空闲状态
            //    public const long SENDING = 2;  //发送中
            //    public const long CLOSED = 5;   //已关闭

            //    private long _state = IDLE;      //当前状态
            //    private ConcurrentQueue<TSendMessageInfo> _queue = new ConcurrentQueue<TSendMessageInfo>();
            //    private Action<TSendMessageInfo> _sendAction = null;
            //    #endregion

            //    #region Constructors
            //    /// <summary>
            //    /// new
            //    /// </summary>
            //    /// <param name="sendAction"></param>
            //    /// <exception cref="ArgumentNullException">sendAction is null.</exception>
            //    public MessageQueue(Action<TSendMessageInfo> sendAction) {
            //        _sendAction = sendAction ?? throw new ArgumentNullException("sendAction");
            //    }
            //    #endregion

            //    #region Public Methods
            //    /// <summary>
            //    /// try send message
            //    /// </summary>
            //    /// <param name="messageInfo"></param>
            //    /// <returns>if CLOSED return false.</returns>
            //    public bool TrySend(TSendMessageInfo messageInfo) {
            //        messageInfo.SetProcess(SendMessageInfoProcess.TrySend);
            //        if (Interlocked.Read(ref _state) == CLOSED) {
            //            messageInfo.SetProcess(SendMessageInfoProcess.MessageQueueClosed);
            //            return false;
            //        }
            //        _queue.Enqueue(messageInfo);
            //        messageInfo.SetProcess(SendMessageInfoProcess.MessageQueueEnqueue);
            //        return TrySendNext(false, messageInfo);
            //    }

            //    /// <summary>
            //    /// close
            //    /// </summary>
            //    /// <returns></returns>
            //    public CloseResult Close() {
            //        var beforeState = Interlocked.Read(ref _state);
            //        Interlocked.Exchange(ref _state, CLOSED);
            //        var arrMessages = this._queue.ToArray();
            //        var queue = this._queue;
            //        if (queue != null) {
            //            while (this._queue.Count > 0) {
            //                this._queue.TryDequeue(out var _);
            //            }
            //            this._queue = null;
            //        }
            //        this._sendAction = null;
            //        return new CloseResult(beforeState, arrMessages);
            //    }

            //    /// <summary>
            //    /// try send next message
            //    /// </summary>
            //    /// <returns>if CLOSED return false.</returns>
            //    public bool TrySendNext(bool complate, TSendMessageInfo newmessageInfo = null) {
            //        newmessageInfo?.SetProcess(SendMessageInfoProcess.MessageQueueTrySendNext);
            //        //read current state
            //        if (Interlocked.Read(ref _state) == CLOSED) {
            //            newmessageInfo?.SetProcess(SendMessageInfoProcess.MessageQueueClosed);
            //            return false;
            //        }
            //        if (complate) {//last message send complate
            //            //try switch to idle from sending
            //            if (Interlocked.CompareExchange(ref _state, IDLE, SENDING) != SENDING) {
            //                //if not sneding, state error
            //                return false;
            //            }
            //        }
            //        //try switch to sending from idle
            //        if(Interlocked.CompareExchange(ref _state, SENDING, IDLE) == IDLE) {
            //            newmessageInfo?.SetProcess(SendMessageInfoProcess.MessageQueueIDLE);
            //            //switch succress
            //            if (this._queue.TryDequeue(out var messageInfo)) {//try get a messageinfo
            //                messageInfo.SetProcess(SendMessageInfoProcess.MessageQueueDequeue);
            //                //succ, send the messageinfo
            //                this._sendAction(messageInfo);
            //            } else {
            //                //can't get a messageInfo, restore state to idle
            //                if (Interlocked.CompareExchange(ref _state, IDLE, SENDING) != SENDING) {
            //                    //state error , queue is closed
            //                    return false;
            //                }
            //            }
            //        } else {
            //            newmessageInfo?.SetProcess(SendMessageInfoProcess.MessageQueueSending);
            //        }
            //        return true;
            //    }
            //    #endregion

            //    #region CloseResult
            //    /// <summary>
            //    /// close queue result
            //    /// </summary>
            //    public sealed class CloseResult {
            //        /// <summary>
            //        /// before close state
            //        /// </summary>
            //        public readonly long BeforeState;
            //        /// <summary>
            //        /// wait sending message array
            //        /// </summary>
            //        public readonly TSendMessageInfo[] Messages;

            //        /// <summary>
            //        /// new
            //        /// </summary>
            //        /// <param name="beforeState"></param>
            //        /// <param name="messages"></param>
            //        public CloseResult(long beforeState, TSendMessageInfo[] messages) {
            //            this.BeforeState = beforeState;
            //            this.Messages = messages;
            //        }
            //    }
            //    #endregion
            //}

            /// <summary>
            /// message queue
            /// </summary>
            private class MessageQueue {
                #region Private Members
                public const int IDLE = 1;     //空闲状态
                public const int SENDING = 2;  //发送中
                public const int ENQUEUE = 3;  //入列状态
                public const int DEQUEUE = 4;  //出列状态
                public const int CLOSED = 5;   //已关闭

                private int _state = IDLE;      //当前状态
                private Queue<TSendMessageInfo> _queue = new Queue<TSendMessageInfo>();
                private Action<TSendMessageInfo> _sendAction = null;
                #endregion

                #region Constructors
                /// <summary>
                /// new
                /// </summary>
                /// <param name="sendAction"></param>
                /// <exception cref="ArgumentNullException">sendAction is null.</exception>
                public MessageQueue(Action<TSendMessageInfo> sendAction) {
                    _sendAction = sendAction ?? throw new ArgumentNullException("sendAction");
                }
                #endregion

                #region Public Methods
                /// <summary>
                /// try send message
                /// </summary>
                /// <param name="messageInfo"></param>
                /// <returns>if CLOSED return false.</returns>
                public bool TrySend(TSendMessageInfo messageInfo) {
                    messageInfo.SetProcess(SendMessageInfoProcess.TrySend);
                    var spin = true;
                    while (spin) {
                        switch (this._state) {
                            case IDLE:
                                if (Interlocked.CompareExchange(ref this._state, SENDING, IDLE) == IDLE)
                                    messageInfo?.SetProcess(SendMessageInfoProcess.MessageQueueIDLE);
                                spin = false;
                                break;
                            case SENDING:
                                if (Interlocked.CompareExchange(ref this._state, ENQUEUE, SENDING) == SENDING) {
                                    this._queue.Enqueue(messageInfo);
                                    messageInfo.SetProcess(SendMessageInfoProcess.MessageQueueEnqueue);
                                    this._state = SENDING;
                                    return true;
                                }
                                break;
                            case ENQUEUE:
                            case DEQUEUE:
                                Thread.Yield();
                                break;
                            case CLOSED:
                                messageInfo.SetProcess(SendMessageInfoProcess.MessageQueueClosed);
                                return false;
                        }
                    }
                    this._sendAction(messageInfo);
                    return true;
                }
                /// <summary>
                /// close
                /// </summary>
                /// <returns></returns>
                public CloseResult Close() {
                    var spin = true;
                    int beforeState = -1;
                    while (spin) {
                        switch (this._state) {
                            case IDLE:
                                if (Interlocked.CompareExchange(ref this._state, CLOSED, IDLE) == IDLE) {
                                    spin = false;
                                    beforeState = IDLE;
                                }
                                break;
                            case SENDING:
                                if (Interlocked.CompareExchange(ref this._state, CLOSED, SENDING) == SENDING) {
                                    spin = false;
                                    beforeState = SENDING;
                                }
                                break;
                            case ENQUEUE:
                            case DEQUEUE:
                                Thread.Yield();
                                break;
                            case CLOSED:
                                return new CloseResult(CLOSED, null);
                        }
                    }

                    var arrMessages = this._queue.ToArray();
                    this._queue.Clear();
                    this._queue = null;
                    this._sendAction = null;
                    return new CloseResult(beforeState, arrMessages);
                }
                /// <summary>
                /// try send next message
                /// </summary>
                /// <returns>if CLOSED return false.</returns>
                public bool TrySendNext() {
                    var spin = true;
                    TSendMessageInfo messageInfo = null;
                    while (spin) {
                        switch (this._state) {
                            case SENDING:
                                if (Interlocked.CompareExchange(ref this._state, DEQUEUE, SENDING) == SENDING) {
                                    if (this._queue.Count == 0) {
                                        this._state = IDLE;
                                        return true;
                                    }

                                    messageInfo = this._queue.Dequeue();
                                    messageInfo.SetProcess(SendMessageInfoProcess.MessageQueueDequeue);
                                    this._state = SENDING;
                                    spin = false;
                                }
                                break;
                            case ENQUEUE:
                                Thread.Yield();
                                break;
                            case CLOSED:
                                return false;
                        }
                    }
                    this._sendAction(messageInfo);
                    return true;
                }
                #endregion

                #region CloseResult
                /// <summary>
                /// close queue result
                /// </summary>
                public sealed class CloseResult {
                    /// <summary>
                    /// before close state
                    /// </summary>
                    public readonly int BeforeState;
                    /// <summary>
                    /// wait sending message array
                    /// </summary>
                    public readonly TSendMessageInfo[] Messages;

                    /// <summary>
                    /// new
                    /// </summary>
                    /// <param name="beforeState"></param>
                    /// <param name="messages"></param>
                    public CloseResult(int beforeState, TSendMessageInfo[] messages) {
                        this.BeforeState = beforeState;
                        this.Messages = messages;
                    }
                }
                #endregion
            }
            #endregion
        }
        #endregion
    }
}