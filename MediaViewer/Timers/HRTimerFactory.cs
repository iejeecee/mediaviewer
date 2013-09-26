using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Timers
{

    public class HRTimerFactory
    {

        public enum TimerType
        {
            DEFAULT,
            MULTI_MEDIA,
            TIMER_QUEUE
        }

        public static HRTimer create(TimerType type)
        {

            HRTimer timer = null;

            switch (type)
            {

                case TimerType.DEFAULT:
                    {
                        timer = new DefaultTimer();
                        break;
                    }
                case TimerType.MULTI_MEDIA:
                    {

                        timer = new MultiMediaTimer();
                        break;
                    }
                case TimerType.TIMER_QUEUE:
                    {
                        timer = new TimerQueueTimer();
                        break;
                    }

            }

            return (timer);
        }

    }

}
