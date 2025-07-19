using System.Device.Spi;
using System.Threading;

public class Sh1107
{
    private readonly SpiDevice _spi;

    public Sh1107(SpiDevice spi)
    {
        _spi = spi;
    }

    public void SendCommand(byte cmd)
    {
        _spi.Write(new[] { (byte)0x00, cmd }); // Control byte 0x00 for command
    }

    public void SendData(byte data)
    {
        _spi.Write(new[] { (byte)0x40, data }); // Control byte 0x40 for data
    }

    public void Initialize()
    {
        SendCommand(0xAE); // Display off

        SendCommand(0xDC);
        SendCommand(0x00); // Display start line
        SendCommand(0x81);
        SendCommand(0x2F); // Contrast
        SendCommand(0xA0); // Segment remap
        SendCommand(0xC0); // COM scan direction
        SendCommand(0xA8);
        SendCommand(0x7F); // Multiplex ratio
        SendCommand(0xD3);
        SendCommand(0x60); // Display offset
        SendCommand(0xD5);
        SendCommand(0x50); // Clock divide ratio
        SendCommand(0xA6); // Normal display
        SendCommand(0xAF); // Display on
    }

    public void DrawTestPattern()
    {
        for (int page = 0; page < 8; page++)
        {
            SendCommand((byte)(0xB0 + page)); // Set page
            SendCommand(0x10); // High column
            SendCommand(0x02); // Low column

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
        SendCommand(0xAE); // Display off
    }
}
