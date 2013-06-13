#pragma once
#include "HRTimer.h"

using namespace System::ComponentModel;
using namespace System::Timers;

namespace imageviewer
{
	public ref class DefaultTimer : public HRTimer
	{
	private:

		System::Timers::Timer ^timer;

		void defaultTimer_Elapsed(Object ^sender, ElapsedEventArgs ^e) {

			Tick(this, EventArgs::Empty);
		}

	public:

		DefaultTimer() {

			timer = gcnew System::Timers::Timer();
			timer->Elapsed += gcnew ElapsedEventHandler(this, &DefaultTimer::defaultTimer_Elapsed);
		}

		property ISynchronizeInvoke ^SynchronizingObject {

			virtual ISynchronizeInvoke ^get() override
			{
			
				return timer->SynchronizingObject;
			}

			virtual void set(ISynchronizeInvoke ^synchronizingObject) override
			{
				timer->SynchronizingObject = synchronizingObject;
			}
		}

		property int Interval
		{
			virtual int get() override
			{
				return(int(timer->Interval));
			}

			virtual void set(int interval) override
			{
				timer->Interval = interval;
			}
		}

		property bool AutoReset
		{
			virtual bool get() override
			{
				return(timer->AutoReset);
			}

			virtual void set(bool autoReset) override
			{
				timer->AutoReset = autoReset;
			}
		}

		virtual void start() override {

			timer->Enabled = true;
			timer->Start();
		}

		virtual void stop() override {

			timer->Stop();
			timer->Enabled = false;
		}
		
	};
}