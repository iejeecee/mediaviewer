using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace MediaViewer.Timers
{

	public class HRTimerTest
	{
	
		static HRTimer timer;
		static Stopwatch stopWatch;
		static int counter;
		static AutoResetEvent autoEvent;

		static void tick(Object sender, EventArgs e) {

			if(counter++ == 100) {

				timer.stop();
				stopWatch.Stop();
				autoEvent.Set();
			}
		}

		static void testTimer(HRTimer timer) {

			timer.Tick += new EventHandler(tick);
			
			for(int interval = 100; interval > 0; interval-=5) {

				autoEvent = new AutoResetEvent(false);

				timer.Interval = interval;

				stopWatch = Stopwatch.StartNew();
				timer.start();

				autoEvent.WaitOne();

				double avgTime = (double)(stopWatch.ElapsedMilliseconds) / counter;

				Debug.WriteLine(interval.ToString() + ": " + avgTime.ToString());

				counter = 0;
			}
			
		}

	
		static HRTimerTest() {

			stopWatch = new Stopwatch();
		}

		public static void test() {

			//timer = HRTimerFactory.create(HRTimerFactory.TimerType.DEFAULT);

			//testTimer(timer);

			timer = HRTimerFactory.create(HRTimerFactory.TimerType.TIMER_QUEUE);

			testTimer(timer);
	
		}

		
	}

}
