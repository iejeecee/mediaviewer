#pragma once
#include "HRTimerFactory.h"
#include "Util.h"

using namespace System::Diagnostics;
using namespace System::Threading;

namespace imageviewer
{
	public ref class HRTimerTest
	{
	
		static HRTimer ^timer;
		static Stopwatch ^stopWatch;
		static int counter;
		static AutoResetEvent ^autoEvent;

	private:


		static void tick(Object ^sender, EventArgs ^e) {

			if(counter++ == 100) {

				timer->stop();
				stopWatch->Stop();
				autoEvent->Set();
			}
		}

		static void testTimer(HRTimer ^timer) {

			timer->Tick += gcnew EventHandler(&HRTimerTest::tick);
			
			for(int interval = 100; interval > 0; interval-=5) {

				autoEvent = gcnew AutoResetEvent(false);

				timer->Interval = interval;

				stopWatch = Stopwatch::StartNew();
				timer->start();

				autoEvent->WaitOne();

				double avgTime = double(stopWatch->ElapsedMilliseconds) / counter;

				Util::DebugOut(interval.ToString() + ": " + avgTime.ToString());

				counter = 0;
			}
			
		}

	public:


		static HRTimerTest() {

			stopWatch = gcnew Stopwatch();
		}

		static void test() {

			//timer = HRTimerFactory::create(HRTimerFactory::TimerType::DEFAULT);

			//testTimer(timer);

			timer = HRTimerFactory::create(HRTimerFactory::TimerType::MULTI_MEDIA);

			testTimer(timer);
	
		}

		
	};
}