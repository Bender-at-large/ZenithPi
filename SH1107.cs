using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;

public class Sh1107
{
    private readonly SpiDevice _spi;
    private readonly GpioController _gpio;
    private readonly int _dcPin;
    private readonly int _resetPin;

    public Sh1107(SpiDevice spi, GpioController gpio, int dcPin, int resetPin)
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
        _gpio.Write(_dcPin, PinValue.Low); // Command mode
        _spi.WriteByte(command);
    }

    public void SendData(byte data)
    {
        _gpio.Write(_dcPin, PinValue.High); // Data mode
        _spi.WriteByte(data);
    }

    public void Initialize()
    {
        // Reset sequence
        _gpio.Write(_resetPin, PinValue.Low);
        Thread.Sleep(10);
        _gpio.Write(_resetPin, PinValue.High);
        Thread.Sleep(10);

        SendCommand(0xAE); // Display OFF
        SendCommand(0xDC);
        SendCommand(0x00); // Display start line
        SendCommand(0x81);
        SendCommand(0x2F); // Contrast
        SendCommand(0xA0); // Segment remap
        SendCommand(0xC0); // COM scan direction
        SendCommand(0xA8);
        SendCommand(0x7F); // Multiplex
        SendCommand(0xD3);
        SendCommand(0x60); // Display offset
        SendCommand(0xD5);
        SendCommand(0x50); // Clock divide
        SendCommand(0xA6); // Normal display
        SendCommand(0xAF); // Display ON
    }

    public void DrawTestPattern()
    {
        for (int page = 0; page < 8; page++)
        {
            SendCommand((byte)(0xB0 + page)); // Set page
            SendCommand(0x10);
            SendCommand(0x02); // Column start

            for (int col = 0; col < 128; col++)
            {
                byte data = (col % 2 == 0) ? (byte)0xAA : (byte)0x55;
                SendData(data);
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

    public void TurnOff()
    {
        SendCommand(0xAE); // Display OFF
    }
}
