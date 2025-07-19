using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;

public class SH1107Driver
{
    private readonly SpiDevice _spi;
    private readonly GpioController _gpio;
    private readonly int _dcPin;

    public SH1107Driver(SpiDevice spi, int dcPin)
    {
        _spi = spi;
        _dcPin = dcPin;
        _gpio = new GpioController();

        _gpio.OpenPin(_dcPin, PinMode.Output);

        Initialize();
    }

    private void SendCommand(byte command)
    {
        _gpio.Write(_dcPin, PinValue.Low);
        _spi.WriteByte(command);
    }

    private void SendData(byte data)
    {
        _gpio.Write(_dcPin, PinValue.High);
        _spi.WriteByte(data);
    }

    private void Initialize()
    {
    SendCommand(0xAE); // Display OFF
SendCommand(0xDC); 
SendCommand(0x62); // Display start line
SendCommand(0xA0); // Segment remap
SendCommand(0xC0); // COM scan direction
SendCommand(0xA8); SendCommand(0x7F); // Multiplex ratio
SendCommand(0xD3); SendCommand(0x00); // Display offset
SendCommand(0xAD); SendCommand(0x8B); // DC-DC control
SendCommand(0xA6); // Normal display
SendCommand(0xAF); // Display ON
    }

    public void Clear()
    {
        for (int page = 0; page < 8; page++)
        {
            SendCommand((byte)(0xB0 + page)); // Set page address
SendCommand(0x10); // High column
SendCommand(0x02); // Low column

            for (int i = 0; i < 128; i++)
                SendData(0x00); // Clear page
        }
    }

    public void HelloTest()
    {
        Clear();
for (int i = 0; i < 128; i++)
    SendData((byte)(i % 2 == 0 ? 0xAA : 0x55));
    }
}