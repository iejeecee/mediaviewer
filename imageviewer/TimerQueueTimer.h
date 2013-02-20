#pragma once

namespace imageviewer
{
	public ref class QueueTimerException : Exception
    {
	public:
        QueueTimerException(String ^message) : Exception(message)
        {
        }

        QueueTimerException(String ^message, Exception ^innerException) : Exception(message, innerException)
        {
        }
    };

    public ref class TimerQueueTimer 
    {

	public:

		 delegate void WaitOrTimerDelegate(IntPtr lpParameter, bool timerOrWaitFired); 

	private:

		enum class Flag {
            WT_EXECUTEDEFAULT = 0x00000000,
            WT_EXECUTEINIOTHREAD = 0x00000001,
            //WT_EXECUTEINWAITTHREAD       = 0x00000004,
            WT_EXECUTEONLYONCE = 0x00000008,
            WT_EXECUTELONGFUNCTION = 0x00000010,
            WT_EXECUTEINTIMERTHREAD = 0x00000020,
            WT_EXECUTEINPERSISTENTTHREAD = 0x00000080,
            //WT_TRANSFER_IMPERSONATION    = 0x00000100
        };

        IntPtr phNewTimer; // Handle to the timer.


        [DllImport("kernel32.dll")]
        static bool CreateTimerQueueTimer(
            IntPtr phNewTimer,          // phNewTimer - Pointer to a handle; this is an out value
            IntPtr TimerQueue,              // TimerQueue - timer  queue handle. For the default timer queue, NULL
            WaitOrTimerDelegate ^Callback,   // callback  - Pointer to the callback function
            IntPtr Parameter,               // Parameter - Value passed to the callback function
            unsigned int DueTime,                   // DueTime - time  (milliseconds), before the timer is set to the signaled state for the first time 
            unsigned int Period,                    // Period - Timer period (milliseconds). If zero, timer is signaled only once
            unsigned int Flags                      // Flags - One or more of the next values (table taken from MSDN):
                                            // WT_EXECUTEINTIMERTHREAD 	The callback function  is invoked  by the timer thread itself. This flag should be used only for short tasks or it could affect other timer operations.
                                            // WT_EXECUTEINIOTHREAD 	The callback function is queued to an I/O worker thread. This flag should be used if the function should be executed in a thread that waits in an alertable state.

                                            // The callback function is queued as an APC. Be sure to address reentrancy issues if the function performs an alertable wait operation.
                                            // WT_EXECUTEINPERSISTENTTHREAD 	The callback function is queued to a thread that never terminates. This flag should be used only for short tasks or it could affect other timer operations.

                                            // Note that currently no worker thread is persistent, although no worker thread will terminate if there are any pending I/O requests.
                                            // WT_EXECUTELONGFUNCTION 	Specifies that the callback function can perform a long wait. This flag helps the system to decide if it should create a new thread.
                                            // WT_EXECUTEONLYONCE 	The timer will be set to the signaled state only once.
            );

        [DllImport("kernel32.dll")]
        static bool DeleteTimerQueueTimer(
            IntPtr timerQueue,              // TimerQueue - A handle to the (default) timer queue
            IntPtr timer,                   // Timer - A handle to the timer
            IntPtr completionEvent          // CompletionEvent - A handle to an optional event to be signaled when the function is successful and all callback functions have completed. Can be NULL.
            );


        [DllImport("kernel32.dll")]
        static bool DeleteTimerQueue(IntPtr TimerQueue);

        [DllImport("kernel32.dll", SetLastError = true)]
        static bool CloseHandle(IntPtr hObject);

	public:

       

        TimerQueueTimer()
        {

        }

		~TimerQueueTimer() 
		{
			Delete();
		}

        void Create(unsigned int dueTime, unsigned int period, WaitOrTimerDelegate ^callbackDelegate)
        {
			IntPtr pParameter = IntPtr::Zero;

            bool success = CreateTimerQueueTimer(
                // Timer handle
                phNewTimer,
                // Default timer queue. IntPtr.Zero is just a constant value that represents a null pointer.
				IntPtr::Zero,
                // Timer callback function
                callbackDelegate,
                // Callback function parameter
                pParameter,
                // Time (milliseconds), before the timer is set to the signaled state for the first time.
                dueTime,
                // Period - Timer period (milliseconds). If zero, timer is signaled only once.
                period,
				(unsigned int)Flag::WT_EXECUTEINIOTHREAD);

            if (!success)
                throw gcnew QueueTimerException("Error creating QueueTimer");
        }

        void Delete()
        {
            //bool success = DeleteTimerQueue(IntPtr.Zero);
            bool success = DeleteTimerQueueTimer(
				IntPtr::Zero, // TimerQueue - A handle to the (default) timer queue
                phNewTimer,  // Timer - A handle to the timer
				IntPtr::Zero  // CompletionEvent - A handle to an optional event to be signaled when the function is successful and all callback functions have completed. Can be NULL.
                );
			int error = Marshal::GetLastWin32Error();
            //CloseHandle(phNewTimer);
        }

/*
		virtual void Dispose() override
        {
            Delete();
        }
*/
    };

  
}