using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace VideoPlayerControl.Timers
{

    public abstract class HRTimer
    {
        protected HRTimer()
        {

        }

        public event EventHandler Tick;

        protected virtual void OnTick(EventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler handler = Tick;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public abstract ISynchronizeInvoke SynchronizingObject
        {

            get;
            set;
        }
        /// <summary>
        /// Interval after which the timer elapses in milliseconds
        /// </summary>
        public abstract int Interval
        {
            get;
            set;
        }
        /// <summary>
        /// Automatically restart the timer after it elapses
        /// </summary>
        public abstract bool AutoReset
        {
            get;
            set;
        }
        /// <summary>
        /// Start the timer
        /// </summary>
        public abstract void start();
        /// <summary>
        /// Stop the timer
        /// </summary>
        public abstract void stop();

        public static double getTimestamp()
        {

            double timeNow = System.Diagnostics.Stopwatch.GetTimestamp() / (double)(System.Diagnostics.Stopwatch.Frequency);
            return (timeNow);
        }

        object tag;

        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }
    }

}

