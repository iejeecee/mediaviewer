using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoPlayerControl
{
    [Serializable]
    class VideoPlayerException : Exception
    {
        public VideoPlayerException(String message)
            : base(message)
        {

        }

        public VideoPlayerException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
