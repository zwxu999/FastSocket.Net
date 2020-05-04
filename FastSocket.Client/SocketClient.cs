using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Sodao.FastSocket.Client.Protocol;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Messaging;
using Sodao.FastSocket.SocketBase.Protocol;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.Client {

    /// <summary>
    /// socket client
    /// </summary>
    /// <typeparam name="TMessageInfo"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public class SocketClient<TMessageInfo, TMessage> : BaseHost<TMessageInfo, TMessage>, ISyncSocketClient<TMessageInfo, TMessage>
        where TMessageInfo : class, ISyncMessageInfo<TMessage> {
        #region Events
        /// <summary>
        /// received unknow message
        /// </summary>
        public event Action<SocketBase.IConnection, TMessage> ReceivedAsyncMessage;
        #endregion

        #region Private Members
        /// <summary>
        /// 
        /// </summary>
        protected readonly ISyncClientProtocolHandlerFactory<ISyncClientProtocolHandler<TMessageInfo, TMessage>, TMessageInfo, TMessage> _protocolHandlerFactory = null;

        /// <summary>
        /// 
        /// </summary>
        protected readonly int _millisecondsSendTimeout;

        /// <summary>
        /// 
        /// </summary>
        protected readonly int _millisecondsReceiveTimeout;

        /// <summary>
        /// 
        /// </summary>
        public readonly PendingSendQueue _pendingQueue = null;
        /// <summary>
        /// 
        /// </summary>
        public readonly ReceivingQueue<TMessageInfo, TMessage> _receivingQueue = null;

        private readonly EndPointManager<TMessageInfo, TMessage> _endPointManager = null;
        private readonly IConnectionPool<TMessageInfo, TMessage> _connectionPool = null;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="clientProtocolHandlerFactory"></param>
        public SocketClient(ISyncClientProtocolHandlerFactory<ISyncClientProtocolHandler<TMessageInfo, TMessage>, TMessageInfo, TMessage> clientProtocolHandlerFactory)
            : this(clientProtocolHandlerFactory, 8192, 8192, 3000, 3000) {
        }

        /// <summary>
        /// new
        /// </summary>
        /// <param name="clientProtocolHandlerFactory"></param>
        /// <param name="socketBufferSize"></param>
        /// <param name="messageBufferSize"></param>
        /// <param name="millisecondsSendTimeout"></param>
        /// <param name="millisecondsReceiveTimeout"></param>
        /// <exception cref="ArgumentNullException">protocol is null</exception>
        public SocketClient(ISyncClientProtocolHandlerFactory<ISyncClientProtocolHandler<TMessageInfo, TMessage>, TMessageInfo, TMessage> clientProtocolHandlerFactory,
            int socketBufferSize,
            int messageBufferSize,
            int millisecondsSendTimeout,
            int millisecondsReceiveTimeout)
            : base(socketBufferSize, messageBufferSize) {
            this._protocolHandlerFactory = clientProtocolHandlerFactory ?? throw new ArgumentNullException("protocol");

            this._connectionPool = new SyncPool<TMessageInfo, TMessage>();

            this._millisecondsSendTimeout = millisecondsSendTimeout;
            this._millisecondsReceiveTimeout = millisecondsReceiveTimeout;

            this._pendingQueue = new PendingSendQueue(this);
            this._receivingQueue = new ReceivingQueue<TMessageInfo, TMessage>(this);

            this._endPointManager = new EndPointManager<TMessageInfo, TMessage>(this);
            this._endPointManager.Connected += this.OnEndPointConnected;
            this._endPointManager.Already += this.OnEndPointAlready;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// 发送超时毫秒数
        /// </summary>
        public int MillisecondsSendTimeout {
            get { return this._millisecondsSendTimeout; }
        }
        /// <summary>
        /// 接收超时毫秒数
        /// </summary>
        public int MillisecondsReceiveTimeout {
            get { return this._millisecondsReceiveTimeout; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// try register endPoint
        /// </summary>
        /// <param name="name"></param>
        /// <param name="arrRemoteEP"></param>
        /// <param name="initFunc"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">socketClient</exception>
        public bool TryRegisterEndPoint(string name, EndPoint[] arrRemoteEP, Func<IConnection, string, Task> initFunc = null) {
            return this._endPointManager.TryRegister(name, arrRemoteEP, initFunc);
        }
        /// <summary>
        /// un register endPoint
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">socketClient</exception>
        public bool UnRegisterEndPoint(string name) {
            return this._endPointManager.UnRegister(name);
        }
        /// <summary>
        /// get all registered endPoint
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<string, EndPoint[]>[] GetAllRegisteredEndPoint() {
            return this._endPointManager.ToArray();
        }
        /// <summary>
        /// send messageInfo
        /// </summary>
        /// <param name="messageInfo"></param>
        /// <exception cref="ArgumentNullException">messageInfo is null.</exception>
        public void Send(TMessageInfo messageInfo) {
            if (messageInfo == null) throw new ArgumentNullException("messageInfo");

            //messageInfo.AllowRetry = true;
            if (this._connectionPool.TryAcquire(out var connection) && connection.Active) {
                connection.BeginSendMessage(messageInfo);
                return;
            }
            this._pendingQueue.Enqueue(messageInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public void TryGetConnection2SendNext(Func<TMessageInfo> func) {
            if (_connectionPool.TryAcquire(out var connection)) {
                var messageInfo = func();
                if (messageInfo == null) {
                    _connectionPool.Release(connection);
                } else {
                    messageInfo.SetProcess(SendMessageInfoProcess.Dequeue);
                    connection.BeginSendMessage(messageInfo);
                }
            }
        }

        /// <summary>
        /// send message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="allowRetry"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">message is null.</exception>
        public Task<bool> Send(TMessage message, bool allowRetry = true) {
            if (message == null) throw new ArgumentNullException("message");

            var messageInfo = this._protocolHandlerFactory.CreateClientAsyncMessageInfo(message, allowRetry);
            if (!this._connectionPool.TryAcquire(out var connection)) {
                this._pendingQueue.Enqueue(messageInfo);
                messageInfo.SetProcess(SendMessageInfoProcess.Enqueue);
            } else {
                connection.BeginSendMessage(messageInfo);
            }
            return messageInfo.Task;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="millisecondsReceiveTimeout"></param>
        /// <param name="allowRetry"></param>
        /// <param name="onException"></param>
        /// <param name="onResult"></param>
        /// <returns></returns>
        public Task<bool> SendSync(TMessage message, Action<Exception> onException, Action<TMessage> onResult, int millisecondsReceiveTimeout = -1, bool allowRetry = true) {
            if (message == null) throw new ArgumentNullException("message");

            var messageInfo = this._protocolHandlerFactory.CreateClientSyncMessageInfo(message, (millisecondsReceiveTimeout > 0 ? millisecondsReceiveTimeout : _millisecondsReceiveTimeout), onException, onResult);
            this._pendingQueue.Enqueue(messageInfo);
            messageInfo.SetProcess(SendMessageInfoProcess.Enqueue);
            TrySendNext();
            return messageInfo.Task;
        }

        #endregion

        #region Protected Methods
        /// <summary>
        /// try send next messageInfo
        /// </summary>
        protected void TrySendNext() {
            //if (this._pendingQueue.TryDequeue(out var messageInfo)) this.Send(messageInfo);
            TryGetConnection2SendNext(() => _pendingQueue.TryDequeue(out var messageInfo) ? messageInfo : null);
        }
        /// <summary>
        /// endPoint connected
        /// </summary>
        /// <param name="name"></param>
        /// <param name="connection"></param>
        protected virtual void OnEndPointConnected(string name, SocketBase.IConnection<TMessageInfo, TMessage> connection) {
            connection.SetProtoHandler(this._protocolHandlerFactory.CreateProtocolHandler(connection));
            base.RegisterConnection(connection);
        }
        /// <summary>
        /// endPoint already available
        /// </summary>
        /// <param name="name"></param>
        /// <param name="connection"></param>
        protected virtual void OnEndPointAlready(string name, SocketBase.IConnection<TMessageInfo, TMessage> connection) {
            this._connectionPool.Register(connection);
        }
        /// <summary>
        /// on pending send timeout
        /// </summary>
        /// <param name="messageInfo"></param>
        public virtual void OnPendingSendTimeout(TMessageInfo messageInfo) {
            ThreadPool.QueueUserWorkItem(_ => {
                try { messageInfo.SetSendException(new MessageException(MessageException.Errors.PendingSendTimeout)); } catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
            });
        }
        /// <summary>
        /// on messageInfo sent
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="messageInfo"></param>
        protected virtual void OnSent(IConnection connection, TMessageInfo messageInfo) {
        }
        /// <summary>
        /// on send failed
        /// </summary>
        /// <param name="messageInfo"></param>
        protected virtual void OnSendFailed(TMessageInfo messageInfo) {
            ThreadPool.QueueUserWorkItem(_ => {
                try { messageInfo.SetSendException(new MessageException(MessageException.Errors.SendFaild)); } catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
            });
        }

        /// <summary>
        /// on message received
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="messageInfo"></param>
        /// <param name="message"></param>
        protected virtual void OnReceived(IConnection<TMessageInfo, TMessage> connection, TMessageInfo messageInfo, TMessage message) {
            //ThreadPool.QueueUserWorkItem(_ => {
            try { messageInfo.SetRecviceResult(message); } catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
            //});
        }
        /// <summary>
        /// on received unknow message
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="message"></param>
        protected virtual void OnReceivedAsyncMessage(IConnection connection, TMessage message) {
            this.ReceivedAsyncMessage?.Invoke(connection, message);
        }
        /// <summary>
        /// on receive timeout
        /// </summary>
        /// <param name="messageInfo"></param>
        public virtual void OnReceiveTimeout(TMessageInfo messageInfo) {
            //ThreadPool.QueueUserWorkItem(_ => {
            try { messageInfo.SetReceiveException(new MessageException(MessageException.Errors.ReceiveTimeout, messageInfo.SeqID)); } catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
            //});
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// OnConnected
        /// </summary>
        /// <param name="connection"></param>
        protected override void OnConnected(IConnection connection) {
            base.OnConnected(connection);
            connection.BeginReceive();//异步开始接收数据
        }
        /// <summary>
        /// on disconnected
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        protected override void OnDisconnected(IConnection<TMessageInfo, TMessage> connection, Exception ex) {
            base.OnDisconnected(connection, ex);
            this._connectionPool.Destroy(connection);
        }
        /// <summary>
        /// OnStartSending
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="messageInfo"></param>
        protected override void OnStartSending(IConnection connection, TMessageInfo messageInfo) {
            //messageInfo.SendConnection = connection;
            if (messageInfo.IsSync) {
                this._receivingQueue.TryAdd(connection, messageInfo);
            }
        }
        /// <summary>
        /// OnSendCallback
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="messageInfo"></param>
        /// <param name="isSuccess"></param>
        protected override void OnSendCallback(IConnection<TMessageInfo, TMessage> connection, TMessageInfo messageInfo, bool isSuccess) {
            if (messageInfo == null) return;

            if (isSuccess) {
                //if (messageInfo.IsSync) {
                //    this._receivingQueue.TryAdd(connection, messageInfo);
                //}
                messageInfo.SentTime = SocketBase.Utils.Date.Now;
                messageInfo.SetProcess(SendMessageInfoProcess.SetSendResult);
                messageInfo.SetSendResult(isSuccess);
                this.OnSent(connection, messageInfo);
                this._connectionPool.Release(connection);
                return;
            }

            if (!isSuccess && messageInfo.IsSync && this._receivingQueue.TryRemove(connection.ConnectionID, messageInfo.SeqID, out var removed)) {

            }


            if (!messageInfo.AllowRetry) {
                messageInfo.SetProcess(SendMessageInfoProcess.SetSendResult);
                messageInfo.SetSendResult(isSuccess);
                this.OnSendFailed(messageInfo);
                return;
            }

            if (SocketBase.Utils.Date.Now.Subtract(messageInfo.CreatedTime).TotalMilliseconds > this._millisecondsSendTimeout) {
                messageInfo.SetProcess(SendMessageInfoProcess.SetSendResult);
                messageInfo.SetSendResult(isSuccess);
                //send time out
                this.OnPendingSendTimeout(messageInfo);
                return;
            }

            messageInfo.SetProcess(SendMessageInfoProcess.Retry);
            //retry send
            this.Send(messageInfo);
        }
        /// <summary>
        /// OnMessageReceived
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="e"></param>
        protected override void OnMessageReceived(IConnection<TMessageInfo, TMessage> connection, MessageReceivedEventArgs<IRecvivedMessageInfo<TMessage>> e) {

            var recvMessageInfo = e.MessageInfo;
            if (recvMessageInfo.IsSync && this._receivingQueue.TryRemove(connection.ConnectionID, recvMessageInfo.SeqID, out var messageInfo)) {
                this.OnReceived(connection, messageInfo, recvMessageInfo.Message);
            } else {
                this.OnReceivedAsyncMessage(connection, recvMessageInfo.Message);
            }
        }
        /// <summary>
        /// stop
        /// </summary>
        public override void Start() {
            this._endPointManager.Start();
        }
        /// <summary>
        /// stop
        /// </summary>
        public override void Stop() {
            this._endPointManager.Stop();
            base.Stop();
        }
        #endregion

        /// <summary>
        /// send queue
        /// </summary>
        public class PendingSendQueue : ConcurrentQueue<TMessageInfo> {
            #region Private Members
            private readonly SocketClient<TMessageInfo, TMessage> _client = null;
            private readonly Timer _timer = null;
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            /// <param name="client"></param>
            public PendingSendQueue(SocketClient<TMessageInfo, TMessage> client) {
                this._client = client;

                this._timer = new Timer(state => {
                    //var count = this.Count;
                    //if (count == 0) return;

                    this._timer.Change(Timeout.Infinite, Timeout.Infinite);

                    var timeOut = this._client.MillisecondsSendTimeout;
                    while (this.Count > 0) {
                        this._client.TryGetConnection2SendNext(() => {
                            while (this.TryDequeue(out var messageInfo)) {
                                var dtNow = SocketBase.Utils.Date.Now;
                                if (dtNow.Subtract(messageInfo.CreatedTime).TotalMilliseconds < timeOut) {
                                    //try send...                                
                                    return messageInfo;//this._client.Send(messageInfo);
                                } else {

                                    //fire send time out
                                    this._client.OnPendingSendTimeout(messageInfo);
                                }
                            }
                            return null;
                        }
                        );
                    }

                    this._timer.Change(500, 500);
                }, null, 500, 500);
            }
            #endregion


        }
    }


    /// <summary>
    /// node info
    /// </summary>
    internal class NodeInfo {
        #region Members
        /// <summary>
        /// name
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// remote endPoint array
        /// </summary>
        public readonly EndPoint[] ArrRemoteEP;
        /// <summary>
        /// init function
        /// </summary>
        public readonly Func<IConnection, string, Task> InitFunc;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="name"></param>
        /// <param name="arrRemoteEP"></param>
        /// <param name="initFunc"></param>
        /// <exception cref="ArgumentNullException">name is null or empty</exception>
        /// <exception cref="ArgumentNullException">arrRemoteEP is null or empty</exception>
        public NodeInfo(string name, EndPoint[] arrRemoteEP, Func<IConnection, string, Task> initFunc) {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (arrRemoteEP == null || arrRemoteEP.Length == 0) throw new ArgumentNullException("arrRemoteEP");

            this.Name = name;
            this.ArrRemoteEP = arrRemoteEP;
            this.InitFunc = initFunc;
        }
        #endregion
    }

    /// <summary>
    /// server node
    /// </summary>
    internal class Node<TClientMessageInfo, TMessage> : IDisposable where TClientMessageInfo : IAsyncMessageInfo<TMessage> {
        #region Members
        static private int NODE_ID = 0;

        private readonly IHost<TClientMessageInfo, TMessage> _host = null;
        private readonly Action<Node<TClientMessageInfo, TMessage>, IConnection<TClientMessageInfo, TMessage>> _connectedCallback;
        private readonly Action<Node<TClientMessageInfo, TMessage>, IConnection<TClientMessageInfo, TMessage>> _alreadyCallback;

        private bool _isdisposed = false;
        private IConnection _connection = null;

        /// <summary>
        /// id
        /// </summary>
        public readonly int ID;
        /// <summary>
        /// node info
        /// </summary>
        public readonly NodeInfo Info;
        #endregion

        #region Constructors
        /// <summary>
        /// free
        /// </summary>
        ~Node() {
            this.Dispose();
        }
        /// <summary>
        /// new
        /// </summary>
        /// <param name="info"></param>
        /// <param name="host"></param>
        /// <param name="connectedCallback"></param>
        /// <param name="alreadyCallback"></param>
        public Node(NodeInfo info, IHost<TClientMessageInfo, TMessage> host,
            Action<Node<TClientMessageInfo, TMessage>, IConnection<TClientMessageInfo, TMessage>> connectedCallback,
            Action<Node<TClientMessageInfo, TMessage>, IConnection<TClientMessageInfo, TMessage>> alreadyCallback) {
            this.ID = Interlocked.Increment(ref NODE_ID);
            this.Info = info ?? throw new ArgumentNullException("info");
            this._host = host ?? throw new ArgumentNullException("host");
            this._connectedCallback = connectedCallback ?? throw new ArgumentNullException("connectedCallback");
            this._alreadyCallback = alreadyCallback ?? throw new ArgumentNullException("alreadyCallback");

            this.Connect();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// begin connect
        /// </summary>
        private void Connect() {
            SocketConnector.Connect(this.Info.ArrRemoteEP.Length == 1 ?
                this.Info.ArrRemoteEP[0] :
                this.Info.ArrRemoteEP[(Guid.NewGuid().GetHashCode() & int.MaxValue) % this.Info.ArrRemoteEP.Length])
                .ContinueWith(t => this.ConnectCallback(t));
        }
        /// <summary>
        /// connect callback
        /// </summary>
        /// <param name="t"></param>
        private void ConnectCallback(Task<Socket> t) {
            if (t.IsFaulted) {
                lock (this) { if (this._isdisposed) return; }
                SocketBase.Utils.TaskEx.Delay(new Random().Next(500, 1500)).ContinueWith(_ => this.Connect());
                return;
            }

            var connection = this._host.NewConnection(t.Result);
            connection.Disconnected += (conn, ex) => {
                lock (this) {
                    this._connection = null;
                    if (this._isdisposed) return;
                }
                SocketBase.Utils.TaskEx.Delay(new Random().Next(100, 1000)).ContinueWith(_ => this.Connect());
            };

            //fire node connected event.
            this._connectedCallback(this, connection);

            if (this.Info.InitFunc == null) {
                lock (this) {
                    if (this._isdisposed) {
                        connection.BeginDisconnect();
                        return;
                    }
                    this._connection = connection;
                }
                //fire node already event.
                this._alreadyCallback(this, connection);
                return;
            }

            this.Info.InitFunc(connection, this.Info.Name).ContinueWith(c => {
                if (c.IsFaulted) {
                    connection.BeginDisconnect(c.Exception.InnerException);
                    return;
                }

                lock (this) {
                    if (this._isdisposed) {
                        connection.BeginDisconnect();
                        return;
                    }
                    this._connection = connection;
                }
                //fire node already event.
                this._alreadyCallback(this, connection);
            });
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// dispose
        /// </summary>
        public void Dispose() {
            IConnection exists = null;
            lock (this) {
                if (this._isdisposed) return;
                this._isdisposed = true;

                exists = this._connection;
                this._connection = null;
            }
            if (exists != null) exists.BeginDisconnect();
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    /// <summary>
    /// endPoint manager
    /// </summary>
    internal class EndPointManager<TMessageInfo, TMessage> where TMessageInfo : IAsyncMessageInfo<TMessage> {
        #region Events
        /// <summary>
        /// node connected event
        /// </summary>
        public event Action<string, IConnection<TMessageInfo, TMessage>> Connected;
        /// <summary>
        /// node already event
        /// </summary>
        public event Action<string, IConnection<TMessageInfo, TMessage>> Already;
        #endregion

        #region Members
        /// <summary>
        /// host
        /// </summary>
        private readonly IHost<TMessageInfo, TMessage> _host = null;
        /// <summary>
        /// key:node name
        /// </summary>
        private readonly Dictionary<string, NodeInfo> _dicNodeInfo = new Dictionary<string, NodeInfo>();
        /// <summary>
        /// key:node id
        /// </summary>
        private readonly Dictionary<int, Node<TMessageInfo, TMessage>> _dicNodes = new Dictionary<int, Node<TMessageInfo, TMessage>>();
        /// <summary>
        /// true is runing
        /// </summary>
        private bool _isRuning = true;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="host"></param>
        public EndPointManager(IHost<TMessageInfo, TMessage> host) {
            this._host = host;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// try register
        /// </summary>
        /// <param name="name"></param>
        /// <param name="arrRemoteEP"></param>
        /// <param name="initFunc"></param>
        /// <returns></returns>
        public bool TryRegister(string name, EndPoint[] arrRemoteEP, Func<IConnection, string, Task> initFunc) {
            lock (this) {
                if (this._dicNodeInfo.ContainsKey(name)) return false;
                var nodeInfo = new NodeInfo(name, arrRemoteEP, initFunc);
                this._dicNodeInfo[name] = nodeInfo;

                if (this._isRuning) {
                    var node = new Node<TMessageInfo, TMessage>(nodeInfo, this._host, this.OnNodeConnected, this.OnNodeAlready);
                    this._dicNodes[node.ID] = node;
                }
                return true;
            }
        }
        /// <summary>
        /// un register
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool UnRegister(string name) {
            KeyValuePair<int, Node<TMessageInfo, TMessage>>[] arrRemoved = null;
            lock (this) {
                if (!this._dicNodeInfo.Remove(name)) return false;
                arrRemoved = this._dicNodes.Where(c => c.Value.Info.Name == name).ToArray();
                foreach (var child in arrRemoved) {
                    this._dicNodes.Remove(child.Key);
                }
            }
            if (arrRemoved != null)
                foreach (var child in arrRemoved) child.Value.Dispose();

            return true;
        }
        /// <summary>
        /// to array
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<string, EndPoint[]>[] ToArray() {
            lock (this) {
                return this._dicNodeInfo.Values.Select(c => new KeyValuePair<string, EndPoint[]>(c.Name, c.ArrRemoteEP)).ToArray();
            }
        }
        /// <summary>
        /// start
        /// </summary>
        public void Start() {
            lock (this) {
                if (this._isRuning) return;
                this._isRuning = true;
                foreach (var info in this._dicNodeInfo.Values) {
                    var node = new Node<TMessageInfo, TMessage>(info, this._host, this.OnNodeConnected, this.OnNodeAlready);
                    this._dicNodes[node.ID] = node;
                }
            }
        }
        /// <summary>
        /// stop
        /// </summary>
        public void Stop() {
            Node<TMessageInfo, TMessage>[] arrNodes = null;
            lock (this) {
                if (!this._isRuning) return;
                this._isRuning = false;
                arrNodes = this._dicNodes.Values.ToArray();
                this._dicNodes.Clear();
            }
            if (arrNodes == null || arrNodes.Length == 0) return;
            foreach (var node in arrNodes) node.Dispose();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// on node connected
        /// </summary>
        /// <param name="node"></param>
        /// <param name="connection"></param>
        private void OnNodeConnected(Node<TMessageInfo, TMessage> node, IConnection<TMessageInfo, TMessage> connection) {
            this.Connected?.Invoke(node.Info.Name, connection);
        }
        /// <summary>
        /// on node already
        /// </summary>
        /// <param name="node"></param>
        /// <param name="connection"></param>
        private void OnNodeAlready(Node<TMessageInfo, TMessage> node, IConnection<TMessageInfo, TMessage> connection) {
            this.Already?.Invoke(node.Info.Name, connection);
        }
        #endregion
    }

    /// <summary>
    /// connection pool interface
    /// </summary>
    internal interface IConnectionPool<TMessageInfo, TMessage> where TMessageInfo : ISendMessageInfo<TMessage> {
        #region Public Methods
        /// <summary>
        /// register
        /// </summary>
        /// <param name="connection"></param>
        void Register(IConnection<TMessageInfo, TMessage> connection);
        /// <summary>
        /// try acquire <see cref="IConnection"/>
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        bool TryAcquire(out IConnection<TMessageInfo, TMessage> connection);
        /// <summary>
        /// release
        /// </summary>
        /// <param name="connection"></param>
        void Release(IConnection<TMessageInfo, TMessage> connection);
        /// <summary>
        /// destroy
        /// </summary>
        /// <param name="connection"></param>
        void Destroy(IConnection<TMessageInfo, TMessage> connection);
        #endregion
    }

    /// <summary>
    /// sync connection pool
    /// </summary>
    public sealed class SyncPool<TMessageInfo, TMessage> : IConnectionPool<TMessageInfo, TMessage> where TMessageInfo : ISendMessageInfo<TMessage> {
        #region Private Members
        private readonly ConcurrentDictionary<long, IConnection> _dic =
            new ConcurrentDictionary<long, IConnection>();
        private readonly ConcurrentStack<IConnection<TMessageInfo, TMessage>> _stack =
            new ConcurrentStack<IConnection<TMessageInfo, TMessage>>();
        #endregion

        #region Public Methods
        /// <summary>
        /// register
        /// </summary>
        /// <param name="connection"></param>
        public void Register(IConnection<TMessageInfo, TMessage> connection) {
            if (this._dic.TryAdd(connection.ConnectionID, connection))
                this._stack.Push(connection);
        }
        /// <summary>
        /// try acquire
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public bool TryAcquire(out IConnection<TMessageInfo, TMessage> connection) {
            return this._stack.TryPop(out connection);
        }
        /// <summary>
        /// release
        /// </summary>
        /// <param name="connection"></param>
        public void Release(IConnection<TMessageInfo, TMessage> connection) {
            if (this._dic.ContainsKey(connection.ConnectionID))
                this._stack.Push(connection);
        }
        /// <summary>
        /// destroy
        /// </summary>
        /// <param name="connection"></param>
        public void Destroy(IConnection<TMessageInfo, TMessage> connection) {
            this._dic.TryRemove(connection.ConnectionID, out IConnection exists);
        }
        #endregion
    }
}