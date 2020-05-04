using System;
using System.Net;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Protocol;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.Server 
{
    /// <summary>
    /// socket server.
    /// </summary>
    /// <typeparam name="TMessageInfo"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public class SocketServer<TMessageInfo, TMessage> : SocketBase.BaseHost<TMessageInfo, TMessage> 
        where TMessageInfo: class, ISendMessageInfo<TMessage> {
        #region Private Members
        private readonly SocketListener<TMessageInfo, TMessage> _listener = null;
        private readonly ISocketService<TMessage> _socketService = null;
        private readonly IServerProtocolHandlerFactory<TMessageInfo, TMessage> _protocolHandlerFactory = null;
        private readonly int _maxMessageSize;
        private readonly int _maxConnections;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="port"></param>
        /// <param name="socketService"></param>
        /// <param name="protocolHandlerFactory"></param>
        /// <param name="socketBufferSize"></param>
        /// <param name="messageBufferSize"></param>
        /// <param name="maxMessageSize"></param>
        /// <param name="maxConnections"></param>
        /// <exception cref="ArgumentNullException">socketService is null.</exception>
        /// <exception cref="ArgumentNullException">protocol is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">maxMessageSize</exception>
        /// <exception cref="ArgumentOutOfRangeException">maxConnections</exception>
        public SocketServer(int port,
            ISocketService<TMessage> socketService,
            IServerProtocolHandlerFactory<TMessageInfo, TMessage> protocolHandlerFactory,
            int socketBufferSize,
            int messageBufferSize,
            int maxMessageSize,
            int maxConnections)
            : base(socketBufferSize, messageBufferSize) 
        {
            if (maxMessageSize < 1) throw new ArgumentOutOfRangeException("maxMessageSize");
            if (maxConnections < 1) throw new ArgumentOutOfRangeException("maxConnections");

            this._socketService = socketService ?? throw new ArgumentNullException("socketService");
            this._protocolHandlerFactory = protocolHandlerFactory ?? throw new ArgumentNullException("protocol");
            this._maxMessageSize = maxMessageSize;
            this._maxConnections = maxConnections;

            this._listener = new SocketListener<TMessageInfo, TMessage>(new IPEndPoint(IPAddress.Any, port), this);
            this._listener.Accepted += this.OnAccepted;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// socket accepted handler
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="connection"></param>
        private void OnAccepted(ISocketListener<TMessageInfo, TMessage> listener, SocketBase.IConnection<TMessageInfo, TMessage> connection) 
        {
            if (base.CountConnection() < this._maxConnections) 
            {
                connection.SetProtoHandler(_protocolHandlerFactory.CreateProtocolHandler());
                base.RegisterConnection(connection);
                return;
            }

            SocketBase.Log.Trace.Info("too many connections.");
            connection.BeginDisconnect();
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// start
        /// </summary>
        public override void Start() 
        {
            base.Start();
            this._listener.Start();
        }
        /// <summary>
        /// stop
        /// </summary>
        public override void Stop() 
        {
            this._listener.Stop();
            base.Stop();
        }
        /// <summary>
        /// OnConnected
        /// </summary>
        /// <param name="connection"></param>
        protected override void OnConnected(SocketBase.IConnection connection) 
        {
            base.OnConnected(connection);
            this._socketService.OnConnected(connection);
        }
        /// <summary>
        /// send callback
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="messageInfo"></param>
        /// <param name="isSuccess"></param>
        protected override void OnSendCallback(SocketBase.IConnection<TMessageInfo,TMessage> connection,
            TMessageInfo messageInfo, bool isSuccess) 
        {
            messageInfo.SetProcess(SendMessageInfoProcess.SetSendResult);
            messageInfo.SetSendResult(isSuccess);
            this._socketService.OnSendCallback(connection, messageInfo.Message, isSuccess);
        }
        /// <summary>
        /// OnMessageReceived
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="e"></param>
        protected override void OnMessageReceived(SocketBase.IConnection<TMessageInfo, TMessage> connection,
            SocketBase.MessageReceivedEventArgs<IRecvivedMessageInfo<TMessage>> e) 
        {
            this._socketService.OnReceived(connection, e.MessageInfo);
        }
        /// <summary>
        /// OnDisconnected
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        protected override void OnDisconnected(SocketBase.IConnection<TMessageInfo, TMessage> connection, Exception ex) 
        {
            base.OnDisconnected(connection, ex);
            this._socketService.OnDisconnected(connection, ex);
        }
        /// <summary>
        /// on connection error
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        protected override void OnConnectionError(SocketBase.IConnection connection, Exception ex) 
        {
            base.OnConnectionError(connection, ex);
            this._socketService.OnException(connection, ex);
        }
        #endregion
    }
}