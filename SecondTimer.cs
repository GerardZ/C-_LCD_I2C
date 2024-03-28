using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCD_I2CFrutten
{
    public class SecondTimer
    {
        System.Timers.Timer aTimer;
        public event EventHandler SecondElapsed;
        public event EventHandler MinuteElapsed;
        public event EventHandler HourElapsed;
        public event EventHandler DayElapsed;

        public int granularity { get; set; }

        private int second;
        private int minute;
        private int hour;
        private int day;
        public  int secondOfDay;

        public SecondTimer()
        {
            this.second = DateTime.Now.Second;
            this.minute = DateTime.Now.Minute;
            this.hour = DateTime.Now.Hour;
            this.day = DateTime.Now.Day;

            this.secondOfDay = this.second + this.minute * 60 + this.hour * 3600;

            this.granularity = 10;

            this.StartTimer();
        }

        private void StartTimer()
        {
            aTimer = new System.Timers.Timer(50);
            aTimer.Elapsed += TimerLoop;
            aTimer.Enabled = true;
        }

        protected virtual void TimerLoop(Object source, EventArgs e)
        {
            if (this.second == DateTime.Now.Second) return;
            this.second = DateTime.Now.Second;
            this.secondOfDay++;
            SecondElapsed?.Invoke(this, e);
            if (this.minute == DateTime.Now.Minute) return;
            this.minute = DateTime.Now.Minute;
            MinuteElapsed?.Invoke(this, e);
            if (this.hour == DateTime.Now.Hour) return;
            this.hour = DateTime.Now.Hour;
            HourElapsed?.Invoke(this, e);
            if (this.day == DateTime.Now.Day) return;
            this.day = DateTime.Now.Day;
            this.secondOfDay = 0;
            DayElapsed?.Invoke(this, e);
        }
    }
}
