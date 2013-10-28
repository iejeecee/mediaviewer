using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaFileModel.Watcher
{
    class MediaFileWatcherQueue
    {
        ConcurrentQueue<FileSystemEventArgs> eventQueue;

        public ConcurrentQueue<FileSystemEventArgs> EventQueue
        {
            get { return eventQueue; }          
        }


    }
}
