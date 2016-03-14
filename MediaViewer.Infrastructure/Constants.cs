using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Infrastructure
{
    public class Constants
    {
        public const int MAX_THUMBNAIL_WIDTH = 480;
        public const int MAX_THUMBNAIL_HEIGHT = 360;
        public const int THUMBNAIL_QUALITY = 70;


        public enum SaveLocation
        {
            Current,
            Ask,
            Fixed
        }
    }
}
