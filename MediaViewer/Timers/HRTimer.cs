using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace MediaViewer.Timers
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

        public abstract int Interval
        {
            get;
            set;
        }

        public abstract bool AutoReset
        {
            get;
            set;
        }

        public abstract void start();
        public abstract void stop();

        public static double getTimestamp()
        {

            double timeNow = System.Diagnostics.Stopwatch.GetTimestamp() / (double)(System.Diagnostics.Stopwatch.Frequency);
            return (timeNow);
        }

    }

}

