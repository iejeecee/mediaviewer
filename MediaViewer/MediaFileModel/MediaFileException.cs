using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaFileModel
{
    class MediaFileException : Exception
    {
        public MediaFileException(string message)
            : base(message)
        {

        }
    }
}
