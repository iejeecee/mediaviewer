using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MediaViewer.Timers
{
    public class TimerStartException : ApplicationException
    {

        //Initializes a new instance of the TimerStartException class.

        public TimerStartException(String message)
            : base(message)
        {
        }
    }

    //Defines constants for the multimedia Timer's event types.
    enum TimerMode
    {
        //Timer event occurs once.
        ONE_SHOT,
        //Timer event occurs periodically.
        PERIODIC
    }


    //Represents information about the multimedia Timer's capabilities.
    [StructLayout(LayoutKind.Sequential)]
    public struct TimerCaps
    {

        //Minimum supported interval in milliseconds.
        public int intervalMin;

        //Maximum supported interval in milliseconds.
        public int intervalMax;
    }


    //Represents the Windows multimedia timer.
    public class MultiMediaTimer : HRTimer
    {

        //Occurs when the Timer has started;
        public event EventHandler Started;

        //Occurs when the Timer has stopped;
        public event EventHandler Stopped;

        public event EventHandler Disposed;

        // Represents the method that is called by Windows when a timer event occurs.
        delegate void TimeProc(int id, int msg, int user, int param1, int param2);

        // Represents methods that raise events.
        delegate void EventRaiser(EventArgs e);

        // Gets timer capabilities.
        [DllImport("winmm.dll")]
        static extern int timeGetDevCaps(ref TimerCaps caps,
            int sizeOfTimerCaps);

        // Creates and starts the timer.
        [DllImport("winmm.dll")]
        static extern int timeSetEvent(int delay, int resolution,
            TimeProc proc, int user, int mode);

        // Stops and destroys the timer.
        [DllImport("winmm.dll")]
        static extern int timeKillEvent(int id);

        // Indicates that the operation was successful.
        const int TIMERR_NOERROR = 0;

        // Timer identifier.
        int timerID;

        // Timer mode.
        volatile TimerMode mode;

        // Interval between timer events in milliseconds.
        volatile int interval;

        // Timer resolution in milliseconds.
        volatile int resolution;

        // Called by Windows when a timer periodic event occurs.
        TimeProc timeProcPeriodic;

        // Called by Windows when a timer one shot event occurs.
        TimeProc timeProcOneShot;

        // Represents the method that raises the Tick event.
        EventRaiser tickRaiser;

        // Indicates whether or not the timer is running.
        bool running;

        // Indicates whether or not the timer has been disposed.
        volatile bool disposed;

        // The ISynchronizeInvoke object to use for marshaling events.
        ISynchronizeInvoke synchronizingObject;

        // For implementing IComponent.
        ISite site;

        // Multimedia timer capabilities.
        static TimerCaps caps;

        // Initialize timer with default values.
        void Initialize()
        {
            this.mode = TimerMode.PERIODIC;
            this.interval = Capabilities.intervalMin;
            this.resolution = 1;

            running = false;
            disposed = false;
            synchronizingObject = null;
            site = null;

            timeProcPeriodic = new TimeProc(TimerPeriodicEventCallback);
            timeProcOneShot = new TimeProc(TimerOneShotEventCallback);
            tickRaiser = new EventRaiser(OnTick);
        }

        // Callback method called by the Win32 multimedia timer when a timer
        // periodic event occurs.
        void TimerPeriodicEventCallback(int id, int msg, int user, int param1, int param2)
        {
            if (synchronizingObject != null)
            {
                synchronizingObject.BeginInvoke(tickRaiser, new Object[] { EventArgs.Empty });
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
            if (synchronizingObject != null)
            {
                synchronizingObject.BeginInvoke(tickRaiser, new Object[] { EventArgs.Empty });
                stop();
            }
            else
            {
                OnTick(EventArgs.Empty);
                stop();
            }
        }

        // Raises the Disposed event.
        void OnDisposed(EventArgs e)
        {
            if (Disposed != null)
            {
                Disposed(this, e);
            }
        }

        // Raises the Started event.
        void OnStarted(EventArgs e)
        {
            if (Started != null)
            {
                Started(this, e);
            }
        }

        // Raises the Stopped event.
        void OnStopped(EventArgs e)
        {
            if (Stopped != null)
            {
                Stopped(this, e);
            }
        }


        //Initialize class.
        static MultiMediaTimer()
        {
            caps = new TimerCaps();
            // Get multimedia timer capabilities.
            timeGetDevCaps(ref caps, Marshal.SizeOf(caps));
        }

        //Initializes a new instance of the Timer class.
        public MultiMediaTimer()
        {
            Initialize();
        }

        ~MultiMediaTimer()
        {
            if (IsRunning)
            {
                // Stop and destroy timer.
                timeKillEvent(timerID);
            }
        }

        //Frees timer resources.
        public void Dispose()
        {
            if(disposed) {
                return;
            }

            if(IsRunning) {

                stop();
            }

            disposed = true;
            OnDisposed(EventArgs.Empty);
        }
        
        //Starts the timer.
        //<exception c="ObjectDisposedException">
        //The timer has already been disposed.
        //</exception>
        //<exception c="TimerStartException">
        //The timer failed to start.
        //</exception>
        public override void start()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Timer");
            }

            if (IsRunning)
            {
                return;
            }

            // If the periodic event callback should be used.
            if (mode == TimerMode.PERIODIC)
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
            if (timerID != 0)
            {
                running = true;

                if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
                {
                    SynchronizingObject.BeginInvoke(new EventRaiser(OnStarted), new Object[] { EventArgs.Empty });
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
        //<exception c="ObjectDisposedException">
        //If the timer has already been disposed.
        //</exception>
        public override void stop()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Timer");
            }

            if (!running)
            {
                return;
            }

            // Stop and destroy timer.
            int result = timeKillEvent(timerID);

            Debug.Assert(result == TIMERR_NOERROR);
            running = false;

            if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
            {
                SynchronizingObject.BeginInvoke(
                    new EventRaiser(OnStopped),
                    new Object[] { EventArgs.Empty });
            }
            else
            {
                OnStopped(EventArgs.Empty);
            }
        }


        //Gets or sets the object used to marshal event-handler calls.
        public override ISynchronizeInvoke SynchronizingObject
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Timer");
                }

                return synchronizingObject;
            }

            set
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Timer");
                }

                synchronizingObject = value;
            }
        }

        //Gets or sets the time between Tick events.
        //<exception c="ObjectDisposedException">
        //If the timer has already been disposed.
        //</exception>   
        public override int Interval
        {
            get
            {

                if (disposed)
                {
                    throw new ObjectDisposedException("Timer");
                }

                return interval;
            }

            set
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Timer");

                }
                else if (value < Capabilities.intervalMin || value > Capabilities.intervalMax)
                {

                    throw new ArgumentOutOfRangeException("Interval", value,
                        "Multimedia Timer interval out of range.");
                }

                interval = value;

                if (IsRunning)
                {

                    stop();
                    start();
                }
            }
        }

        //Gets or sets the timer resolution.
        //<exception c="ObjectDisposedException">
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
                if (disposed)
                {
                    throw new ObjectDisposedException("Timer");
                }

                return resolution;
            }

            set
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Timer");
                }
                else if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Resolution", value,
                        "Multimedia timer resolution out of range.");
                }

                resolution = value;

                if (IsRunning)
                {
                    stop();
                    start();
                }
            }
        }

        public override bool AutoReset
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Timer");
                }

                return mode == TimerMode.PERIODIC ? true : false;
            }

            set
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Timer");
                }

                mode = (value == true) ? TimerMode.PERIODIC : TimerMode.ONE_SHOT;

                if (IsRunning)
                {
                    stop();
                    start();
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

    }
}
