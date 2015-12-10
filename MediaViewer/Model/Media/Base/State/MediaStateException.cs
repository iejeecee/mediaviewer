using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.Base.State
{
    [Serializable]
    class MediaStateException : Exception
    {
        public MediaStateException(string message)
            : base(message)
        {

        }
    }
}
