#pragma once

using namespace System::ComponentModel;

namespace imageviewer
{
	public ref class HRTimer abstract 
	{

	protected:
		
		HRTimer() {

		}

	public:

		event EventHandler ^Tick;

		property ISynchronizeInvoke ^SynchronizingObject {

			virtual ISynchronizeInvoke ^get() = 0;	
			virtual void set(ISynchronizeInvoke ^synchronizingObject) = 0;
		}

		property int Interval
		{
			virtual int get() = 0;
			virtual void set(int interval) = 0;
		}

		property bool AutoReset
		{			
			virtual bool get() = 0;
			virtual void set(bool autoReset) = 0;
		}

		virtual void start() = 0;
		virtual void stop() = 0;

		static double getTimestamp() {
			
			double timeNow = Diagnostics::Stopwatch::GetTimestamp() / double(Diagnostics::Stopwatch::Frequency);
			return(timeNow);
		}
		
	};
}