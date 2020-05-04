using System;
using System.Threading.Tasks;
using Sodao.FastSocket.SocketBase.Messaging;

namespace Sodao.FastSocket.SocketBase.Protocol.Abstractions {

    /// <summary>
    /// tcp协议接口
    /// </summary>
    /// <typeparam name="TSendMessageInfo"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public interface IProtocolHandler<TSendMessageInfo, TMessage> : IDisposable
        where TSendMessageInfo : ISendMessageInfo<TMessage> {


        /// <summary>
        /// CreateMessageInfo
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        TSendMessageInfo CreateMessageInfo(TMessage message);

        /// <summary>
        /// parse protocol message
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="maxMessageSize"></param>
        /// <param name="recvivedMessageInfo"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        bool TryParse(IConnection connection, ArraySegment<byte> buffer, int maxMessageSize, out IRecvivedMessageInfo<TMessage> recvivedMessageInfo, out int readlength);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageInfo"></param>
        /// <param name="seqId"></param>
        /// <returns></returns>
        byte[] Serialize(TSendMessageInfo messageInfo, out int seqId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        void UpdateCryptKey(object key);
    }


    /// <summary>
    /// 协议接口
    /// </summary>
    /// <typeparam name="TMessageInfo"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public interface IClientProtocolHandler<TMessageInfo, TMessage> : IProtocolHandler<TMessageInfo, TMessage>
        where TMessageInfo : IAsyncMessageInfo<TMessage> {

    }


    /// <summary>
    /// 协议接口
    /// </summary>
    /// <typeparam name="TSendMessageInfo"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public interface IServerProtocolHandler<TSendMessageInfo, TMessage> : IProtocolHandler<TSendMessageInfo, TMessage> where TSendMessageInfo : ISendMessageInfo<TMessage> {

    }

    /// <summary>
    /// 协议接口
    /// </summary>
    /// <typeparam name="TMessageInfo"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public interface ISyncClientProtocolHandler<TMessageInfo, TMessage> : IClientProtocolHandler<TMessageInfo, TMessage>
        where TMessageInfo : ISyncMessageInfo<TMessage> {

    }
}