using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCD_I2CFrutten
{
    public class BME280
    {

        private I2CPort.USB_ISS i2c;
        private byte I2CBmeAddress;
        public BME280(I2CPort.USB_ISS uSB_ISS)
        {
            this.i2c = uSB_ISS;
            this.I2CBmeAddress = 0x76;

            Init();
        }

        private bool Init()
        {
            byte _sensorID = this.i2c.I2CRead(this.I2CBmeAddress, (byte)Bme280_Register.CHIPID);
            if (_sensorID != 0x60) return false;
            Console.WriteLine("BME found !!!");
            return true;
        }
    }


    public enum Bme280_Register
    {
        DIG_T1 = 0x88,
        DIG_T2 = 0x8A,
        DIG_T3 = 0x8C,

        DIG_P1 = 0x8E,
        DIG_P2 = 0x90,
        DIG_P3 = 0x92,
        DIG_P4 = 0x94,
        DIG_P5 = 0x96,
        DIG_P6 = 0x98,
        DIG_P7 = 0x9A,
        DIG_P8 = 0x9C,
        DIG_P9 = 0x9E,

        DIG_H1 = 0xA1,
        DIG_H2 = 0xE1,
        DIG_H3 = 0xE3,
        DIG_H4 = 0xE4,
        DIG_H5 = 0xE5,
        DIG_H6 = 0xE7,

        CHIPID = 0xD0,
        VERSION = 0xD1,
        SOFTRESET = 0xE0,

        CAL26 = 0xE1, // R calibration stored in 0xE1-0xF0

        CONTROLHUMID = 0xF2,
        STATUS = 0XF3,
        CONTROL = 0xF4,
        CONFIG = 0xF5,
        PRESSUREDATA = 0xF7,
        TEMPDATA = 0xFA,
        HUMIDDATA = 0xFD
    }
}
