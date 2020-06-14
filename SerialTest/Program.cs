using NewLife;
using NewLife.Log;
using NewLife.Threading;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace SerialTest
{
    class Program
    {
        static void Main(string[] args)
        {
            XTrace.UseConsole();

            TestSerial();

            //Console.WriteLine("OK!");
            //Console.ReadKey();
        }

        static void TestSerial()
        {
            Console.WriteLine("发现串口：");
            var idx = 1;
            var names = SerialPort.GetPortNames();
            foreach (var item in names)
            {
                Console.WriteLine("{0}, {1}", idx++, item);
            }

            if (names.Length == 0) return;

            Console.WriteLine();
            Console.Write("选择串口[1]：");
            var n = Console.ReadLine().ToInt();
            if (n <= 0 || n > names.Length) n = 1;

            Console.Write("波特率[115200]：");
            var baudRate = Console.ReadLine().ToInt();
            if (baudRate <= 0) baudRate = 115200;

            var sp = new SerialPort(names[n - 1], baudRate)
            {
                Encoding = Encoding.UTF8
            };
            sp.DataReceived += Sp_DataReceived;
            sp.ErrorReceived += Sp_ErrorReceived;
            sp.PinChanged += Sp_PinChanged;
            sp.Open();

            // Mono没有中断，事件不可用，需要定时刷
            TimerX timer = null;
            if (Runtime.Mono) timer = new TimerX(s => Sp_DataReceived(sp, null), null, 100, 100) { Async = true };

            // 显示串口状态
            Console.WriteLine();
            foreach (var pi in sp.GetType().GetProperties())
            {
                Object val = null;
                try
                {
                    val = pi.GetValue(sp, null);
                }
                catch (Exception ex)
                {
                    val = $"[{ex.GetTrue().Message}]";
                }
                Console.WriteLine("{0}:\t{1}", pi.Name, val);
            }

            Console.WriteLine();
            Console.WriteLine("向串口{0}发送数据", sp.PortName);
            for (int i = 0; i < 10; i++)
            {
                var str = $"我是{sp.PortName}, 第{i + 1}行";
                sp.Write(str);

                Thread.Sleep(1000);
            }

            //Thread.Sleep(30_000);
        }

        private static void Sp_DataReceived(Object sender, SerialDataReceivedEventArgs e)
        {
            var sp = sender as SerialPort;
            if (sp.BytesToRead == 0) return;

            var buf = new Byte[sp.BytesToRead];
            var count = sp.Read(buf, 0, buf.Length);

            Console.WriteLine("[{0}]DataReceived：{1}", sp.PortName, buf.ToStr(null, 0, count));
        }

        private static void Sp_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            var sp = sender as SerialPort;

            Console.WriteLine("[{0}]ErrorReceived:{1}", sp.PortName, e.EventType);
        }

        private static void Sp_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            var sp = sender as SerialPort;

            Console.WriteLine("[{0}]PinChanged:{1}", sp.PortName, e.EventType);
        }
    }
}