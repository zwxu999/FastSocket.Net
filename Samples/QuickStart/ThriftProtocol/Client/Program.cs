using Sodao.FastSocket.Client;
using Sodao.FastSocket.Client.Messaging;
using Sodao.FastSocket.Client.Protocol;
using Sodao.FastSocket.Client.Protocol.Handlers;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Protocol;
using Sodao.FastSocket.SocketBase.Protocol.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThriftClient {
    class Program {
        static private long index = 0;
        static ConcurrentDictionary<long, Task<bool>> sendTasks = new ConcurrentDictionary<long, Task<bool>>();
        static ConcurrentDictionary<long, bool> sendSuccStatus = new ConcurrentDictionary<long, bool>();
        static ConcurrentDictionary<long, bool> sendFailStatus = new ConcurrentDictionary<long, bool>();
        static ConcurrentDictionary<long, bool> recvSuccStatus = new ConcurrentDictionary<long, bool>();
        static ConcurrentDictionary<long, object[]> recvFailStatus = new ConcurrentDictionary<long, object[]>();
        static ConcurrentBag<string> ReceivedAsyncMessages = new ConcurrentBag<string>();
        static ConcurrentBag<string> ReceivedAsyncMessages2 = new ConcurrentBag<string>();

        static public void Main() {

            Sodao.FastSocket.SocketBase.Log.Trace.EnableConsole();
            var client = new SocketClient<ISyncMessageInfo<ClientThriftMessage >, ClientThriftMessage>(new SyncClientThriftProtocolHandlerFactory(), 102400, 102400, 30000, 30000);
            client.ReceivedAsyncMessage += (connection, msg) => {
                if (msg.Text.Length > 20) {//"init_reply ok"
                    ReceivedAsyncMessages.Add(msg.Text);
                    //if (ReceivedAsyncMessages.Count % 3000 == 0) 
                    Console.WriteLine($"client {connection.ConnectionID} ReceivedAsyncMessage:\t{msg}");

                } else {
                    ReceivedAsyncMessages2.Add(msg.Text);
                }
            };

            //建立50个socket连接
            for (int i = 0; i < 50; i++) {
                client.TryRegisterEndPoint(i.ToString(), new[] { new IPEndPoint(IPAddress.Parse("127.0.0.1"), 19516) },
                    (connection, name) => {
                        var source = new TaskCompletionSource<bool>();
                        client.SendSync(new ClientThriftMessage("init"),
                            ex => source.TrySetException(ex),
                            message => { source.TrySetResult(true); Console.WriteLine($"{connection.ConnectionID} init reply: {name}\t->\t{message}"); },
                            8000);

                        source.TrySetResult(true);
                        var task = source.Task;
                        return task;
                    });
            }


            void print(object obj) {
                while (true) {
                    Thread.Sleep(2000);
                    Console.WriteLine("_pendingQueue:\t" + client._pendingQueue.Count);
                    Console.WriteLine("_receivingQueue:\t" + client._receivingQueue.Count);
                    Console.WriteLine("sendTasks:\t" + sendTasks.Count);
                    Console.WriteLine("sendSuccStatus:\t" + sendSuccStatus.Count);
                    Console.WriteLine("sendFailStatus:\t" + sendFailStatus.Count);
                    Console.WriteLine("send total:\t" + (sendSuccStatus.Count + sendFailStatus.Count));
                    Console.WriteLine("recvSuccStatus:\t" + recvSuccStatus.Count);
                    Console.WriteLine("recvFailStatus:\t" + recvFailStatus.Count);
                    Console.WriteLine("ReceivedAsyncMessages:\t" + ReceivedAsyncMessages.Count);
                    Console.WriteLine("ReceivedAsyncMessages2:\t" + ReceivedAsyncMessages2.Count);
                    Console.WriteLine("recv total:\t" + (recvFailStatus.Count + recvSuccStatus.Count + sendFailStatus.Count + ReceivedAsyncMessages.Count));
                    Console.WriteLine("***************************************************************************");
                }
            }

            var printthread = new Thread(print);
            printthread.Start();

            while (true) {
                Do(client, 500000);
                Console.ReadLine();
                Console.WriteLine();
                sendTasks.Clear();
                sendSuccStatus.Clear();
                sendFailStatus.Clear();
                recvSuccStatus.Clear();
                recvFailStatus.Clear();
                while (ReceivedAsyncMessages.Count > 0) {
                    ReceivedAsyncMessages.TryTake(out var _);
                }
                while (ReceivedAsyncMessages2.Count > 0) {
                    ReceivedAsyncMessages2.TryTake(out var _);
                }
            }
        }

        static private void Do(SocketClient<ISyncMessageInfo<ClientThriftMessage>, ClientThriftMessage> client, int count) {
            for (int i = 0; i < count; i++) {
                try { Echo(client, Interlocked.Increment(ref index)); } catch (Exception ex) {
                    Console.WriteLine(i.ToString() + " " + ex.Message);
                }
            }
        }

        static public void Echo(SocketClient<ISyncMessageInfo<ClientThriftMessage>, ClientThriftMessage> client, long seqId) {
            var seq = $"{seqId:0000000}";
            var guid = Guid.NewGuid().ToString();
            var task = client.SendSync(new ClientThriftMessage($"echo {seq} {guid }"),
                new errorhandler(recvFailStatus, seqId).handle,
                new messageHandler(recvSuccStatus, recvFailStatus, guid, seqId).handle
                );
            sendTasks.TryAdd(seqId, task);
            task.ContinueWith(p => {
                var succ = p.Status == TaskStatus.RanToCompletion && p.Result;
                var list = succ ? sendSuccStatus : sendFailStatus;
                list.TryAdd(seqId, succ);
                sendTasks.TryRemove(seqId, out var _);
                if (seqId % 3000 == 100)
                    Console.WriteLine($"client send: {seq:0000000}: {succ}");
            });
        }
    }

    class messageHandler {
        public messageHandler(
            ConcurrentDictionary<long, bool> recvSuccStatus,
            ConcurrentDictionary<long, object[]> recvFailStatus,
            string guid, long seq) {
            RecvSuccStatus = recvSuccStatus;
            RecvFailStatus = recvFailStatus;
            Guid = guid;
            Seq = seq;
        }

        public ConcurrentDictionary<long, bool> RecvSuccStatus { get; }
        public ConcurrentDictionary<long, object[]> RecvFailStatus { get; }
        public string Guid { get; }
        public long Seq { get; }

        public void handle(ClientThriftMessage message) {
            var seg = message.Text.Split(' ');
            if (seg.Length > 2) {
                var succ = seg[2] == Guid;
                if (succ) {
                    RecvSuccStatus.TryAdd(Seq, succ);
                } else {
                    RecvFailStatus.AddOrUpdate(Seq, s => new string[] { message.Text, "" }, (u, old) => new object[] { message, old[1] });
                }
                if (Seq % 3000 == 0) {
                    Console.WriteLine($"client recv: {Seq:0000000}: {succ}");
                }
            }
        }
    }

    class errorhandler {
        public errorhandler(ConcurrentDictionary<long, object[]> recvFailStatus, long seq) {
            RecvFailStatus = recvFailStatus;
            Seq = seq;
        }

        public ConcurrentDictionary<long, object[]> RecvFailStatus { get; }
        public long Seq { get; }

        public void handle(Exception ex) {
            RecvFailStatus.AddOrUpdate(Seq, s => new string[] { "", ex.ToString() }, (s, old) => new object[] { old[0], ex.ToString() });
            if (Seq % 3000 == 0) {
                Console.WriteLine($"client recv: {Seq}: failed, {ex}");
            }
        }
    }
}