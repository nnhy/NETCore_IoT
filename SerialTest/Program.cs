using NewLife.Log;
using System;
using System.IO.Ports;
using System.Threading;

namespace SerialTest
{
    class Program
    {
        static void Main(string[] args)
        {
            XTrace.UseConsole();

            TestSerial();

            Console.WriteLine("OK!");
            Console.ReadKey();
        }

        static void TestSerial()
        {
            Console.WriteLine("发现串口：");
            var names = SerialPort.GetPortNames();
            foreach (var item in names)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine();

            if (names.Length == 0) return;

            var sp = new SerialPort(names[0], 115200);
            sp.DataReceived += Sp_DataReceived;
            sp.Open();

            for (int i = 0; i < 10; i++)
            {
                //sp.Write("我是第" + i + "行");
                //sp.Write("I am " + i + " !");
                var str = $"我是第{i + 1}行";
                var buf = str.GetBytes();
                sp.Write(buf, 0, buf.Length);

                Thread.Sleep(100);
            }
        }

        private static void Sp_DataReceived(Object sender, SerialDataReceivedEventArgs e)
        {
            var sp = sender as SerialPort;

            var buf = new Byte[sp.BytesToRead];
            var count = sp.Read(buf, 0, buf.Length);

            Console.WriteLine("收到：{0}", buf.ToStr(null, 0, count));
        }
    }
}