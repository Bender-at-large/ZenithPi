using PimoroniSh1107Driver;
using DisplayCore;
using System.Device.Gpio;
using System.Device.Spi;
using System.Data;

public static class Program
{
    static void Main(string[] arg)
    {
        // GPIO
        var gpio = new GpioController();

        // SPI Device 0, CE0 → oled1
        var spi1 = SpiDevice.Create(new SpiConnectionSettings(0, 0)
        {
            ClockFrequency = 10_000_000,
            Mode = SpiMode.Mode0
        });

        // SPI Device 0, CE1 → oled2
        var spi2 = SpiDevice.Create(new SpiConnectionSettings(0, 1)
        {
            ClockFrequency = 10_000_000,
            Mode = SpiMode.Mode0
        });

        // Instantiate display drivers
        var oled1 = new Sh1107Display(spi1, gpio, dcPin: 25, resetPin: 24);
        var oled2 = new Sh1107Display(spi2, gpio, dcPin: 26, resetPin: 23);

        var testFrame = new byte[8, 128];
        for (int col = 0; col < 128; col++)
        {
            testFrame[0, col] = 0xFF; // Top line
            testFrame[7, col] = 0xFF; // Bottom line
            if (col % 2 == 0) testFrame[3, col] = 0xAA; // Center pattern
        }

        for (int i = 0; i <= 5; i++)
        {
            oled1.Clear();
            oled2.Clear();
            Thread.Sleep(1000);
            oled1.DisplayFrame(testFrame);
            oled2.DisplayFrame(testFrame);
            Thread.Sleep(1000);
        }

        Thread.Sleep(5000);

        oled1.TurnOff();
        oled1.TurnOff();
    }
}