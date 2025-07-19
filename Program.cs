using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        var gpio = new GpioController();

        // OLED #1: CE0, DC=GPIO25, RESET=GPIO24
        var spi1 = SpiDevice.Create(
            new SpiConnectionSettings(0, 0) { ClockFrequency = 10_000_000, Mode = SpiMode.Mode0 }
        );
        var oled1 = new Sh1107(spi1, gpio, dcPin: 25, resetPin: 24);

        // OLED #2: CE1, DC=GPIO26, RESET=GPIO27
        var spi2 = SpiDevice.Create(
            new SpiConnectionSettings(0, 1) { ClockFrequency = 10_000_000, Mode = SpiMode.Mode0 }
        );
        var oled2 = new Sh1107(spi2, gpio, dcPin: 26, resetPin: 27);

        // Initialize and display on OLED #1
        oled1.Initialize();
        oled1.DrawTestPattern();

        // Initialize and display on OLED #2
        oled2.Initialize();
        oled2.DrawTestPattern();

        Thread.Sleep(2000);

        // Clear and turn off both displays
        oled1.Clear();
        oled1.TurnOff();

        oled2.Clear();
        oled2.TurnOff();
    }
}
