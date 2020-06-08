using Iot.Device.Bh1750fvi;
using NewLife.Log;
using NewLife.Net;
using NewLife.Threading;
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace IoTTest
{
    class Program
    {
        static void Main(String[] args)
        {
            XTrace.UseConsole();

            Console.WriteLine("Hello World!");

            //TestInput();
            //TestDO();
            //TestDI();

            //TestUdp();
            //TestTcp();
            TestNet();
            //TestBH1750();
            //TestSerial();

            Console.ReadKey();
        }

        static void TestDO()
        {
            var pin1 = 17;
            var pin2 = 23;
            using var gpio = new GpioController(PinNumberingScheme.Logical);
            gpio.OpenPin(pin1, PinMode.Output);
            gpio.OpenPin(pin2, PinMode.Output);

            var flag = false;
            for (var i = 0; i < 200; i++)
            {
                var pv1 = flag ? PinValue.High : PinValue.Low;
                var pv2 = flag ? PinValue.Low : PinValue.High;
                flag = !flag;
                XTrace.WriteLine("P{0}={1} P{2}={3}", pin1, pv1, pin2, pv2);

                gpio.Write(pin1, pv1);
                gpio.Write(pin2, pv2);

                Thread.Sleep(500);
            }
        }

        private static GpioController _controller;
        private static TimerX _timer1;
        private static TimerX _timer2;
        static void TestInput()
        {
            var pin1 = 23;
            var pin2 = 24;
            var gpio = new GpioController(PinNumberingScheme.Logical);
            gpio.OpenPin(pin1, PinMode.Input);
            gpio.OpenPin(pin2, PinMode.Input);

            _controller = gpio;

            _timer1 = new TimerX(OnInput1, null, 500, 500);
            _timer2 = new TimerX(OnInput2, null, 500, 500);

            Task.Run(TestDI);
        }

        static void OnInput1(Object state)
        {
            var flag = _controller.Read(23) == PinValue.High;
            if (flag) XTrace.WriteLine("光敏传感器：{0}", flag);
        }

        static void OnInput2(Object state)
        {
            var flag = _controller.Read(24) == PinValue.High;
            if (flag) XTrace.WriteLine("声音传感器：{0}", flag);
        }

        private static GpioController _gpio;
        static void TestDI()
        {
            var pin = 24;
            var gpio = new GpioController(PinNumberingScheme.Logical);
            gpio.OpenPin(pin, PinMode.Input);

            gpio.RegisterCallbackForPinValueChangedEvent(pin, PinEventTypes.Rising, OnChange);
            gpio.RegisterCallbackForPinValueChangedEvent(pin, PinEventTypes.Falling, OnChange);

            _gpio = gpio;
        }

        static void OnChange(Object sender, PinValueChangedEventArgs e)
        {
            //var gpio = sender as GpioController;
            var gpio = _gpio;

            XTrace.WriteLine("Event p{0}={1} {2}", e.PinNumber, gpio.Read(e.PinNumber), e.ChangeType);
        }

        static void TestBH1750()
        {
            var set = new I2cConnectionSettings(0x01, 0x23);
            var i2c = I2cDevice.Create(set);
            var bh = new Bh1750fvi(i2c);

            Console.WriteLine("MeasuringMode:{0}", bh.MeasuringMode);

            for (var i = 0; i < 100000; i++)
            {
                var lux = bh.Illuminance;
                Console.WriteLine($"光照强度：{lux} 流明 {bh.LightTransmittance:p2}");

                Thread.Sleep(500);
            }
        }

        static async void TestUdp()
        {
            var udp = new UdpClient(1234);
            while (true)
            {
                var rs = await udp.ReceiveAsync();
                XTrace.WriteLine("收到[{0}]：{1}", rs.RemoteEndPoint, rs.Buffer.ToStr());

                var buf = "我收到啦！".GetBytes();
                udp.Send(buf, buf.Length, rs.RemoteEndPoint);
            }
        }

        static async void TestTcp()
        {
            // 监听所有地址的1235端口
            var server = new TcpListener(IPAddress.Any, 1235);
            server.Start();

            while (true)
            {
                // 等待链接
                var tcp = await server.AcceptSocketAsync();
                XTrace.WriteLine("收到链接，来自：{0}", tcp.RemoteEndPoint);

                // 异步处理该链接
                ThreadPool.QueueUserWorkItem(s =>
                {
                    var sock = s as Socket;
                    var buf = new Byte[4096];
                    while (true)
                    {
                        // 接收数据
                        var rs = sock.Receive(buf);
                        XTrace.WriteLine("收到[{0}]：{1}", sock.RemoteEndPoint, buf.ToStr(null, 0, rs));

                        // 发送数据
                        sock.Send("我收到啦！".GetBytes());
                    }
                }, tcp);
            }
        }

        private static NetServer _svr;
        static void TestNet()
        {
            var server = new MyServer
            {
                Port = 1236,
                Log = XTrace.Log,
                SessionLog = XTrace.Log,
                StatPeriod = 10,
            };

            server.Start();

            _svr = server;
        }

        class MyServer : NetServer<MySession> { }

        class MySession : NetSession
        {
            public override void Start()
            {
                WriteLog("来了来了 {0}", Remote);
                base.Start();
            }

            protected override void OnReceive(ReceivedEventArgs e)
            {
                WriteLog("收到：{0}", e.Packet.ToStr());

                Send(e.Packet);
            }
        }
    }
}