using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace VideoPlayerControl.Timers
{

	public class HRTimerTest
	{
	
		static HRTimer timer;
		static Stopwatch stopWatch;
		static int counter;
		static AutoResetEvent autoEvent;

		static void tick1(Object sender, EventArgs e) {

			if(counter++ == 100) {

				timer.stop();
				stopWatch.Stop();
				autoEvent.Set();
			}
		}

		static void testAverageTimerAccuracy(HRTimer timer) {

			timer.Tick += new EventHandler(tick1);
			
			for(int interval = 100; interval > 0; interval-=5) {

				autoEvent = new AutoResetEvent(false);

				timer.Interval = interval;

				stopWatch = Stopwatch.StartNew();
				timer.start();

				autoEvent.WaitOne();

				double avgTime = (double)(stopWatch.ElapsedMilliseconds) / counter;

				Debug.WriteLine("Test period: " + interval.ToString() + " (ms) : " + avgTime.ToString() + " avg period (ms)");

				counter = 0;
			}
			
		}

        static void tick2(Object sender, EventArgs e)
        {

            counter++;
            autoEvent.Set();            
        }

        static void testTimerAutoResetFunctionality(HRTimer timer)
        {

            timer.Tick += new EventHandler(tick2);
            timer.AutoReset = false;

            for (int interval = 100; interval > 0; interval -= 5)
            {
                counter = 0;
                autoEvent = new AutoResetEvent(false);

                timer.Interval = interval;
                timer.start();

                autoEvent.WaitOne();
                Thread.Sleep(interval * 3);

                Debug.WriteLine(interval.ToString() + ": " + counter.ToString());
               
            }

        }

	
		static HRTimerTest() {

			stopWatch = new Stopwatch();
		}

		public static void test() {

            timer = HRTimerFactory.create(HRTimerFactory.TimerType.TIMER_QUEUE);
			//timer = HRTimerFactory.create(HRTimerFactory.TimerType.DEFAULT);
            //timer = HRTimerFactory.create(HRTimerFactory.TimerType.MULTI_MEDIA);

          

			//testTimerAccuracy(timer);
            testTimerAutoResetFunctionality(timer);

			

        
	
		}

		
	}

}
