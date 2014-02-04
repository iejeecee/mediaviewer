using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaFileModel.Watcher
{
    public enum MediaFileItemState
    {
        EMPTY,
        LOADING,
        LOADED,
        CANCELLED,
        DELETED,
        ERROR
    }
}
