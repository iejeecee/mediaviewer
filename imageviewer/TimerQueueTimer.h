#pragma once
#include "HRTimer.h"

namespace imageviewer
{
	public ref class TimerQueueTimerException : Exception
    {
	public:
        TimerQueueTimerException(String ^message) : Exception(message)
        {
        }

        TimerQueueTimerException(String ^message, Exception ^innerException) : Exception(message, innerException)
        {
        }
    };

	public ref class TimerQueueTimer : public HRTimer
    {
		 

	private:

		delegate void WaitOrTimerDelegate(IntPtr lpParameter, bool timerOrWaitFired); 

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

        [DllImport("kernel32.dll")]
        static bool CreateTimerQueueTimer(
            IntPtr %timerHandle,          // timerHandle - Pointer to a handle; this is an out value
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

		IntPtr timerHandle; 
		bool running;
		bool autoReset;
		int interval;
		WaitOrTimerDelegate ^callback;
		ISynchronizeInvoke ^synchronizingObject;

		void timerOrWaitFired(IntPtr lpParameter, bool timerOrWaitFired) {

			if(autoReset == false) {

				running = false;
			}

			Tick(this, EventArgs::Empty);
		}

	public:

        TimerQueueTimer()
        {
			running = false;
			timerHandle = IntPtr::Zero;
			autoReset = true;
			synchronizingObject = nullptr;

			callback = gcnew WaitOrTimerDelegate(this, &TimerQueueTimer::timerOrWaitFired);
        }

		~TimerQueueTimer() 
		{
			if(running == true) {
				stop();
			}
		}

        virtual void start() override
        {

			if(running == true) return;

			IntPtr pParameter = IntPtr::Zero;

			unsigned int executeThread = synchronizingObject == nullptr ? 
				(unsigned int)Flag::WT_EXECUTEDEFAULT :
				(unsigned int)Flag::WT_EXECUTEINTIMERTHREAD;

			int period = interval;
			
			if(autoReset == false) {

				period = 0;
				executeThread |= (unsigned int)Flag::WT_EXECUTEONLYONCE;
			} 
			

            bool success = CreateTimerQueueTimer(
                // Timer handle
                timerHandle,
                // Default timer queue. IntPtr.Zero is just a constant value that represents a null pointer.
				IntPtr::Zero,
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
                
				throw gcnew TimerQueueTimerException("Error creating QueueTimer");
			} else {

				running = true;
			}
        }

        virtual void stop() override
        {
			if(running == false) return;

            //bool success = DeleteTimerQueue(IntPtr.Zero);
            bool success = DeleteTimerQueueTimer(
				IntPtr::Zero, // TimerQueue - A handle to the (default) timer queue
                timerHandle,  // Timer - A handle to the timer
				IntPtr::Zero  // CompletionEvent - A handle to an optional event to be signaled when the function is successful and all callback functions have completed. Can be NULL.
                );

			//if(!success) {
                
			//	throw gcnew TimerQueueTimerException("Error deleting QueueTimer");
				
			//} else {

				running = false;
			//}
			
           // CloseHandle(timerHandle);
			int error = Marshal::GetLastWin32Error();
        }

		property ISynchronizeInvoke ^SynchronizingObject
		{
			virtual ISynchronizeInvoke ^get() override
			{
			
				return(synchronizingObject);
			}

			virtual void set(ISynchronizeInvoke ^synchronizingObject) override
			{
				this->synchronizingObject = synchronizingObject;
			}
		}

		property bool AutoReset
		{
			virtual bool get() override
			{
				
				return autoReset;
			}

			virtual void set(bool autoReset) override
			{				
				this->autoReset = autoReset;

				if(IsRunning)
				{
					stop();
					start();
				}
			}
		}	

		property int Interval
		{
			virtual int get() override
			{

				return interval;
			}

			virtual void set(int interval) override
			{			
				this->interval = interval;

				if(IsRunning) {

					stop();
					start();
				}
			}
		}

		property bool IsRunning
		{
			bool get() {

				return running;
			}
		}


/*
		virtual void Dispose() override
        {
            Delete();
        }
*/
    };

  
}