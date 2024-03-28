using System.Text;

namespace LCD_I2CFrutten
{
    public class I2CLcd
    {
        public Pin backlight { get; set; }

        private I2CPort.USB_ISS uSB_ISS;
        private const bool COMMAND = true;
        private readonly byte LcdPCF8574Address;

        public I2CLcd(I2CPort.USB_ISS uSB_ISS, byte address = 0x4E, Pin bl = Pin.BL)
        {
            this.uSB_ISS = uSB_ISS;
            this.LcdPCF8574Address = address;
            this.backlight = bl;

            this.Init();
        }

        public void Init()
        {
            SendLCD(0x33, COMMAND); // Must initialize to 8-line mode at first
            SendLCD(0x32, COMMAND); // Then initialize to 4-line mode
            SendLCD(0x28, COMMAND); // 2 Lines & 5*7 dots
            SendLCD(0x0C, COMMAND); // Enable display without cursor
            Clear();
        }

        public void Clear()
        {
            SendLCD(0x01, COMMAND); //clear Screen
        }

        public void SetCursor(int x, int y)
        {
            SendLCD(0x80 + 0x40 * y + x, COMMAND); // Move cursor
        }

        public void Write(int x, int y, string str)
        {
            SetCursor(x, y);

            foreach (byte b in Encoding.ASCII.GetBytes(str))
            {
                SendLCD(b);
            }
        }

        private void SendLCD(int data, bool isCommand = false)
        {
            byte[] buf = new byte[4];
            // Send bit7-4 firstly
            buf[0] = (byte)(data & 0xF0 | (byte)Pin.RS | (byte)Pin.EN | (byte)this.backlight);
            if (isCommand) buf[0] &= (byte)~Pin.RS;     // Command? -> RS=0
            buf[1] = (byte)(buf[0] & (byte)~Pin.EN);    // Make EN = 0
            // Send bit3-0 secondly
            buf[2] = (byte)(((data & 0x0F) << 4) | (byte)Pin.RS | (byte)Pin.EN | (byte)this.backlight);
            if (isCommand) buf[2] &= (byte)~Pin.RS;     // Command? -> RS=0
            buf[3] = (byte)(buf[2] & (byte)~Pin.EN);    // Make EN = 0

            this.uSB_ISS.I2CWrite(this.LcdPCF8574Address, 4, buf);

            //if (isCommand) Thread.Sleep(1);
        }

        public enum Pin : byte // pin layout
        {
            BLOFF = 0,  // slight hack backlight-off
            RS = 1,     // Data
            RW = 2,     // we only write here
            EN = 4,     // clock in data
            BL = 8,     // backlight
            D4 = 16,
            D5 = 32,
            D6 = 64,
            D7 = 128,
        }
    }
}
