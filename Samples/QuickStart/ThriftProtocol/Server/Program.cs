using Sodao.FastSocket.Server;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Protocol;
using System;
using System.Text;
using System.Linq;
using System.Diagnostics;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;
using Sodao.FastSocket.SocketBase.Messaging;

namespace ThriftServer {
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

    public class MyService : AbsSocketService<ThriftMessage> {
        public override void OnConnected(IConnection connection) {
            base.OnConnected(connection);
            Console.WriteLine(connection.RemoteEndPoint.ToString() + " " + connection.ConnectionID.ToString() + " connected");
            connection.BeginReceive();
        }

        int count = 0;
        public override void OnReceived(IConnection connection, IRecvivedMessageInfo<ThriftMessage> recvivedMessageInfo) {
            base.OnReceived(connection, recvivedMessageInfo);
            var cmd = recvivedMessageInfo.Message;
            System.Threading.Interlocked.Increment(ref count);
            var arr = cmd.Text.Split(' ');
            switch (arr[0]) {
                case "echo":
                    recvivedMessageInfo.Reply(new ThriftMessage(string.Join(" ", "echo_reply", arr[1], arr[2])), connection);
                    break;
                case "init":
                    recvivedMessageInfo.Reply(new ThriftMessage("init_reply ok"), connection);
                    break;
                default:
                    recvivedMessageInfo.Reply(new ThriftMessage("error unknow command "), connection);
                    break;
            }
            if (count % 2000 == 100) {
                Console.WriteLine($"connection:\t{connection.ConnectionID}\t{recvivedMessageInfo.Message}");
                connection.BeginSend(new ThriftMessage("hello!!!!"));
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