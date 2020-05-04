using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Sodao.FastSocket.Server.Protocol.CommandLine;
using Sodao.FastSocket.Server.Protocol.Thrift;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// Socket server manager.
    /// </summary>
    public class SocketServerManager
    {
        #region Private Members
        /// <summary>
        /// key:server name.
        /// </summary>
        static private readonly Dictionary<string, SocketBase.IConnectionManager> _connectionManagerCollection =
            new Dictionary<string, SocketBase.IConnectionManager>();
        #endregion

        #region Static Methods
        /// <summary>
        /// 初始化Socket Server
        /// </summary>
        static public void Init()
        {
            Init("socketServer");
        }
        /// <summary>
        /// 初始化Socket Server
        /// </summary>
        /// <param name="sectionName"></param>
        static public void Init(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName)) throw new ArgumentNullException("sectionName");
            Init(ConfigurationManager.GetSection(sectionName) as Config.SocketServerConfig);
        }
        /// <summary>
        /// 初始化Socket Server
        /// </summary>
        /// <param name="config"></param>
        static public void Init(Config.SocketServerConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (config.Servers == null) return;

            foreach (Config.Server serverConfig in config.Servers)
            {
                //inti protocolHandlerFactory
                var objProtocolHandlerFactory = GetProtocolHandlerFactory(serverConfig.ProtocolHandlerFactory);
                if (objProtocolHandlerFactory == null) throw new InvalidOperationException("protocolHandlerFactory");

                //init custom service
                var tService = Type.GetType(serverConfig.ServiceType, false);
                if (tService == null) throw new InvalidOperationException("serviceType");

                var objService = Activator.CreateInstance(tService);
                if (objService == null) throw new InvalidOperationException("serviceType");

                var messageTypes = objProtocolHandlerFactory.GetType().GetInterface(typeof(IProtocolHandlerFactory<,,>).Name).GetGenericArguments();
                var socketServerType = typeof(SocketServer<,>).MakeGenericType(messageTypes[1], messageTypes[2]);

                //init host.
                _connectionManagerCollection.Add(serverConfig.Name, Activator.CreateInstance(
                        socketServerType,
                        serverConfig.Port,
                        objService,
                        objProtocolHandlerFactory,
                        serverConfig.SocketBufferSize,
                        serverConfig.MessageBufferSize,
                        serverConfig.MaxMessageSize,
                        serverConfig.MaxConnections) as SocketBase.IConnectionManager);
            }
        }
        /// <summary>
        /// get protocol.
        /// </summary>
        /// <param name="protocolHandlerFactory"></param>
        /// <returns></returns>
        static public object GetProtocolHandlerFactory(string protocolHandlerFactory)
        {
            switch (protocolHandlerFactory) 
            {
                case Protocol.ProtocolNames.Thrift: return new ServerThriftProtocolHandlerFactory();
                case Protocol.ProtocolNames.CommandLine: return new ServerCommandLineProtocolHandlerFactory();
            }
            return Activator.CreateInstance(Type.GetType(protocolHandlerFactory, false));
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        static public void Start()
        {
            _connectionManagerCollection.ToList().ForEach(c => c.Value.Start());
        }
        /// <summary>
        /// 停止服务
        /// </summary>
        static public void Stop()
        {
            _connectionManagerCollection.ToList().ForEach(c => c.Value.Stop());
        }
        /// <summary>
        /// try get host by name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="connectionManager"></param>
        /// <returns></returns>
        static public bool TryGetHost(string name, out SocketBase.IConnectionManager connectionManager)
        {
            return _connectionManagerCollection.TryGetValue(name, out connectionManager);
        }
        #endregion
    }
}