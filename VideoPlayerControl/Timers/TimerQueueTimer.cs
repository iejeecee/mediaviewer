using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace VideoPlayerControl.Timers
{
    [Serializable]
	public class TimerQueueTimerException : Exception
    {
	
        public TimerQueueTimerException(String message) : base(message)
        {
        }

        public TimerQueueTimerException(String message, Exception innerException) : base(message, innerException)
        {
        }
    };

	public class TimerQueueTimer : HRTimer, IDisposable
    {
		 	
		delegate void WaitOrTimerDelegate(IntPtr lpParameter, bool timerOrWaitFired); 

		enum Flag {
            WT_EXECUTEDEFAULT = 0x00000000,
            WT_EXECUTEINIOTHREAD = 0x00000001,
            //WT_EXECUTEINWAITTHREAD       = 0x00000004,
            WT_EXECUTEONLYONCE = 0x00000008,
            WT_EXECUTELONGFUNCTION = 0x00000010,
            WT_EXECUTEINTIMERTHREAD = 0x00000020,
            WT_EXECUTEINPERSISTENTTHREAD = 0x00000080,
            //WT_TRANSFER_IMPERSONATION    = 0x00000100
        };

        [DllImport("kernel32.dll")]
        static extern bool CreateTimerQueueTimer(
            out IntPtr timerHandle,          // timerHandle - Pointer to a handle; this is an out value
            IntPtr TimerQueue,              // TimerQueue - timer  queue handle. For the default timer queue, NULL
            WaitOrTimerDelegate Callback,   // callback  - Pointer to the callback function
            IntPtr Parameter,               // Parameter - Value passed to the callback function
            int DueTime,                   // DueTime - time  (milliseconds), before the timer is set to the signaled state for the first time 
            int Period,                    // Period - Timer period (milliseconds). If zero, timer is signaled only once
            int Flags                      // Flags - One or more of the next values (table taken from MSDN):
                                            // WT_EXECUTEINTIMERTHREAD 	The callback function  is invoked  by the timer thread itself. This flag should be used only for short tasks or it could affect other timer operations.
                                            // WT_EXECUTEINIOTHREAD 	The callback function is queued to an I/O worker thread. This flag should be used if the function should be executed in a thread that waits in an alertable state.

                                            // The callback function is queued as an APC. Be sure to address reentrancy issues if the function performs an alertable wait operation.
                                            // WT_EXECUTEINPERSISTENTTHREAD 	The callback function is queued to a thread that never terminates. This flag should be used only for short tasks or it could affect other timer operations.

                                            // Note that currently no worker thread is persistent, although no worker thread will terminate if there are any pending I/O requests.
                                            // WT_EXECUTELONGFUNCTION 	Specifies that the callback function can perform a long wait. This flag helps the system to decide if it should create a new thread.
                                            // WT_EXECUTEONLYONCE 	The timer will be set to the signaled state only once.
            );

        [DllImport("kernel32.dll")]
        static extern bool DeleteTimerQueueTimer(
            IntPtr timerQueue,              // TimerQueue - A handle to the (default) timer queue
            IntPtr timer,                   // Timer - A handle to the timer
            IntPtr completionEvent          // CompletionEvent - A handle to an optional event to be signaled when the function is successful and all callback functions have completed. Can be NULL.
            );


        [DllImport("kernel32.dll")]
        static extern bool DeleteTimerQueue(IntPtr TimerQueue);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

		IntPtr timerHandle; 
		bool running;
		bool autoReset;
		int interval;
		WaitOrTimerDelegate callback;
		ISynchronizeInvoke synchronizingObject;

		void timerOrWaitFired(IntPtr lpParameter, bool timerOrWaitFired) {

			if(autoReset == false) {

				running = false;
			}

			OnTick(EventArgs.Empty);
		}
	
        public TimerQueueTimer()
        {
     
			running = false;
			timerHandle = IntPtr.Zero;
			autoReset = true;
			synchronizingObject = null;

			callback = new WaitOrTimerDelegate(timerOrWaitFired);
        }

        ~TimerQueueTimer()
        {
            Dispose(false);
        }

		public void Dispose() 
		{
            Dispose(true);
		}

        protected virtual void Dispose(bool safe)
        {
            if (running == true)
            {
                stop();
            }
           
        }

        public override void start() 
        {

			if(running == true) return;

			IntPtr pParameter = IntPtr.Zero;

			int executeThread = synchronizingObject == null ? 
				(int)Flag.WT_EXECUTEDEFAULT :
				(int)Flag.WT_EXECUTEINTIMERTHREAD;

			int period = interval;
			
			if(autoReset == false) {

				period = 0;
				executeThread |= (int)Flag.WT_EXECUTEONLYONCE;
			} 
			
            bool success = CreateTimerQueueTimer(
                // Timer handle
                out timerHandle,
                // Default timer queue. IntPtr.Zero is just a constant value that represents a null pointer.
				IntPtr.Zero,
                // Timer callback function
                callback,
                // Callback function parameter
                pParameter,
                // Time (milliseconds), before the timer is set to the signaled state for the first time.
                interval,
                // Period - Timer period (milliseconds). If zero, timer is signaled only once.
                period,
				executeThread);

			if(!success) {
                
				throw new TimerQueueTimerException("Error creating QueueTimer");
			} else {

				running = true;
			}
        }

        public override void stop() 
        {
			if(running == false) return;

            //bool success = DeleteTimerQueue(IntPtr.Zero);
            bool success = DeleteTimerQueueTimer(
				IntPtr.Zero, // TimerQueue - A handle to the (default) timer queue
                timerHandle,  // Timer - A handle to the timer
				IntPtr.Zero  // CompletionEvent - A handle to an optional event to be signaled when the function is successful and all callback functions have completed. Can be NULL.
                );

			//if(!success) {
                
			//	throw gcnew TimerQueueTimerException("Error deleting QueueTimer");
				
			//} else {

				running = false;
			//}
			
           // CloseHandle(timerHandle);
			int error = Marshal.GetLastWin32Error();
        }

		public override ISynchronizeInvoke SynchronizingObject
		{
			get 
			{
			
				return(synchronizingObject);
			}

			set
			{
				this.synchronizingObject = value;
			}
		}

        public override bool AutoReset
		{
			get
			{
				
				return autoReset;
			}

			set
			{				
				autoReset = value;

				if(IsRunning)
				{
					stop();
					start();
				}
			}
		}

        public override int Interval
		{
			get
			{

				return interval;
			}

			set
			{			
				this.interval = value;

				if(IsRunning) {

					stop();
					start();
				}
			}
		}

        public bool IsRunning
		{
			get {

				return running;
			}
		}


/*
		override void Dispose() override
        {
            Delete();
        }
*/
    };

  
}
