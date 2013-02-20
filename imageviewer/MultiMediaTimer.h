#pragma once

/* Copyright (c) 2006 Leslie Sanford
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy 
* of this software and associated documentation files (the "Software"), to 
* deal in the Software without restriction, including without limitation the 
* rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
* sell copies of the Software, and to permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software. 
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
* THE SOFTWARE.
*/


using namespace System;
using namespace System.ComponentModel;
using namespace System.Diagnostics;
using namespace System.Runtime.InteropServices;

namespace imageviewer
{

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
	public struct TimerCaps
	{

		//Minimum supported period in milliseconds.

		public int periodMin;


		//Maximum supported period in milliseconds.

		public int periodMax;
	}


	//Represents the Windows multimedia timer.

	public ref class MultiMediaTimer : IComponent
	{

	private:

		// Represents the method that is called by Windows when a timer event occurs.
		delegate void TimeProc(int id, int msg, int user, int param1, int param2);

		// Represents methods that raise events.
		delegate void EventRaiser(EventArgs e);

		// Gets timer capabilities.
		[DllImport("winmm.dll")]
		static int timeGetDevCaps(ref TimerCaps caps,
			int sizeOfTimerCaps);

		// Creates and starts the timer.
		[DllImport("winmm.dll")]
		static int timeSetEvent(int delay, int resolution,
			TimeProc proc, int user, int mode);

		// Stops and destroys the timer.
		[DllImport("winmm.dll")]
		static int timeKillEvent(int id);

		// Indicates that the operation was successful.
		const int TIMERR_NOERROR = 0;


		// Timer identifier.
		int timerID;

		// Timer mode.
		volatile TimerMode mode;

		// Period between timer events in milliseconds.
		volatile int period;

		// Timer resolution in milliseconds.
		volatile int resolution;        

		// Called by Windows when a timer periodic event occurs.
		TimeProc timeProcPeriodic;

		// Called by Windows when a timer one shot event occurs.
		TimeProc timeProcOneShot;

		// Represents the method that raises the Tick event.
		EventRaiser tickRaiser;

		// Indicates whether or not the timer is running.
		bool running = false;

		// Indicates whether or not the timer has been disposed.
		volatile bool disposed = false;

		// The ISynchronizeInvoke object to use for marshaling events.
		ISynchronizeInvoke synchronizingObject = null;

		// For implementing IComponent.
		ISite site = null;

		// Multimedia timer capabilities.
		static TimerCaps caps;






		//Occurs when the Timer has started;

		public event EventHandler Started;


		//Occurs when the Timer has stopped;

		public event EventHandler Stopped;


		//Occurs when the time period has elapsed.

		public event EventHandler Tick;





		//Initialize class.

		static Timer()
		{
			// Get multimedia timer capabilities.
			timeGetDevCaps(ref caps, Marshal.SizeOf(caps));
		}


		//Initializes a new instance of the Timer class with the specified IContainer.

		//<param name="container">
		//The IContainer to which the Timer will add itself.
		//</param>
		public Timer(IContainer container)
		{
			///
			//Required for Windows.Forms Class Composition Designer support
			///
			container.Add(this);

			Initialize();
		}


		//Initializes a new instance of the Timer class.

		public Timer()
		{
			Initialize();
		}

		~Timer()
		{
			if(IsRunning)
			{
				// Stop and destroy timer.
				timeKillEvent(timerID);
			}
		}

		// Initialize timer with default values.
		void Initialize()
		{
			this.mode = TimerMode.PERIODIC;
			this.period = Capabilities.periodMin;
			this.resolution = 1;

			running = false;

			timeProcPeriodic = new TimeProc(TimerPeriodicEventCallback);
			timeProcOneShot = new TimeProc(TimerOneShotEventCallback);
			tickRaiser = new EventRaiser(OnTick);
		}






		//Starts the timer.

		//<exception cref="ObjectDisposedException">
		//The timer has already been disposed.
		//</exception>
		//<exception cref="TimerStartException">
		//The timer failed to start.
		//</exception>
		public void Start()
		{


			if(disposed)
			{
				throw new ObjectDisposedException("Timer");
			}





			if(IsRunning)
			{
				return;
			}



			// If the periodic event callback should be used.
			if(Mode == TimerMode.PERIODIC)
			{
				// Create and start timer.
				timerID = timeSetEvent(Period, Resolution, timeProcPeriodic, 0, (int)Mode);
			}
			// Else the one shot event callback should be used.
			else
			{
				// Create and start timer.
				timerID = timeSetEvent(Period, Resolution, timeProcOneShot, 0, (int)Mode);
			}

			// If the timer was created successfully.
			if(timerID != 0)
			{
				running = true;

				if(SynchronizingObject != null && SynchronizingObject.InvokeRequired)
				{
					SynchronizingObject.BeginInvoke(
						new EventRaiser(OnStarted), 
						new object[] { EventArgs.Empty });
				}
				else
				{
					OnStarted(EventArgs.Empty);
				}                
			}
			else
			{
				throw new TimerStartException("Unable to start multimedia Timer.");
			}
		}


		//Stops timer.

		//<exception cref="ObjectDisposedException">
		//If the timer has already been disposed.
		//</exception>
		public void Stop()
		{


			if(disposed)
			{
				throw new ObjectDisposedException("Timer");
			}





			if(!running)
			{
				return;
			}



			// Stop and destroy timer.
			int result = timeKillEvent(timerID);

			Debug.Assert(result == TIMERR_NOERROR);

			running = false;

			if(SynchronizingObject != null && SynchronizingObject.InvokeRequired)
			{
				SynchronizingObject.BeginInvoke(
					new EventRaiser(OnStopped), 
					new object[] { EventArgs.Empty });
			}
			else
			{
				OnStopped(EventArgs.Empty);
			}
		}        


		// Callback method called by the Win32 multimedia timer when a timer
		// periodic event occurs.
		void TimerPeriodicEventCallback(int id, int msg, int user, int param1, int param2)
		{
			if(synchronizingObject != null)
			{
				synchronizingObject.BeginInvoke(tickRaiser, new object[] { EventArgs.Empty });
			}
			else
			{
				OnTick(EventArgs.Empty);
			}
		}

		// Callback method called by the Win32 multimedia timer when a timer
		// one shot event occurs.
		void TimerOneShotEventCallback(int id, int msg, int user, int param1, int param2)
		{
			if(synchronizingObject != null)
			{
				synchronizingObject.BeginInvoke(tickRaiser, new object[] { EventArgs.Empty });
				Stop();
			}
			else
			{
				OnTick(EventArgs.Empty);
				Stop();
			}
		}



		// Raises the Disposed event.
		void OnDisposed(EventArgs e)
		{
			EventHandler handler = Disposed;

			if(handler != null)
			{
				handler(this, e);
			}
		}

		// Raises the Started event.
		void OnStarted(EventArgs e)
		{
			EventHandler handler = Started;

			if(handler != null)
			{
				handler(this, e);
			}
		}

		// Raises the Stopped event.
		void OnStopped(EventArgs e)
		{
			EventHandler handler = Stopped;

			if(handler != null)
			{
				handler(this, e);
			}
		}

		// Raises the Tick event.
		void OnTick(EventArgs e)
		{
			EventHandler handler = Tick;

			if(handler != null)
			{
				handler(this, e);
			}
		}

        






		//Gets or sets the object used to marshal event-handler calls.

		public ISynchronizeInvoke SynchronizingObject
		{
			get
			{


				if(disposed)
				{
					throw new ObjectDisposedException("Timer");
				}



				return synchronizingObject;
			}
			set
			{


				if(disposed)
				{
					throw new ObjectDisposedException("Timer");
				}



				synchronizingObject = value;
			}
		}


		//Gets or sets the time between Tick events.

		//<exception cref="ObjectDisposedException">
		//If the timer has already been disposed.
		//</exception>   
		public int Period
		{
			get
			{


				if(disposed)
				{
					throw new ObjectDisposedException("Timer");
				}



				return period;
			}
			set
			{


				if(disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				else if(value < Capabilities.periodMin || value > Capabilities.periodMax)
				{
					throw new ArgumentOutOfRangeException("Period", value,
						"Multimedia Timer period out of range.");
				}



				period = value;

				if(IsRunning)
				{
					Stop();
					Start();
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
		public int Resolution
		{
			get
			{

				if(disposed)
				{
					throw new ObjectDisposedException("Timer");
				}



				return resolution;
			}
			set
			{

				if(disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				else if(value < 0)
				{
					throw new ArgumentOutOfRangeException("Resolution", value,
						"Multimedia timer resolution out of range.");
				}



				resolution = value;

				if(IsRunning)
				{
					Stop();
					Start();
				}
			}
		}


		//Gets the timer mode.

		//<exception cref="ObjectDisposedException">
		//If the timer has already been disposed.
		//</exception>
		public TimerMode Mode
		{
			get
			{


				if(disposed)
				{
					throw new ObjectDisposedException("Timer");
				}



				return mode;
			}
			set
			{


				if(disposed)
				{
					throw new ObjectDisposedException("Timer");
				}



				mode = value;

				if(IsRunning)
				{
					Stop();
					Start();
				}
			}
		}


		//Gets a value indicating whether the Timer is running.

		public bool IsRunning
		{
			get
			{
				return running;
			}
		}


		//Gets the timer capabilities.

		public static TimerCaps Capabilities
		{
			get
			{
				return caps;
			}
		}


		public event System.EventHandler Disposed;

		public ISite Site
		{
			get
			{
				return site;
			}
			set
			{
				site = value;
			}
		}


		//Frees timer resources.

		public void Dispose()
		{


			if(disposed)
			{
				return;
			}

               

			if(IsRunning)
			{
				Stop();
			}

			disposed = true;

			OnDisposed(EventArgs.Empty);
		}

       
	};


	//The exception that is thrown when a timer fails to start.

	public class TimerStartException : ApplicationException
	{

		//Initializes a new instance of the TimerStartException class.

		//<param name="message">
		//The error message that explains the reason for the exception. 
		//</param>
		public TimerStartException(string message) : base(message)
		{
		}
	}

}