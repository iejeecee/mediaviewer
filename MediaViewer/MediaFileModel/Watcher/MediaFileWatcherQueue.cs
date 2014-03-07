using MediaViewer.Timers;
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
        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        class OrderedFileEvent
        {

            public OrderedFileEvent(int order, FileSystemEventArgs fileEvent)
            {
                this.order = order;
                this.fileEvent = fileEvent;
            }

            int order;

            public int Order
            {
                get { return order; }
                set { order = value; }
            }

            FileSystemEventArgs fileEvent;

            public FileSystemEventArgs FileEvent
            {
                get { return fileEvent; }
                set { fileEvent = value; }
            }

        }

        ConcurrentQueue<FileSystemEventArgs> eventQueue;

        public ConcurrentQueue<FileSystemEventArgs> EventQueue
        {
            get { return eventQueue; }
            private set { eventQueue = value; }
        }

        object processEventQueueLock;

        DefaultTimer timer;

        MediaFileWatcher mediaFileWatcher;

        public MediaFileWatcher MediaFileWatcher
        {

            get
            {
                return (mediaFileWatcher);
            }

            private set
            {

                this.mediaFileWatcher = value;
            }
        }

        public MediaFileWatcherQueue(MediaFileWatcher mediaFileWatcher)
        {
            MediaFileWatcher = mediaFileWatcher;

            created = new List<MediaFileItem>();
            removed = new List<MediaFileItem>();
            changed = new List<MediaFileItem>();
            renamedNewFiles = new List<MediaFileItem>();
            renamedOldFiles = new List<MediaFileItem>();

            eventQueue = new ConcurrentQueue<FileSystemEventArgs>();
            timer = new DefaultTimer();
            timer.Interval = 1000;
            timer.Tick += processEvents;
            timer.AutoReset = false;
            timer.start();

            processEventQueueLock = new object();

          
        }

  
        public void clear()
        {
            lock (processEventQueueLock)
            {
                FileSystemEventArgs e = null;

                while (eventQueue.TryDequeue(out e))
                {

                }
            }
            
        }

        List<MediaFileItem> created;
        List<MediaFileItem> removed;
        List<MediaFileItem> renamedOldFiles;
        List<MediaFileItem> renamedNewFiles;
        List<MediaFileItem> changed;

        void insertEvent(FileSystemEventArgs e)
        {
            if (e == null) return;

            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                    {
                        if (Utils.MediaFormatConvert.isMediaFile(e.Name))
                        {
                            changed.Add(new MediaFileItem(e.FullPath));
                        }
                        break;
                    }
                case WatcherChangeTypes.Created:
                    {
                        if (Utils.MediaFormatConvert.isMediaFile(e.Name))
                        {
                            created.Add(new MediaFileItem(e.FullPath));
                        }
                        break;
                    }
                case WatcherChangeTypes.Deleted:
                    {
                        if (Utils.MediaFormatConvert.isMediaFile(e.Name))
                        {
                            removed.Add(new MediaFileItem(e.FullPath));
                        }
                        break;
                    }
                case WatcherChangeTypes.Renamed:
                    {
                        RenamedEventArgs r = e as RenamedEventArgs;

                        if (Utils.MediaFormatConvert.isMediaFile(r.OldName) && !Utils.MediaFormatConvert.isMediaFile(r.Name))
                        {
                            MediaFileItem oldFile = new MediaFileItem(r.OldFullPath);

                            if (MediaFileWatcher.MediaState.isInUse(oldFile)) 
                            {
                                // Updating metadata on a mediafile will create a temporary copy of the mediafile
                                // which causes several create/rename/delete events
                                // ignore these events to prevent order of file locking problems in the UI

                            }
                            else
                            {
                                removed.Add(oldFile);
                            }
                            
                        }
                        else if (!Utils.MediaFormatConvert.isMediaFile(r.OldName) && Utils.MediaFormatConvert.isMediaFile(r.Name))
                        {
                            MediaFileItem newFile = new MediaFileItem(r.FullPath);

                            if (MediaFileWatcher.MediaState.isInUse(newFile))
                            {
                                // Updating metadata on a mediafile will create a temporary copy of the mediafile
                                // which causes several create/rename/delete events
                                // ignore these events to prevent order of file locking problems in the UI
                            }
                            else
                            {
                                created.Add(newFile);
                            }

                        }
                        else if (Utils.MediaFormatConvert.isMediaFile(r.OldName) && Utils.MediaFormatConvert.isMediaFile(r.Name))
                        {
                            renamedOldFiles.Add(new MediaFileItem(r.OldFullPath));
                            renamedNewFiles.Add(new MediaFileItem(r.FullPath));
                        }

                        break;
                    }
            }
        }

        void invokeEvents()
        {
            if (removed.Count > 0)
            {
                MediaFileWatcher.MediaState.remove(removed);
                removed.Clear();
            }

            if (created.Count > 0)
            {
                MediaFileWatcher.MediaState.add(created);
                created.Clear();
            }
          
            if (changed.Count > 0)
            {
                //MediaFileWatcher.MediaFiles.RemoveAll(removed);
                changed.Clear();
            }

            if (renamedOldFiles.Count > 0 || renamedNewFiles.Count > 0)
            {
                MediaFileWatcher.MediaState.rename(renamedOldFiles, renamedNewFiles);
                renamedOldFiles.Clear();
                renamedNewFiles.Clear();
            }
        }

        void processEvents(object sender, EventArgs arg)
        {
            lock (processEventQueueLock)
            {
                if (eventQueue.IsEmpty)
                {
                    timer.start();
                    return;
                }

                FileSystemEventArgs prevEvent = null;

                eventQueue.TryDequeue(out prevEvent);
                FileSystemEventArgs e = prevEvent;

                do
                {
                    switch (e.ChangeType)
                    {
                        case WatcherChangeTypes.Changed:
                            {
                                if (prevEvent.ChangeType == WatcherChangeTypes.Changed)
                                {
                                    insertEvent(e);
                                }
                                else
                                {
                                    invokeEvents();
                                    insertEvent(e);
                                }
                                break;
                            }
                        case WatcherChangeTypes.Created:
                            {
                                if (prevEvent.ChangeType == WatcherChangeTypes.Created)
                                {
                                    insertEvent(e);
                                }
                                else
                                {
                                    invokeEvents();
                                    insertEvent(e);
                                }
                                break;
                            }
                        case WatcherChangeTypes.Deleted:
                            {
                                if (prevEvent.ChangeType == WatcherChangeTypes.Deleted)
                                {
                                    insertEvent(e);
                                }
                                else
                                {
                                    invokeEvents();
                                    insertEvent(e);
                                }
                                break;
                            }
                        case WatcherChangeTypes.Renamed:
                            {

                                // this isn't correct for complex rename operations 
                                // for example chained renames like a => b and then b => c 
                                // Because in mediafilestate all the removes happen first in a batch operation
                                // after which all the new items are added.
                                // These situations will seldomly happen though, and to fix it 
                                // more complex parsing of the rename events should be done
                                if (prevEvent.ChangeType == WatcherChangeTypes.Renamed)
                                {
                                    insertEvent(e);
                                }
                                else
                                {
                                    invokeEvents();
                                    insertEvent(e);
                                }
                                break;
                            }
                    }

                    prevEvent = e;

                } while (eventQueue.TryDequeue(out e));

                invokeEvents();

                timer.start();
            }
        }

        

    }
}
