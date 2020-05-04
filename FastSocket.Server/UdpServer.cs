using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// upd server
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public sealed class UdpServer<TMessage> : IUdpServer<TMessage>
    {
        #region Private Members
        private readonly int _port;
        private readonly int _messageBufferSize;

        private Socket _socket = null;
        private AsyncSendPool _pool = null;

        private readonly IUdpProtocolHandler<TMessage> _protocolHandler = null;
        private readonly IUdpService<TMessage> _service = null;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="port"></param>
        /// <param name="_protocolHandler"></param>
        /// <param name="service"></param>
        public UdpServer(int port, IUdpProtocolHandler<TMessage> _protocolHandler,
            IUdpService<TMessage> service)
            : this(port, 2048, _protocolHandler, service)
        {
        }
        /// <summary>
        /// new
        /// </summary>
        /// <param name="port"></param>
        /// <param name="messageBufferSize"></param>
        /// <param name="_protocolHandler"></param>
        /// <param name="service"></param>
        /// <exception cref="ArgumentNullException">protocol is null.</exception>
        /// <exception cref="ArgumentNullException">service is null.</exception>
        public UdpServer(int port, int messageBufferSize,
            IUdpProtocolHandler<TMessage> _protocolHandler,
            IUdpService<TMessage> service)
        {
            this._port = port;
            this._messageBufferSize = messageBufferSize;
            this._protocolHandler = _protocolHandler ?? throw new ArgumentNullException("protocol");
            this._service = service ?? throw new ArgumentNullException("service");
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 异步接收数据
        /// </summary>
        /// <param name="e"></param>
        private void BeginReceive(SocketAsyncEventArgs e)
        {
            if (!this._socket.ReceiveFromAsync(e))
                this.ReceiveCompleted(this, e);//ThreadPool.QueueUserWorkItem(_ => )
        }
        /// <summary>
        /// completed handle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                var session = new UdpSession(e.RemoteEndPoint, this);
                TMessage message;
                try {
                    if (this._protocolHandler.TryParse(new ArraySegment<byte>(e.Buffer, 0, e.BytesTransferred), out message)) {
                        this._service.OnReceived(session, message);
                    }
                } catch (Exception ex)
                {
                    SocketBase.Log.Trace.Error(ex.Message, ex);
                    this._service.OnError(session, ex);
                }
            }

            //receive again
            this.BeginReceive(e);
        }
        #endregion

        #region IUdpServer Members
        /// <summary>
        /// start
        /// </summary>
        public void Start()
        {
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this._socket.Bind(new IPEndPoint(IPAddress.Any, this._port));
            this._socket.DontFragment = true;

            this._pool = new AsyncSendPool(this._messageBufferSize, this._socket);

            var e = new SocketAsyncEventArgs();
            e.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            e.SetBuffer(new byte[this._messageBufferSize], 0, this._messageBufferSize);
            e.Completed += this.ReceiveCompleted;
            this.BeginReceive(e);
        }
        /// <summary>
        /// stop
        /// </summary>
        public void Stop()
        {
            this._socket.Close();
            this._socket = null;
            this._pool = null;
        }
        /// <summary>
        /// send to...
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="payload"></param>
        public void SendTo(EndPoint endPoint, byte[] payload)
        {
            this._pool.SendAsync(endPoint, payload);
        }
        #endregion

        /// <summary>
        /// 用于异步发送的<see cref="SocketAsyncEventArgs"/>对象池
        /// </summary>
        private class AsyncSendPool
        {
            #region Private Members
            private const int MAXPOOLSIZE = 3000;
            private readonly int _messageBufferSize;
            private readonly Socket _socket = null;
            private readonly ConcurrentStack<SocketAsyncEventArgs> _stack =
                new ConcurrentStack<SocketAsyncEventArgs>();
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            /// <param name="messageBufferSize"></param>
            /// <param name="socket"></param>
            public AsyncSendPool(int messageBufferSize, Socket socket)
            {
                this._messageBufferSize = messageBufferSize;
                this._socket = socket ?? throw new ArgumentNullException("socket");
            }
            #endregion

            #region Private Methods
            /// <summary>
            /// send completed handle
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void SendCompleted(object sender, SocketAsyncEventArgs e)
            {
                this.Release(e);
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// acquire
            /// </summary>
            /// <returns></returns>
            public SocketAsyncEventArgs Acquire()
            {
                SocketAsyncEventArgs e;
                if (this._stack.TryPop(out e)) return e;

                e = new SocketAsyncEventArgs();
                e.SetBuffer(new byte[this._messageBufferSize], 0, this._messageBufferSize);
                e.Completed += this.SendCompleted;
                return e;
            }
            /// <summary>
            /// release
            /// </summary>
            /// <param name="e"></param>
            public void Release(SocketAsyncEventArgs e)
            {
                if (this._stack.Count >= MAXPOOLSIZE)
                {
                    e.Completed -= this.SendCompleted;
                    e.Dispose();
                    return;
                }

                this._stack.Push(e);
            }
            /// <summary>
            /// sned async
            /// </summary>
            /// <param name="endPoint"></param>
            /// <param name="payload"></param>
            /// <exception cref="ArgumentNullException">endPoint is null</exception>
            /// <exception cref="ArgumentNullException">payload is null or empty</exception>
            /// <exception cref="ArgumentOutOfRangeException">payload length大于messageBufferSize</exception>
            public void SendAsync(EndPoint endPoint, byte[] payload)
            {
                if (payload == null || payload.Length == 0) throw new ArgumentNullException("payload");
                if (payload.Length > this._messageBufferSize)
                    throw new ArgumentOutOfRangeException("payload.Length", "payload length大于messageBufferSize");

                var e = this.Acquire();
                e.RemoteEndPoint = endPoint ?? throw new ArgumentNullException("endPoint");

                Buffer.BlockCopy(payload, 0, e.Buffer, 0, payload.Length);
                e.SetBuffer(0, payload.Length);

                if (!this._socket.SendToAsync(e))
                     this.Release(e);//ThreadPool.QueueUserWorkItem(_ =>)
            }
            #endregion
        }
    }
}