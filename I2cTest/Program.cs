using Iot.Device.Bh1750fvi;
using NewLife.Log;
using System;
using System.Device.I2c;
using System.Threading;

namespace I2cTest
{
    class Program
    {
        static void Main(string[] args)
        {
            XTrace.UseConsole();

            if (args.Length >= 1)
            {
                var mode = args[0]?.ToLower();
                XTrace.WriteLine("Mode: {0}", mode);

                if (mode == "bh1750")
                    TestBH1750();
            }
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
    }
}