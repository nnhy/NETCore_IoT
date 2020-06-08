using System;
using System.Collections.Generic;
using System.Device.Gpio.Drivers;
using System.Text;

namespace IoTTest
{
    class RaspberryPi4Driver : RaspberryPi3Driver
    {
        protected override Int32 ConvertPinNumberToLogicalNumberingScheme(Int32 pinNumber)
        {
            if (pinNumber == 7) return 4;

            return base.ConvertPinNumberToLogicalNumberingScheme(pinNumber);
        }
    }
}