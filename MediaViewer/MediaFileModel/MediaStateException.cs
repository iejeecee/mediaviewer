using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaFileModel
{
    class MediaStateException : Exception
    {
        public MediaStateException(string message)
            : base(message)
        {

        }
    }
}
