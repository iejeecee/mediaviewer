#pragma once
#include "HRTimer.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Diagnostics;
using namespace System::Runtime::InteropServices;

namespace imageviewer
{

	//The exception that is thrown when a timer fails to start.
	public ref class TimerStartException : ApplicationException
	{

		//Initializes a gcnew instance of the TimerStartException class.
	public:

		TimerStartException(String ^message) : ApplicationException(message)
		{
		}
	};

	//Defines constants for the multimedia Timer's event types.
	enum class TimerMode
	{
		//Timer event occurs once.
		ONE_SHOT,
		//Timer event occurs periodically.
		PERIODIC
	};


	//Represents information about the multimedia Timer's capabilities.
	[StructLayout(LayoutKind::Sequential)]
	public ref struct TimerCaps
	{

		//Minimum supported interval in milliseconds.
		int intervalMin;

		//Maximum supported interval in milliseconds.
		int intervalMax;
	};


	//Represents the Windows multimedia timer.
	public ref class MultiMediaTimer : public HRTimer
	{
	public:
		//Occurs when the Timer has started;
		event EventHandler ^Started;

		//Occurs when the Timer has stopped;
		event EventHandler ^Stopped;

		event EventHandler ^Disposed;

	private:

		// Represents the method that is called by Windows when a timer event occurs.
		delegate void TimeProc(int id, int msg, int user, int param1, int param2);

		// Represents methods that raise events.
		delegate void EventRaiser(EventArgs ^e);

		// Gets timer capabilities.
		[DllImport("winmm.dll")]
		static int timeGetDevCaps(TimerCaps ^caps,
			int sizeOfTimerCaps);

		// Creates and starts the timer.
		[DllImport("winmm.dll")]
		static int timeSetEvent(int delay, int resolution,
			TimeProc ^proc, int user, int mode);

		// Stops and destroys the timer.
		[DllImport("winmm.dll")]
		static int timeKillEvent(int id);

		// Indicates that the operation was successful.
		static const int TIMERR_NOERROR = 0;

		// Timer identifier.
		int timerID;

		// Timer mode.
		volatile TimerMode mode;

		// Interval between timer events in milliseconds.
		volatile int interval;

		// Timer resolution in milliseconds.
		volatile int resolution;        

		// Called by Windows when a timer periodic event occurs.
		TimeProc ^timeProcPeriodic;

		// Called by Windows when a timer one shot event occurs.
		TimeProc ^timeProcOneShot;

		// Represents the method that raises the Tick event.
		EventRaiser ^tickRaiser;

		// Indicates whether or not the timer is running.
		bool running;

		// Indicates whether or not the timer has been disposed.
		volatile bool disposed;

		// The ISynchronizeInvoke ^object to use for marshaling events.
		ISynchronizeInvoke ^synchronizingObject;

		// For implementing IComponent.
		ISite ^site;

		// Multimedia timer capabilities.
		static TimerCaps ^caps;

		// Initialize timer with default values.
		void Initialize()
		{
			this->mode = TimerMode::PERIODIC;
			this->interval = Capabilities->intervalMin;
			this->resolution = 1;

			running = false;
			disposed = false;
			synchronizingObject = nullptr;
			site = nullptr;

			timeProcPeriodic = gcnew TimeProc(this, &MultiMediaTimer::TimerPeriodicEventCallback);
			timeProcOneShot = gcnew TimeProc(this, &MultiMediaTimer::TimerOneShotEventCallback);
			tickRaiser = gcnew EventRaiser(this, &MultiMediaTimer::OnTick);
		}

		// Callback method called by the Win32 multimedia timer when a timer
		// periodic event occurs.
		void TimerPeriodicEventCallback(int id, int msg, int user, int param1, int param2)
		{
			if(synchronizingObject != nullptr)
			{
				synchronizingObject->BeginInvoke(tickRaiser, gcnew array<Object ^>{ EventArgs::Empty });
			}
			else
			{
				OnTick(EventArgs::Empty);
			}
		}

		// Callback method called by the Win32 multimedia timer when a timer
		// one shot event occurs.
		void TimerOneShotEventCallback(int id, int msg, int user, int param1, int param2)
		{
			if(synchronizingObject != nullptr)
			{
				synchronizingObject->BeginInvoke(tickRaiser, gcnew array<Object ^>{ EventArgs::Empty });
				stop();
			}
			else
			{
				OnTick(EventArgs::Empty);
				stop();
			}
		}

		// Raises the Disposed event.
		void OnDisposed(EventArgs ^e)
		{
			Disposed(this, e);
		}

		// Raises the Started event.
		void OnStarted(EventArgs ^e)
		{
			Started(this, e);
		}

		// Raises the Stopped event.
		void OnStopped(EventArgs ^e)
		{
			Stopped(this, e);
		}

		// Raises the Tick event.
		void OnTick(EventArgs ^e)
		{
			Tick(this, e);
		}

public:

		//Initialize class.
		static MultiMediaTimer()
		{
			caps = gcnew TimerCaps();
			// Get multimedia timer capabilities.
			timeGetDevCaps(caps, Marshal::SizeOf(caps));
		}

		//Initializes a gcnew instance of the Timer class.
		MultiMediaTimer()
		{
			Initialize();
		}

		~MultiMediaTimer()
		{
			if(IsRunning)
			{
				// Stop and destroy timer.
				timeKillEvent(timerID);
			}
		}

		//Frees timer resources.
		!MultiMediaTimer() 
		{
			if(disposed) {
				return;
			}

			if(IsRunning) {

				stop();
			}

			disposed = true;
			OnDisposed(EventArgs::Empty);
		}

		//Starts the timer.
		//<exception cref="ObjectDisposedException">
		//The timer has already been disposed.
		//</exception>
		//<exception cref="TimerStartException">
		//The timer failed to start.
		//</exception>
		virtual void start() override
		{
			if(disposed)
			{
				throw gcnew ObjectDisposedException("Timer");
			}

			if(IsRunning)
			{
				return;
			}

			// If the periodic event callback should be used.
			if(mode == TimerMode::PERIODIC)
			{
				// Create and start timer.
				timerID = timeSetEvent(Interval, Resolution, timeProcPeriodic, 0, (int)mode);
			}
			// Else the one shot event callback should be used.
			else
			{
				// Create and start timer.
				timerID = timeSetEvent(Interval, Resolution, timeProcOneShot, 0, (int)mode);
			}

			// If the timer was created successfully.
			if(timerID != 0)
			{
				running = true;

				if(SynchronizingObject != nullptr && SynchronizingObject->InvokeRequired)
				{
					SynchronizingObject->BeginInvoke(
						gcnew EventRaiser(this, &MultiMediaTimer::OnStarted), 
						gcnew array<Object ^>{ EventArgs::Empty });
				}
				else
				{
					OnStarted(EventArgs::Empty);
				}                
			}
			else
			{
				throw gcnew TimerStartException("Unable to start multimedia Timer.");
			}
		}

		//Stops timer.
		//<exception cref="ObjectDisposedException">
		//If the timer has already been disposed.
		//</exception>
		virtual void stop() override
		{
			if(disposed)
			{
				throw gcnew ObjectDisposedException("Timer");
			}

			if(!running)
			{
				return;
			}

			// Stop and destroy timer.
			int result = timeKillEvent(timerID);

			Debug::Assert(result == TIMERR_NOERROR);
			running = false;

			if(SynchronizingObject != nullptr && SynchronizingObject->InvokeRequired)
			{
				SynchronizingObject->BeginInvoke(
					gcnew EventRaiser(this, &MultiMediaTimer::OnStopped), 
					gcnew array<Object ^>{ EventArgs::Empty });
			}
			else
			{
				OnStopped(EventArgs::Empty);
			}
		}        

	
		//Gets or sets the object used to marshal event-handler calls.
		property ISynchronizeInvoke ^SynchronizingObject
		{
			virtual ISynchronizeInvoke ^get() override
			{
				if(disposed)
				{
					throw gcnew ObjectDisposedException("Timer");
				}

				return synchronizingObject;
			}

			virtual void set(ISynchronizeInvoke ^value) override
			{
				if(disposed)
				{
					throw gcnew ObjectDisposedException("Timer");
				}

				synchronizingObject = value;
			}
		}

		//Gets or sets the time between Tick events.
		//<exception cref="ObjectDisposedException">
		//If the timer has already been disposed.
		//</exception>   
		property int Interval
		{
			virtual int get() override
			{

				if(disposed)
				{
					throw gcnew ObjectDisposedException("Timer");
				}

				return interval;
			}

			virtual void set(int value) override
			{
				if(disposed)
				{
					throw gcnew ObjectDisposedException("Timer");

				} else if(value < Capabilities->intervalMin || value > Capabilities->intervalMax) {

					throw gcnew ArgumentOutOfRangeException("Interval", value,
						"Multimedia Timer interval out of range.");
				}

				interval = value;

				if(IsRunning) {

					stop();
					start();
				}
			}
		}

		//Gets or sets the timer resolution.
		//<exception cref="ObjectDisposedException">
		//If the timer has already been disposed.
		//</exception>        
		//<remarks>
		//The resolution is in milliseconds. The resolution increases 
		//with smaller values; a resolution of 0 indicates periodic events 
		//should occur with the greatest possible accuracy. To reduce system 
		//overhead, however, you should use the maximum value appropriate 
		//for your application.
		//</remarks>
		property int Resolution
		{
			int get()
			{
				if(disposed)
				{
					throw gcnew ObjectDisposedException("Timer");
				}

				return resolution;
			}

			void set(int value)
			{
				if(disposed)
				{
					throw gcnew ObjectDisposedException("Timer");
				}
				else if(value < 0)
				{
					throw gcnew ArgumentOutOfRangeException("Resolution", value,
						"Multimedia timer resolution out of range.");
				}

				resolution = value;

				if(IsRunning)
				{
					stop();
					start();
				}
			}
		}

		property bool AutoReset
		{
			virtual bool get() override
			{
				if(disposed)
				{
					throw gcnew ObjectDisposedException("Timer");
				}

				return mode == TimerMode::PERIODIC ? true : false;
			}

			virtual void set(bool autoReset) override
			{
				if(disposed)
				{
					throw gcnew ObjectDisposedException("Timer");
				}

				mode = (autoReset == true) ? TimerMode::PERIODIC : TimerMode::ONE_SHOT;

				if(IsRunning)
				{
					stop();
					start();
				}
			}
		}	

		//Gets a value indicating whether the Timer is running.
		property bool IsRunning
		{
			bool get() {

				return running;
			}
		}

		//Gets the timer capabilities.
		static property TimerCaps ^Capabilities
		{
			TimerCaps ^get()
			{
				return caps;
			}
		}

		property ISite ^Site
		{
			ISite ^get()
			{
				return site;
			}
			void set(ISite ^value)
			{
				site = value;
			}
		}

		
       
	};




}