using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MediaViewer.Utils.Windows
{
    class DriveIdleMonitor
    {
        DispatcherTimer[] timers;
        PerformanceCounter[] diskIdleTime;
        public event EventHandler<String> DriveInUse;
        List<string> drivesMonitored;

        public List<string> DrivesMonitored
        {
            get { return drivesMonitored; }           
        }

        public DriveIdleMonitor()
        {
            drivesMonitored = new List<string>();

            PerformanceCounterCategory cat = new System.Diagnostics.PerformanceCounterCategory("PhysicalDisk");         
            string[] instNames = cat.GetInstanceNames();           

            if (instNames.Length - 1 > 0)
            {
                timers = new DispatcherTimer[instNames.Length - 1];
                diskIdleTime = new PerformanceCounter[instNames.Length - 1];
            }

            int i = 0;

            foreach (string inst in instNames)
            {
                if (inst.Equals("_Total")) continue;

                timers[i] = new DispatcherTimer();
                diskIdleTime[i] = new PerformanceCounter("PhysicalDisk",
                     "% Idle Time", inst);
                diskIdleTime[i].NextValue();

                timers[i].Interval = new TimeSpan(0, 0, 3);
                timers[i].Tick += new EventHandler(timer_Tick);
                timers[i].Tag = inst;
                timers[i].Start();

                char[] delimiterChars = {' '};
           
                string[] drives = inst.Split(delimiterChars);

                foreach(String drive in drives) {
                    
                    int val;
                    bool isInt = int.TryParse(drive, out val);

                    if (!isInt)
                    {
                        drivesMonitored.Add(drive);
                    }
                }

                i++;
            }        
            
        }

        ~DriveIdleMonitor()
        {
            foreach (PerformanceCounter c in diskIdleTime)
            {
                c.Dispose();
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            String instanceName = "";

            int i = 0;

            foreach (DispatcherTimer t in timers)
            {
                if (t.Equals(sender))
                {
                    instanceName = (string)t.Tag;
                    break;
                }

                i++;
            }

            float value = diskIdleTime[i].NextValue();

            if (Math.Floor(value) < 99)
            {
                if (DriveInUse != null)
                {
                    DriveInUse(this, instanceName);
                }
            }

        }
    }
}
