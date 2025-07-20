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

        oled1.Clear();
        oled1.DrawText("Hello Pi", page: 0, column: 2);

        oled2.Clear();
        oled2.DrawText("Hello Pi", page: 0, column: 2);

        Thread.Sleep(5000);

        // Clear and turn off both displays
        oled1.Clear();
        oled1.TurnOff();

        oled2.Clear();
        oled2.TurnOff();
    }

    public static void TestOffset()
    {
        var gpio = new GpioController();

        var spi = SpiDevice.Create(
            new SpiConnectionSettings(0, 0) // Change to CE1 for second OLED
            {
                ClockFrequency = 10_000_000,
                Mode = SpiMode.Mode0,
            }
        );

        var oled = new Sh1107OffsetTest(spi, gpio, dcPin: 25, resetPin: 24);

        for (int offset = 0; offset <= 96; offset += 8) // Steps of 8 up to 0x60
        {
            Console.WriteLine($"Testing offset: 0x{offset:X2}");

            oled.Initialize(offset);
            oled.Clear();
            oled.DrawText($"OFFSET 0x{offset:X2}", page: 0, column: 2);

            Thread.Sleep(1000);
        }
    }
}

public class Sh1107OffsetTest
{
    private readonly SpiDevice _spi;
    private readonly GpioController _gpio;
    private readonly int _dcPin;
    private readonly int _resetPin;

    public Sh1107OffsetTest(SpiDevice spi, GpioController gpio, int dcPin, int resetPin)
    {
        _spi = spi;
        _gpio = gpio;
        _dcPin = dcPin;
        _resetPin = resetPin;

        _gpio.OpenPin(_dcPin, PinMode.Output);
        _gpio.OpenPin(_resetPin, PinMode.Output);
    }

    public void SendCommand(byte command)
    {
        _gpio.Write(_dcPin, PinValue.Low);
        _spi.WriteByte(command);
    }

    public void SendData(byte data)
    {
        _gpio.Write(_dcPin, PinValue.High);
        _spi.WriteByte(data);
    }

    public void Initialize(int offset)
    {
        _gpio.Write(_resetPin, PinValue.Low);
        Thread.Sleep(10);
        _gpio.Write(_resetPin, PinValue.High);
        Thread.Sleep(10);

        SendCommand(0xAE); // Display OFF
        SendCommand(0xDC);
        SendCommand(0x00); // Start line
        SendCommand(0x81);
        SendCommand(0x2F); // Contrast
        SendCommand(0xA0);
        SendCommand(0xC0); // Segment remap, COM scan
        SendCommand(0xA8);
        SendCommand(0x7F); // Multiplex
        SendCommand(0xD3);
        SendCommand((byte)offset); // Display offset
        SendCommand(0xD5);
        SendCommand(0x50); // Clock
        SendCommand(0xA6); // Normal display
        SendCommand(0xAF); // Display ON
    }

    public void DrawText(string text, int page, int column)
    {
        SendCommand((byte)(0xB0 + page));
        SendCommand(0x10);
        SendCommand((byte)(column & 0x0F));

        foreach (char c in text)
        {
            if (Font5x8.Glyphs.TryGetValue(c, out var glyph))
            {
                foreach (var b in glyph)
                    SendData(b);
                SendData(0x00);
            }
        }
    }

    public void Clear()
    {
        for (int page = 0; page < 8; page++)
        {
            SendCommand((byte)(0xB0 + page));
            SendCommand(0x10);
            SendCommand(0x02);
            for (int col = 0; col < 128; col++)
                SendData(0x00);
        }
    }

    public void TurnOff() => SendCommand(0xAE);
}

public static class Font5x8
{
    public static readonly Dictionary<char, byte[]> Glyphs = new()
    {
        ['H'] = new byte[] { 0x7F, 0x08, 0x08, 0x08, 0x7F },
        ['e'] = new byte[] { 0x3C, 0x4A, 0x4A, 0x4A, 0x30 },
        ['l'] = new byte[] { 0x00, 0x41, 0x7F, 0x40, 0x00 },
        ['o'] = new byte[] { 0x3E, 0x41, 0x41, 0x41, 0x3E },
        ['P'] = new byte[] { 0x7F, 0x09, 0x09, 0x09, 0x06 },
        ['i'] = new byte[] { 0x00, 0x41, 0x7F, 0x40, 0x00 },
        [' '] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 },
        ['0'] = new byte[] { 0x3E, 0x41, 0x41, 0x41, 0x3E },
        ['1'] = new byte[] { 0x00, 0x42, 0x7F, 0x40, 0x00 },
        ['2'] = new byte[] { 0x42, 0x61, 0x51, 0x49, 0x46 },
        ['6'] = new byte[] { 0x3C, 0x4A, 0x49, 0x49, 0x30 },
        ['X'] = new byte[] { 0x63, 0x14, 0x08, 0x14, 0x63 },
        ['F'] = new byte[] { 0x7F, 0x09, 0x09, 0x09, 0x01 },
        ['E'] = new byte[] { 0x7F, 0x49, 0x49, 0x49, 0x41 },
        ['T'] = new byte[] { 0x01, 0x01, 0x7F, 0x01, 0x01 },
        [' '] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 },
        // Add more as needed
    };
}
