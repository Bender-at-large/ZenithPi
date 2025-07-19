using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        // SPI setup: bus 0, chip select 0
        var settings = new SpiConnectionSettings(0, 0)
        {
            ClockFrequency = 10_000_000,
            Mode = SpiMode.Mode0,
        };

        using SpiDevice spi = SpiDevice.Create(settings);
        var oled = new Sh1107(spi);

        // Power up sequence
        oled.Initialize();
        oled.DrawTestPattern();

        Thread.Sleep(1500); // Let the pattern sit

        // Clear the screen and power down
        oled.Clear();
        oled.TurnOff();
    }
}
