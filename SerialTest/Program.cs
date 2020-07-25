using NewLife;
using NewLife.Log;
using NewLife.Security;
using NewLife.Threading;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using NewLife;
using System.IO;
using NewLife.IO;

namespace SerialTest
{
    class Program
    {
        static void Main(string[] args)
        {
            XTrace.UseConsole();

            //TestSerial();
            Test485();

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

        static void Test485()
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

            Console.Write("波特率[9600]：");
            var baudRate = Console.ReadLine().ToInt();
            if (baudRate <= 0) baudRate = 9600;

            var sp = new SerialPort(names[n - 1], baudRate)
            {
                Encoding = Encoding.UTF8
            };
            sp.Open();

            //var cmd = new Byte[] { 0x01, 0x05, 0x00, 0x00, 0x5A, 0x00, 0xF7, 0x6A };
            //var crc = Crc(cmd, 0, cmd.Length - 2);
            //var cmd = WriteSingleCoil(1, 0, 0x5A00);
            for (int i = 0; i < 100; i++)
            {
                var cmd = WriteSingleCoil(1, (UInt16)(i % 4), 0x5500);
                Execute(sp, cmd);

                Thread.Sleep(500);
            }
        }

        private static Byte[] Execute(SerialPort sp, Byte[] cmd)
        {
            sp.Write(cmd, 0, cmd.Length);
            Thread.Sleep(100);

            var rs = new Byte[32];
            var count = sp.Read(rs, 0, rs.Length);
            XTrace.WriteLine(rs.ToHex(0, count));

            return rs;
        }

        private static Byte[] WriteSingleCoil(Byte id, UInt16 addr, UInt16 value)
        {
            var cmd = new Byte[8];
            cmd[0] = id;
            cmd[1] = 0x05;
            cmd[2] = (Byte)(addr >> 8);
            cmd[3] = (Byte)(addr & 0xFF);
            cmd[4] = (Byte)(value >> 8);
            cmd[5] = (Byte)(value & 0xFF);

            var crc = Crc(cmd, 0, cmd.Length - 2);
            cmd[6] = (Byte)(crc & 0xFF);
            cmd[7] = (Byte)(crc >> 8);

            return cmd;
        }

        #region CRC
        static readonly UInt16[] crc_ta = new UInt16[16] { 0x0000, 0xCC01, 0xD801, 0x1400, 0xF001, 0x3C00, 0x2800, 0xE401, 0xA001, 0x6C00, 0x7800, 0xB401, 0x5000, 0x9C01, 0x8801, 0x4400, };

        /// <summary>Crc校验</summary>
        /// <param name="data"></param>
        /// <param name="offset">偏移</param>
        /// <param name="count">数量</param>
        /// <returns></returns>
        public static UInt16 Crc(Byte[] data, Int32 offset, Int32 count = -1)
        {
            if (data == null || data.Length < 1) return 0;

            UInt16 u = 0xFFFF;
            Byte b;

            if (count == 0) count = data.Length - offset;

            for (var i = offset; i < count; i++)
            {
                b = data[i];
                u = (UInt16)(crc_ta[(b ^ u) & 15] ^ (u >> 4));
                u = (UInt16)(crc_ta[((b >> 4) ^ u) & 15] ^ (u >> 4));
            }

            return u;
        }
        #endregion
    }
}