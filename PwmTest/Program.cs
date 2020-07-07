using Iot.Device.Buzzer;
using NewLife.Log;
using System;
using System.Threading;

namespace PwmTest
{
    class Program
    {
        static void Main(string[] args)
        {
            XTrace.UseConsole();

            if (args.Length >= 1)
            {
                var pin = args[0].ToInt(-1);
                if (pin > 0) TestBuzzer(pin);
            }
        }


        const int dDo = 175;
        const int dRe = 196;
        const int dMi = 221;
        const int dFa = 234;
        const int dSol = 262;
        const int dLa = 294;
        const int dSi = 330;
        const int Do = 350;
        const int Re = 393;
        const int Mi = 441;
        const int Fa = 495;
        const int Sol = 556;
        const int La = 624;
        const int Si = 661;
        const int hDo = 700;
        const int hRe = 786;
        const int hMi = 882;
        const int hFa = 935;
        const int hSol = 1049;
        const int hLa = 1178;
        const int hSi = 1322;
        static int[] scale = { Do, Re, Mi, Fa, Sol, La, Si, dDo, dRe, dMi, dFa, dSol, dLa, dSi, hDo, hRe, hMi, hFa, hSol, hLa, hSi };
        static int[] pu = { 3, 5, 5, 3, 5, 3, 5, 5, 400, 5, 1, 3, 3, 400, 400, 400, 400, 400, 400, 400, 3, 5, 5, 3, 5, 1, 5, 5, 5, 3, 5, 6, 3, 3, 2, 1, 1, 400, 400, 400, 400, 1, 15, 7, 6, 5, 6, 6, 5, 6, 6, 5, 6, 400, 5, 6, 6, 6, 6, 5, 5, 5, 400, 3, 2, 3, 2, 400, 5, 5, 4, 4, 400, 400, 3, 3, 2, 2, 400, 400, 2, 1, 1, 400, 400, 400, 400, 400, 400, 400, 6, 7, 15, 15, 15, 15, 15, 15, 16, 7, 15, 15, 400, 15, 15, 7, 6, 5, 5, 17, 17, 16, 15, 16, 16, 400 };
        static void TestBuzzer(Int32 pin)
        {
            XTrace.WriteLine("TestBuzzer {0}", pin);

            var buzzer = new Buzzer(pin);
            for (int i = 0; i < pu.Length; i++)
            {
                if (pu[i] < scale.Length)
                {
                    var freq = scale[pu[i] - 1];
                    XTrace.WriteLine("play {0}", freq);

                    buzzer.PlayTone(freq, 100);
                }
                else
                    Thread.Sleep(pu[i]);

                Thread.Sleep(100);
            }
        }
    }
}