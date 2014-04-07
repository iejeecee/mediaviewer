using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Timers;

namespace VideoPlayerControl.Timers
{
    public class DefaultTimer : HRTimer
    {

        System.Timers.Timer timer;

        void defaultTimer_Elapsed(Object sender, ElapsedEventArgs e)
        {

            base.OnTick(EventArgs.Empty);
        }

        public DefaultTimer()
        {

            timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(defaultTimer_Elapsed);
        }

        public override ISynchronizeInvoke SynchronizingObject 
        {

            get
            {
                return timer.SynchronizingObject;
            }

            set
            {
                timer.SynchronizingObject = value;
            }
        }
        /// <summary>
        /// Period in milliseconds until the timer fires
        /// </summary>
        public override int Interval
        {
            get
            {
                return ((int)timer.Interval);
            }

            set
            {
                timer.Interval = value;
            }
        }

        /// <summary>
        /// If false run the timer only once, otherwise automatically restart the timer
        /// </summary>
        public override bool AutoReset
        {
            get
            {
                return (timer.AutoReset);
            }

            set
            {
                timer.AutoReset = value;
            }
        }

        public override void start()
        {

            timer.Enabled = true;
            timer.Start();
        }

        public override void stop()
        {

            timer.Stop();
            timer.Enabled = false;
        }

    }

}
