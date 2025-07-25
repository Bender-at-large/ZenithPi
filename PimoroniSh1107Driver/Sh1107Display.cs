
using DisplayCore;
using System.Device.Spi;
using System.Device.Gpio;
using System.Threading;

namespace PimoroniSh1107Driver;

public class Sh1107Display : IDisplayTarget
{
    private readonly SpiDevice _spi;
    private readonly GpioController _gpio;
    private readonly int _dcPin;
    private readonly int _resetPin;

    public Sh1107Display(SpiDevice spi, GpioController gpio, int dcPin, int resetPin)
    {
        _spi = spi;
        _gpio = gpio;
        _dcPin = dcPin;
        _resetPin = resetPin;

        _gpio.OpenPin(_dcPin, PinMode.Output);
        _gpio.OpenPin(_resetPin, PinMode.Output);
        Initialize();
    }

    public void DisplayFrame(byte[,] frameBuffer)
    {
        for (int page = 0; page < 8; page++)
        {
            SendCommand((byte)(0xB0 + page));
            SendCommand(0x10);
            SendCommand(0x02);

            for (int col = 0; col < 128; col++)
            {
                SendData(frameBuffer[page, col]);
            }
        }
    }

    private void Initialize()
    {
        _gpio.Write(_resetPin, PinValue.Low);
        Thread.Sleep(10);
        _gpio.Write(_resetPin, PinValue.High);
        Thread.Sleep(10);

        SendCommand(0xAE); // Display off
        SendCommand(0xDC); SendCommand(0x00); // Display start line
        SendCommand(0x81); SendCommand(0x2F); // Contrast
        SendCommand(0xA0); // Segment remap
        SendCommand(0xC0); // COM scan direction
        SendCommand(0xA8); SendCommand(0x7F); // Multiplex ratio
        SendCommand(0xD3); SendCommand(0x00); // Display offset
        SendCommand(0xD5); SendCommand(0x50); // Clock
        SendCommand(0xA6); // Normal display
        SendCommand(0xAF); // Display on
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

    public void Clear()
    {
        for (int page = 0; page < 8; page++)
        {
            SendCommand((byte)(0xB0 + page));  // Set page
            SendCommand(0x10);                 // High column
            SendCommand(0x02);                 // Low column

            for (int col = 0; col < 128; col++)
            {
                SendData(0x00);
            }
        }
    }
    public void TurnOff()
    {
        SendCommand(0xAE); // Display OFF
    }
}
