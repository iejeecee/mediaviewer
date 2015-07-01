using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoPlayerControl
{
    public class DebugVariables
    {
        public int MaxVideoPacketsInQueue { get; set; }
        public int MaxAudioPacketsInQueue { get; set; }
        public int VideoPacketsInQueue { get; set; }
        public int AudioPacketsInQueue { get; set; }
        public int NrFramesDropped { get; set; }
        public double VideoClock { get; set; }
        public double AudioClock { get; set; }
    }
}
