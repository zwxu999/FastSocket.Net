using Sodao.FastSocket.Server;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Protocol;
using System;
using System.Text;
using System.Linq;
using System.Diagnostics;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;
using Sodao.FastSocket.SocketBase.Messaging;

namespace CommandLineServer {
    class Program {
        static void Main(string[] args) {

            SocketServerManager.Init();
            SocketServerManager.Start();

            //每隔10秒强制断开所有连接
            System.Threading.Tasks.Task.Factory.StartNew(() => {
                while (true) {
                    System.Threading.Thread.Sleep(1000 * 10);
                    if (SocketServerManager.TryGetHost("quickStart", out var connectionManager)) {
                        var arr = connectionManager.ListAllConnection();
                        //foreach (var c in arr) c.BeginDisconnect();
                    }
                }
            });
            Console.ReadLine();
        }
    }

    public class MyService : AbsSocketService<CommandLineMessage> {
        public override void OnConnected(IConnection connection) {
            base.OnConnected(connection);
            Console.WriteLine(connection.RemoteEndPoint.ToString() + " " + connection.ConnectionID.ToString() + " connected");
            connection.BeginReceive();
        }

        int count = 0;
        public override void OnReceived(IConnection connection, IRecvivedMessageInfo<CommandLineMessage> recvivedMessageInfo) {
            base.OnReceived(connection, recvivedMessageInfo);
            var cmd = recvivedMessageInfo.Message;
            System.Threading.Interlocked.Increment(ref count);

            switch (cmd.CmdName) {
                case "echo":
                    recvivedMessageInfo.Reply(new CommandLineMessage("echo_reply", cmd.Parameters), connection);
                    break;
                case "init":
                    recvivedMessageInfo.Reply(new CommandLineMessage("init_reply", new[] { "ok" }), connection);
                    break;
                default:
                    recvivedMessageInfo.Reply(new CommandLineMessage("error unknow command "), connection);
                    break;
            }
            if (count % 2000 == 100) {
                Console.WriteLine($"connection:\t{connection.ConnectionID}\t{recvivedMessageInfo.Message}");
                connection.BeginSend(new CommandLineMessage("hello!!!!!!"));
            }
        }

        public override void OnDisconnected(IConnection connection, Exception ex) {
            base.OnDisconnected(connection, ex);
            Console.WriteLine(connection.RemoteEndPoint.ToString() + " disconnected");
        }

        public override void OnException(IConnection connection, Exception ex) {
            base.OnException(connection, ex);
            Console.WriteLine(ex.ToString());
        }
    }
}