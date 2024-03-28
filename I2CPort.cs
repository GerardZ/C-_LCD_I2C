using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LCD_I2CFrutten
{
    public class I2CPort
    {
        public class USB_ISS
        {

            private SerialPort USB_PORT;
            private string comport;
            private byte[] SerBuf = new byte[100];
            private ISS_MODE mode;
            private bool initOk = false;
            private bool errorOccurred = true;

            public string UsbIssVersion { get; set; }

            const int delay = 1;
            const int shortLCDDelay = 3000;  // 10000 is safe...

            public event EventHandler BusWasReInit;

            public enum ISS_MODE : byte
            {
                IO_CHANGE = 0x10,
                I2C_S_20KHZ = 0x20,
                I2C_S_50KHZ = 0x30,
                I2C_S_100KHZ = 0x40,
                I2C_S_400KHZ = 0x50,
                I2C_H_100KHZ = 0x60,
                I2C_H_400KHZ = 0x70,
                I2C_H_1000KHZ = 0x80,
                SPI_MODE = 0x90,
                SERIAL = 0x01,
            };

            public enum IIS_I2C_COMMANDS : byte
            {
                I2C_SGL = 0x53,                 // Read/Write single byte for non-registered devices, such as the Philips PCF8574 I/O chip.
                I2C_AD0 = 0x54,                 // Read/Write multiple bytes for devices without internal address or where address does not require resetting.   
                I2C_AD1 = 0x55,                 // Read/Write 1 byte addressed devices (the majority of devices will use this one)
                I2C_AD2 = 0x56,                 // Read/Write 2 byte addressed devices, eeproms from 32kbit (4kx8) and up. 
                I2C_DIRECT = 0x57,              // Used to build your own custom I2C sequences.
                I2C_TEST = 0x58,                // Used to check for the existence of an I2C device on the bus. (V5 or later firmware only)
            }


            public USB_ISS(string Comport = "COM3", ISS_MODE mode = ISS_MODE.I2C_S_400KHZ)
            {
                this.USB_PORT = new SerialPort();
                this.comport = Comport;
                this.mode = mode;

                InitPort();
            }


            private void InitPort()
            {
                USB_PORT.Close();                   // close any existing handle
                USB_PORT.PortName = this.comport;   // "COMx"
                USB_PORT.ReadTimeout = 50;         // was 50...
                USB_PORT.WriteTimeout = 50;         // was 50

                USB_PORT.Open();

                SerBuf[0] = 0x5A;       // get version command for USB-ISS, returns module id, software version and operating mode
                SerBuf[1] = 0x01;
                WriteToPort(2, SerBuf);
                ReadFromPort(3, SerBuf);



                if (SerBuf[0] == 7)  // if the module id is that of the USB-ISS 
                {
                    this.UsbIssVersion = $"USB-ISS V{SerBuf[1]}, mode: {SerBuf[2]}";

                    Console.WriteLine($"Found {this.UsbIssVersion}");  //print the software version on screen
                    this.initOk = true;
                    Thread.Sleep(50);
                    this.SetModeI2C(this.mode);

                }
                else
                {
                    Console.WriteLine("Not Found");
                    this.initOk = false;
                }


            }

            internal void WriteBytes(int v, byte[] data)
            {
                throw new NotImplementedException();
            }

            internal void WriteByte(int v, byte data)
            {
                throw new NotImplementedException();
            }

            private void SetModeI2C(ISS_MODE mode)
            {
                Thread.Sleep(50);
                SerBuf[0] = 0x5A;       // Set mode
                SerBuf[1] = (byte)mode;
                WriteToPort(2, SerBuf);
                ReadFromPort(1, SerBuf);

                Console.WriteLine($"USB-ISS interface init with mode: {mode}, result {SerBuf[0]}");
            }

            public void WriteToPort(int write_bytes, byte[] serBuf)
            {
                try
                {
                    USB_PORT.Write(serBuf, 0, write_bytes);      // writes specified amount of SerBuf out on COM port
                }
                catch (Exception e)
                {
                    HandleI2CException("writing", e); 
                }
            }

            public void ReadFromPort(int read_bytes, byte[] serBuf)
            {
                byte x;

                for (x = 0; x < read_bytes; x++)       // this will call the read function for the passed number times, 
                {                                      // this way it ensures each byte has been correctly recieved while
                    try                                // still using timeouts
                    {
                        USB_PORT.Read(serBuf, x, 1);     // retrieves 1 byte at a time and places in SerBuf at position x
                    }
                    catch (Exception e)                   // timeout or other error occured, set lost comms indicator
                    {
                        serBuf[0] = 255;
                        HandleI2CException("reading", e);
                    }
                }
            }

            private void HandleI2CException(string source, Exception e)
            {
                this.errorOccurred = true;
                Console.WriteLine($"Exception while {source} I2C");

                BusWasReInit?.Invoke(this, EventArgs.Empty);
            }

            public byte I2CWrite(byte i2cAddress, byte value)  // write 1 value to device, no internal addressing
            {
                byte[] buf = new byte[3];

                buf[0] = 0x53;
                buf[1] = (byte)i2cAddress;
                buf[2] = value;
                WriteToPort(3, buf);
                ReadFromPort(1, buf);
                return buf[0];
            }

            public byte I2CWrite(byte i2cAddress, byte numberBytes, byte[] buffer)
            {
                byte[] buf = new byte[numberBytes + 3];

                buf[0] = (byte)IIS_I2C_COMMANDS.I2C_AD0;
                buf[1] = (byte)i2cAddress;
                buf[2] = numberBytes;
                
                for(int i = 0;i<numberBytes; i++) { buf[i + 3] = buffer[i]; }
                
                WriteToPort(numberBytes+3, buf);
                ReadFromPort(1, buf);
                return buf[0];
            }


            public byte I2CWrite(byte i2cAddress, byte address, byte value)
            {
                byte[] buf = new byte[5];

                buf[0] = (byte)IIS_I2C_COMMANDS.I2C_AD0;
                buf[1] = (byte)i2cAddress;
                buf[2] = address;
                buf[3] = 1;
                buf[4] = value;
                WriteToPort(5, buf);
                ReadFromPort(1, buf);
                return buf[0];
            }

            public byte I2CWrite(byte i2cAddress, byte register, byte numberBytes, byte[] buffer)
            {
                if (numberBytes > 59) throw new ArgumentOutOfRangeException("NumberBytes cannot be greater than 59");

                byte[] buf = new byte[3];

                buf[0] = (byte)IIS_I2C_COMMANDS.I2C_AD0;
                buf[1] = (byte)i2cAddress;
                buf[2] = register;
                buf[3] = numberBytes;
                WriteToPort(4, buf);        // !! THIS wont work !!
                WriteToPort(numberBytes, buffer);
                ReadFromPort(1, buf);
                return buf[0];
            }


            public byte I2CRead(byte i2cAddress, byte register)
            {
                byte[] buf = new byte[4];

                buf[0] = (byte)IIS_I2C_COMMANDS.I2C_AD1;
                buf[1] = (byte)(i2cAddress | 0x01);
                buf[2] = register;
                buf[3] = 1;
                WriteToPort(4, buf);
                ReadFromPort(1, buf);
                return buf[0];
            }

            public void I2CRead(byte i2cAddress, byte register, byte numberOfBytes, byte[] outBuffer)
            {
                byte[] buf = new byte[3];

                buf[0] = (byte)IIS_I2C_COMMANDS.I2C_AD1;
                buf[1] = (byte)(i2cAddress | 0x01);
                buf[2] = register;
                buf[3] = numberOfBytes;
                WriteToPort(4, buf);
                ReadFromPort(numberOfBytes, outBuffer);
            }





            public List<byte> ScanBus()  // not working properly....
            {
                List<byte> addresses = new List<byte>();
                for (byte address = 8; address < 128; address += 2)
                {
                    SerBuf[0] = (byte)IIS_I2C_COMMANDS.I2C_TEST;       // Check presence of device @address
                    SerBuf[1] = address;
                    WriteToPort(2, SerBuf);
                    ReadFromPort(1, SerBuf);
                    if (SerBuf[0] != 0)
                    {
                        addresses.Add(address);
                        Console.WriteLine($"found: {address}");
                    }
                    Thread.Sleep(20);

                }
                return addresses;

            }


        }
    }




}
