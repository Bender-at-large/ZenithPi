using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
class Program
{
    private static readonly GpioController _gpio = new GpioController();
    private static readonly int _dcPin = 25;

    static void Main()
    {
        var spiSettings = new SpiConnectionSettings(0, 0)
        {
            ClockFrequency = 1000000,
            Mode = SpiMode.Mode0,
            DataBitLength = 8
        };

        using SpiDevice spi = SpiDevice.Create(spiSettings);

        _gpio.OpenPin(_dcPin, PinMode.Output);

        Initialize(spi);

        Clear(spi);
        for (int i = 0; i < 128; i++)
            SendData((byte)(i % 2 == 0 ? 0xAA : 0x55), spi);

        SendCommand(0xAE, spi); // Display OFF
    }

    private static void SendCommand(byte command, SpiDevice spi)
    {
        _gpio.Write(_dcPin, PinValue.Low);
        spi.WriteByte(command);
    }

    private static void Initialize(SpiDevice spi)
    {
        SendCommand(0xAE, spi); // Display OFF
        SendCommand(0xDC, spi);
        SendCommand(0x62, spi); // Display start line
        SendCommand(0xA0, spi); // Segment remap
        SendCommand(0xC0, spi); // COM scan direction
        SendCommand(0xA8, spi); SendCommand(0x7F, spi); // Multiplex ratio
        SendCommand(0xD3, spi); SendCommand(0x00, spi); // Display offset
        SendCommand(0xAD, spi); SendCommand(0x8B, spi); // DC-DC control
        SendCommand(0xA6, spi); // Normal display
        SendCommand(0xAF, spi); // Display ON
    }

    public static void Clear(SpiDevice spi)
    {
        for (int page = 0; page < 8; page++)
        {
            SendCommand((byte)(0xB0 + page), spi); // Set page address
            SendCommand(0x10, spi); // High column
            SendCommand(0x02, spi); // Low column

            for (int i = 0; i < 128; i++)
                SendData(0x00, spi); // Clear page
        }
    }
    
     private static void SendData(byte data, SpiDevice spi)
    {
        _gpio.Write(_dcPin, PinValue.High);
        spi.WriteByte(data);
    }
}