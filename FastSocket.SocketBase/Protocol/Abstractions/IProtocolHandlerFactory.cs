using Sodao.FastSocket.SocketBase;

namespace Sodao.FastSocket.SocketBase.Protocol.Abstractions {

    /// <summary>
    /// Protocol处理器工厂
    /// </summary>
    /// <typeparam name="TProtocolHandler"></typeparam>
    /// <typeparam name="TMessageInfo"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public interface IProtocolHandlerFactory<TProtocolHandler, TMessageInfo, TMessage>
        where TProtocolHandler : IProtocolHandler<TMessageInfo, TMessage>
        where TMessageInfo : ISendMessageInfo<TMessage> 
        {

        /// <summary>
        /// CreateProtocolHandler
        /// </summary>
        /// <returns></returns>
        IProtocolHandler<TMessageInfo, TMessage> CreateProtocolHandler();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TMessageInfo"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public interface IServerProtocolHandlerFactory<TMessageInfo, TMessage> : IProtocolHandlerFactory<IServerProtocolHandler<TMessageInfo, TMessage>, TMessageInfo, TMessage>
        where TMessageInfo : ISendMessageInfo<TMessage> 
        {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TMessageInfo"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public interface IClientProtocolHandlerFactory<TMessageInfo, TMessage> : IProtocolHandlerFactory<IClientProtocolHandler<TMessageInfo, TMessage>, TMessageInfo, TMessage>
        where TMessageInfo : ISyncMessageInfo<TMessage> {

    }
}