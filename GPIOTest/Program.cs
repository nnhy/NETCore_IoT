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
                var mode = args[0]?.ToLower();
                XTrace.WriteLine("Mode: {0}", mode);

                if (mode == "-o")
                    TestOutput(args[1].ToInt());
                else if (mode == "-i")
                    TestInput(args[1].ToInt());
                else if (mode == "-di")
                    TestDI(args[1].ToInt());
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

        private static GpioController _gpio;
        static void TestDI(Int32 pin)
        {
            using var gpio = new GpioController(PinNumberingScheme.Logical);
            gpio.OpenPin(pin, PinMode.Input);

            gpio.RegisterCallbackForPinValueChangedEvent(pin, PinEventTypes.Rising, OnChange);
            gpio.RegisterCallbackForPinValueChangedEvent(pin, PinEventTypes.Falling, OnChange);

            _gpio = gpio;

            Thread.Sleep(30_000);
        }

        static void OnChange(Object sender, PinValueChangedEventArgs e)
        {
            //var gpio = sender as GpioController;
            var gpio = _gpio;

            XTrace.WriteLine("Event p{0}={1} {2}", e.PinNumber, gpio.Read(e.PinNumber), e.ChangeType);
        }
    }
}