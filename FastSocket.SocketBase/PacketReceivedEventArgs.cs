using System;

namespace Sodao.FastSocket.SocketBase {
    /// <summary>
    /// packet received eventArgs
    /// </summary>
    public sealed class PacketReceivedEventArgs {
        /// <summary>
        /// process callback
        /// </summary>
        private readonly PacketProcessHandler _processCallback = null;
        /// <summary>
        /// Buffer
        /// </summary>
        public readonly ArraySegment<byte> Buffer;

        /// <summary>
        /// new
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="processCallback"></param>
        /// <exception cref="ArgumentNullException">processCallback is null</exception>
        public PacketReceivedEventArgs(ArraySegment<byte> buffer, PacketProcessHandler processCallback) {
            this.Buffer = buffer;
            this._processCallback = processCallback ?? throw new ArgumentNullException("processCallback");
        }

        /// <summary>
        /// 设置已读取长度
        /// </summary>
        /// <param name="readlength"></param>
        public void SetReadlength(int readlength) {
            this._processCallback(this.Buffer, readlength);
        }
    }
}