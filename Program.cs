using System;
using System.Collections.Generic;
using System.Threading;

namespace LCD_I2CFrutten
{
    class Program
    {
        static I2CLcd lcd;
        static I2CPort.USB_ISS i2c;
        static BME280 bme280;


        static void Main(string[] args)
        {
            i2c = new I2CPort.USB_ISS("Com3");

            lcd = new I2CLcd(i2c, 0x4E, I2CLcd.Pin.BL);

            bme280 = new BME280(i2c);



            List<byte> devices = i2c.ScanBus();

            foreach (byte device in devices)
            {
                Console.WriteLine($"Device found  @{device}({device.ToString("x16")})");
            }

            Thread.Sleep(3000);

            Console.WriteLine("");

            lcd.Write(0, 0, "Hij dut t...");



            SecondTimer timer = new SecondTimer();
            timer.SecondElapsed += HandleSecond;

            while (true)
            {

                Thread.Sleep(500);
            }

        }

        private static void HandleSecond(object sender, EventArgs e)
        {
            WriteToConsoleAndLcd(0, 0, $"{DateTime.Now.ToString("ddd, dd MMM yyy")}");
            WriteToConsoleAndLcd(0, 1, $"{DateTime.Now.ToString("HH:mm:ss 'GMT+1'")}");
        }

        private static void WriteToConsoleAndLcd(int x, int y, string str)
        {
            Console.SetCursorPosition(x, Console.CursorTop + y);
            Console.Write(str);
            Console.SetCursorPosition(x, Console.CursorTop - y);

            lcd.Write(x, y, str);
        }
    }
}
