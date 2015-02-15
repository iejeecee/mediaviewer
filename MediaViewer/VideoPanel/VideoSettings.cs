using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.VideoPanel
{
    public class VideoSettings
    {
        public int VideoScreenShotTimeOffset { get; set; }
        public String VideoScreenShotLocation { get; set; }
        public ObservableCollection<String> VideoScreenShotLocationHistory { get; set; }

        public void setDefaults()
        {
            if (VideoScreenShotLocation == null)
            {
                VideoScreenShotLocation = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }    
        }
        
    }

    
}
