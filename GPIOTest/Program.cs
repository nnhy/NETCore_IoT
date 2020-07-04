using NewLife.Data;
using NewLife.Log;
using System;
using System.Device.Gpio;
using System.Threading;

namespace GPIOTest
{
    class Program
    {
        static void Main(string[] args)
        {
            XTrace.UseConsole();

            if (args.Length >= 2)
            {
                if (args[0] == "-o")
                    TestOutput(args[1].ToInt());
                else if (args[0] == "-i")
                    TestInput(args[1].ToInt());
            }
        }

        static void TestOutput(Int32 pin)
        {
            using var gpio = new GpioController(PinNumberingScheme.Logical);
            gpio.OpenPin(pin, PinMode.Output);

            var flag = false;
            for (var i = 0; i < 20; i++)
            {
                XTrace.WriteLine("write {0}={1}", pin, flag);

                gpio.Write(pin, flag ? PinValue.High : PinValue.Low);
                flag = !flag;

                Thread.Sleep(500);
            }
        }

        static void TestInput(Int32 pin)
        {
            using var gpio = new GpioController(PinNumberingScheme.Logical);
            gpio.OpenPin(pin, PinMode.Input);

            for (var i = 0; i < 20; i++)
            {
                var val = gpio.Read(pin);
                XTrace.WriteLine("read {0}={1}", pin, val);

                Thread.Sleep(500);
            }
        }
    }
}