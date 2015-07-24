using MediaViewer.Model.Media.File.Watcher;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaFileBrowser.DirectoryBrowser
{
    class MediaFileBrowserDirectoryBrowserViewModel : BindableBase
    {
        public MediaFileBrowserDirectoryBrowserViewModel(MediaFileWatcher mediaFileWatcher)
        {

            MediaFileWatcher = mediaFileWatcher;
            
        }

        public MediaFileWatcher MediaFileWatcher { get; set; }
    }
}
