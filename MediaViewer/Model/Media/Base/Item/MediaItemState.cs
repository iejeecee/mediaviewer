using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.Base.Item
{
    public enum MediaItemState
    {
        DUMMY,
        EMPTY,
        RELOAD,
        LOADING,
        LOADED,
        CANCELLED,
        TIMED_OUT,
        FILE_NOT_FOUND,
        DELETED,
        ERROR
    }
}
