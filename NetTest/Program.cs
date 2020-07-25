using NewLife;
using NewLife.Log;
using NewLife.Net;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetTest
{
    class Program
    {
        static void Main(string[] args)
        {
            XTrace.UseConsole();

            //TestUdp();
            TestTcp();
            TestServer();

            Console.WriteLine("OK!");
            Console.ReadKey();
        }

        static async void TestUdp()
        {
            var udp = new UdpClient(1234);
            while (true)
            {
                var rs = await udp.ReceiveAsync();
                XTrace.WriteLine("收到[{0}]:{1}", rs.RemoteEndPoint, rs.Buffer.ToStr());

                var buf = "我收到啦！".GetBytes();
                udp.Send(buf, buf.Length, rs.RemoteEndPoint);
            }
        }

        static async void TestTcp()
        {
            var server = new TcpListener(IPAddress.Any, 1234);
            server.Start();

            while (true)
            {
                // 等待连接
                var tcp = await server.AcceptTcpClientAsync();
                XTrace.WriteLine("收到连接，来自：{0}", tcp.Client.RemoteEndPoint);

                // 异步处理
                ThreadPool.QueueUserWorkItem(async s =>
                {
                    // 通过网络流接收数据
                    var ns = tcp.GetStream();
                    ns.Write("欢迎观临嵌入式NETCore直播间！".GetBytes());

                    var buf = new Byte[8192];
                    while (true)
                    {
                        var count = await ns.ReadAsync(buf, 0, buf.Length);
                        if (count > 0)
                        {
                            XTrace.WriteLine("收到[{0}]:{1}", tcp.Client.RemoteEndPoint, buf.ToStr(null, 0, count));

                            ns.Write("我收到啦！".GetBytes());
                        }
                    }
                });
            }
        }

        private static NetServer _Server;
        static void TestServer()
        {
            var server = new MyServer
            {
                Port = 2234,
                Log = XTrace.Log
            };
            server.Start();

            _Server = server;
        }
    }
}
