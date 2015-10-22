using MediaViewer.Infrastructure.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.File.Watcher
{
    class MediaFileWatcherQueue : IDisposable
    {
        
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

        BlockingCollection<FileSystemEventArgs> eventItems;

        public BlockingCollection<FileSystemEventArgs> EventItems
        {
            get { return eventItems; }
            private set { eventItems = value; }
        }
 
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
            renamedNewLocations = new List<String>();
            renamedOldFiles = new List<MediaFileItem>();

            eventItems = new BlockingCollection<FileSystemEventArgs>(new ConcurrentQueue<FileSystemEventArgs>());
            Task.Run(() => processEvents());         

        }

        public void Dispose()
        {
            if (eventItems != null)
            {
                eventItems.Dispose();
                eventItems = null;
            }
        }

/*
        public void clear()
        {
            lock (processEventQueueLock)
            {
                FileSystemEventArgs e = null;

                while (eventItems.TryDequeue(out e))
                {

                }
            }

        }
*/
        List<MediaFileItem> created;
        List<MediaFileItem> removed;
        List<MediaFileItem> renamedOldFiles;
        List<String> renamedNewLocations;
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
                            MediaFileItem item = MediaFileItem.Factory.findInDictionary(e.FullPath);

                            if (item != null)
                            {
                                changed.Add(item);
                            }
                        }
                        break;
                    }
                case WatcherChangeTypes.Created:
                    {
                        if (Utils.MediaFormatConvert.isMediaFile(e.Name))
                        {
                            created.Add(MediaFileItem.Factory.create(e.FullPath));
                        }
                        break;
                    }
                case WatcherChangeTypes.Deleted:
                    {
                        if (Utils.MediaFormatConvert.isMediaFile(e.Name))
                        {
                            MediaFileItem item = MediaFileItem.Factory.findInDictionary(e.FullPath);

                            if (item != null)
                            {
                                removed.Add(item);
                            }
                        }
                        break;
                    }
                case WatcherChangeTypes.Renamed:
                    {
                        RenamedEventArgs r = e as RenamedEventArgs;

                        if (Utils.MediaFormatConvert.isMediaFile(r.OldName) && !Utils.MediaFormatConvert.isMediaFile(r.Name))
                        {
                            MediaFileItem oldItem = MediaFileItem.Factory.findInDictionary(r.OldFullPath);
                        
                            if (Path.GetExtension(r.Name).Equals("._01_"))
                            {
                                // Updating metadata on a mediafile will create a temporary copy of the mediafile
                                // which causes several create/rename/delete events
                                // ignore these events to prevent order of file locking problems in the UI

                            }
                            else
                            {
                                if (oldItem != null)
                                {
                                    removed.Add(oldItem);
                                }
                            }

                        }
                        else if (!Utils.MediaFormatConvert.isMediaFile(r.OldName) && Utils.MediaFormatConvert.isMediaFile(r.Name))
                        {
                            MediaFileItem newFile = MediaFileItem.Factory.create(r.FullPath);

                            if (Path.GetExtension(r.OldName).Equals("._00_"))
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
                            MediaFileItem renamedItem = MediaFileItem.Factory.findInDictionary(r.OldFullPath);

                            if(renamedItem != null) {

                                renamedOldFiles.Add(renamedItem);
                                renamedNewLocations.Add(r.FullPath);
                            }
                        }

                        break;
                    }
            }
        }

        void invokeEvents()
        {
            if (removed.Count > 0)
            {
                MediaFileWatcher.MediaFileState.removeUIState(removed);
                removed.Clear();
            }

            if (created.Count > 0)
            {
                MediaFileWatcher.MediaFileState.addUIState(created);
                created.Clear();
            }

            if (changed.Count > 0)
            {
                MediaFileWatcher.MediaFileState.changedUIState(changed);
                changed.Clear();
            }

            if (renamedOldFiles.Count > 0 || renamedNewLocations.Count > 0)
            {
                MediaFileWatcher.MediaFileState.renameUIState(renamedOldFiles, renamedNewLocations);
                renamedOldFiles.Clear();
                renamedNewLocations.Clear();
            }
        }

        void processEvents()
        {
            while (true)
            {
                try
                {
                    FileSystemEventArgs prevEvent = eventItems.Take();

                    //allow the queue to fill before consuming it's items
                    Thread.Sleep(100);

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

                    } while (eventItems.TryTake(out e));


                    invokeEvents();
                }
                catch (Exception e)
                {
                    Logger.Log.Error("Exception in MediaFileWatcherQueue", e);
                }

            }

        }

    }
}
