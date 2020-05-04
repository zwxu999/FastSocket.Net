using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Sodao.FastSocket.SocketBase {
    /// <summary>
    /// 消息处理handler
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="readlength"></param>
    public delegate void PacketProcessHandler(ArraySegment<byte> buffer, int readlength);

}