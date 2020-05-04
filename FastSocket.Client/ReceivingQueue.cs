using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sodao.FastSocket.Client.Protocol;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Messaging;
using Sodao.FastSocket.SocketBase.Protocol;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.Client {

    /// <summary>
    /// receiving queue
    /// </summary>
    public class ReceivingQueue<TClientMessageInfo, TMessage> : ConcurrentDictionary<string, TClientMessageInfo>/*ConcurrentDictionary<long, ConcurrentDictionary<int, TClientMessageInfo>>*/
    where TClientMessageInfo : ISyncMessageInfo<TMessage> {
        #region Private Members
        /// <summary>
        /// socket client
        /// </summary>
        private readonly ISyncSocketClient<TClientMessageInfo, TMessage> _client = null;
        /// <summary>
        /// timer for check receive timeout
        /// </summary>
        private readonly Timer _timer = null;

        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="client"></param>
        public ReceivingQueue(ISyncSocketClient<TClientMessageInfo, TMessage> client) {
            this._client = client;
            //ResetWriters();
            this._timer = new Timer(_ => {
                if (this.Count == 0) return;
                this._timer.Change(Timeout.Infinite, Timeout.Infinite);
                var dtNow = SocketBase.Utils.Date.Now;

                var arr = this.ToArray().Where(c => c.Value.SentTime != DateTime.MinValue &&
                    dtNow.Subtract(c.Value.SentTime).TotalMilliseconds > c.Value.MillisecondsReceiveTimeout).ToArray();

                for (int i = 0, l = arr.Length; i < l; i++) {
                    var key = arr[i].Key;
                    if (this.TryRemove(key, out var messageInfo)) {
                        var seg = key.Split('/');
                        this._client.OnReceiveTimeout(messageInfo);
                    }
                }

                this._timer.Change(500, 500);
            }, null, 500, 500);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// to key
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="messageInfo"></param>
        /// <returns></returns>
        private string ToKey(IConnection connection, TClientMessageInfo messageInfo) {
            //if (messageInfo.SendConnection == null) throw new ArgumentNullException("messageInfo.SendConnection");
            return this.ToKey(/*messageInfo.Send*/connection.ConnectionID, messageInfo.SeqID);
        }
        /// <summary>
        /// to key
        /// </summary>
        /// <param name="connectionID"></param>
        /// <param name="seqID"></param>
        /// <returns></returns>
        private string ToKey(long connectionID, int seqID) {
            return string.Concat(connectionID.ToString(), "/", seqID.ToString());
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// try add
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="messageInfo"></param>
        /// <returns></returns>
        public bool TryAdd(IConnection connection, TClientMessageInfo messageInfo) {
            var succ = this.TryAdd(this.ToKey(connection, messageInfo), messageInfo);
            var log = messageInfo.Message.ToString();
            return succ;
            //var collection = this.GetOrAdd(connection.ConnectionID, p => new ConcurrentDictionary<int, TClientMessageInfo>());
            //return collection.TryAdd(messageInfo.SeqID, messageInfo);
        }

        /// <summary>
        /// try remove
        /// </summary>
        /// <param name="connectionID"></param>
        /// <param name="seqID"></param>
        /// <param name="messageInfo"></param>
        /// <returns></returns>
        public bool TryRemove(long connectionID, int seqID, out TClientMessageInfo messageInfo) {

            var succ = this.TryRemove(this.ToKey(connectionID, seqID), out messageInfo);
            return succ;
            //return TryGetValue(connectionID, out var collection) && collection.TryRemove(seqID, out messageInfo);
        }

        #endregion
    }


    /// <summary>
    /// async connection pool
    /// </summary>
    public sealed class AsyncPool<TMessageInfo, TMessage> : IConnectionPool<TMessageInfo, TMessage> where TMessageInfo : ISendMessageInfo<TMessage> {
        #region Private Members
        private readonly List<IConnection<TMessageInfo, TMessage>> _list = new List<IConnection<TMessageInfo, TMessage>>();
        private IConnection<TMessageInfo, TMessage>[] _arr = null;
        private int _acquireNumber = 0;
        #endregion

        #region Public Methods
        /// <summary>
        /// register
        /// </summary>
        /// <param name="connection"></param>
        public void Register(IConnection<TMessageInfo, TMessage> connection) {
            if (connection == null) throw new ArgumentNullException("connection");

            lock (this) {
                if (this._list.Contains(connection)) return;

                this._list.Add(connection);
                this._arr = this._list.ToArray();
            }
        }
        /// <summary>
        /// try acquire
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public bool TryAcquire(out IConnection<TMessageInfo, TMessage> connection) {
            var arr = this._arr;
            if (arr == null || arr.Length == 0) {
                connection = null;
                return false;
            }

            if (arr.Length == 1) connection = arr[0];
            else connection = arr[(Interlocked.Increment(ref this._acquireNumber) & 0x7fffffff) % arr.Length];
            return true;
        }
        /// <summary>
        /// release
        /// </summary>
        /// <param name="connection"></param>
        public void Release(IConnection<TMessageInfo, TMessage> connection) {

        }
        /// <summary>
        /// destroy
        /// </summary>
        /// <param name="connection"></param>
        public void Destroy(IConnection<TMessageInfo, TMessage> connection) {
            if (connection == null) throw new ArgumentNullException("connection");

            lock (this) {
                if (this._list.Remove(connection)) this._arr = this._list.ToArray();
            }
        }
        #endregion
    }
}