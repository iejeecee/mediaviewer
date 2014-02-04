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
            renamedCreatedFiles = new List<MediaFileItem>();
            renamedRemovedFiles = new List<MediaFileItem>();

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
        List<MediaFileItem> renamedRemovedFiles;
        List<MediaFileItem> renamedCreatedFiles;
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
                                renamedRemovedFiles.Add(oldFile);
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
                                renamedCreatedFiles.Add(newFile);
                            }

                        }
                        else if (Utils.MediaFormatConvert.isMediaFile(r.OldName) && Utils.MediaFormatConvert.isMediaFile(r.Name))
                        {
                            renamedRemovedFiles.Add(new MediaFileItem(r.OldFullPath));
                            renamedCreatedFiles.Add(new MediaFileItem(r.FullPath));
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

            if (renamedRemovedFiles.Count > 0 || renamedCreatedFiles.Count > 0)
            {
                MediaFileWatcher.MediaState.remove(renamedRemovedFiles);
                MediaFileWatcher.MediaState.add(renamedCreatedFiles);
                renamedRemovedFiles.Clear();
                renamedCreatedFiles.Clear();
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

        /*
                private void processEvents(object sender, EventArgs arg)
                {
           
                    lock (eventQueue)
                    {

                        if (eventQueue.Count == 0) return;

                        Dictionary<String, List<OrderedFileEvent>> groupedEvents = new Dictionary<string, List<OrderedFileEvent>>();

                        int order = 0;

                        // Group all events for the same file together                
                        foreach (FileSystemEventArgs e in eventQueue)
                        {                   
                            String key;

                            if (e is RenamedEventArgs)
                            {
                                key = (e as RenamedEventArgs).OldName;
                            }
                            else
                            {
                                key = e.Name;
                            }

                            List<OrderedFileEvent> events;

                            bool success = groupedEvents.TryGetValue(key, out events);

                            if (success == true)
                            {
                                events.Add(new OrderedFileEvent(order, e));

                                if (e is RenamedEventArgs)
                                {
                                    RenamedEventArgs temp = e as RenamedEventArgs;

                                    groupedEvents.Remove(temp.OldName);
                                    groupedEvents.Add(temp.Name, events);
                                }
                            }
                            else
                            {
                                events = new List<OrderedFileEvent>();
                                events.Add(new OrderedFileEvent(order, e));

                                groupedEvents.Add(e.Name, events);
                            }

                            order++;
                        }

                        eventQueue.Clear(); 

                        parseGroupedEvents(groupedEvents);
                    }

                }

                private void parseGroupedEvents(Dictionary<string, List<OrderedFileEvent>> groupedEvents)
                {
                    List<OrderedFileEvent> allCollapesedEvents = new List<OrderedFileEvent>();     

                    // collapse multiple successive modify or rename combinations into single events
                    foreach (KeyValuePair<string, List<OrderedFileEvent>> kv in groupedEvents)
                    {
                        List<OrderedFileEvent> events = kv.Value;
                        List<OrderedFileEvent> collapsedEvents = new List<OrderedFileEvent>();              

                        collapsedEvents.Add(events[events.Count - 1]);

                        string directory = Utils.FileUtils.getPathWithoutFileName(collapsedEvents[0].FileEvent.FullPath);
           
                        for (int i = events.Count - 2; i >= 0; i--)
                        {                    
                            switch (events[i].FileEvent.ChangeType)
                            {
                                case WatcherChangeTypes.Changed:
                                    {                             
                                        // ignore                                
                                        break;
                                    }
                                case WatcherChangeTypes.Created:
                                    {
                                        if (collapsedEvents[0].FileEvent.ChangeType == WatcherChangeTypes.Changed)
                                        {
                                            collapsedEvents[0] = events[i];
                                        }
                                        else
                                        {
                                            collapsedEvents.Insert(0, events[i]);
                                        }
                                        break;
                                    }
                                case WatcherChangeTypes.Deleted:
                                    {
                                        collapsedEvents.Insert(0, events[i]);                             
                                        break;
                                    }
                                case WatcherChangeTypes.Renamed:
                                    {
                                        if (collapsedEvents[0].FileEvent.ChangeType == WatcherChangeTypes.Renamed)
                                        {
                                            // collapse sequential rename operations into a single rename
                                            collapsedEvents[0].FileEvent = new RenamedEventArgs(WatcherChangeTypes.Renamed,
                                                directory, collapsedEvents[0].FileEvent.Name,(events[i].FileEvent as RenamedEventArgs).OldName);
                                            collapsedEvents[0].Order = events[i].Order;
                                        }
                                        else if (collapsedEvents[0].FileEvent.ChangeType == WatcherChangeTypes.Changed)
                                        {
                                            // replace changed events with rename events
                                            collapsedEvents[0] = events[i];
                                        }
                                        else
                                        {
                                            collapsedEvents.Insert(0, events[i]);      
                                        }
                                        break;
                                    }
                            }

                        }

                        allCollapesedEvents.AddRange(collapsedEvents);
                                                         
                    }

                    // invoke the collapsed events in the correct order
                    allCollapesedEvents.Sort((a, b) => a.Order.CompareTo(b.Order)); 

                    foreach (OrderedFileEvent e in allCollapesedEvents)
                    {
                        invokeEvent(e);
                    }

                    log.Debug("----END OF PARSE-----"); 
                }

                void invokeEvent(OrderedFileEvent e)
                {
                    switch (e.FileEvent.ChangeType)
                    {
                        case WatcherChangeTypes.Changed:
                            {
                                log.Debug(String.Format("Changed: {0} ({1})", e.FileEvent.Name, e.Order)); 
                                MediaChanged(this, e.FileEvent);
                                break;
                            }
                        case WatcherChangeTypes.Created:
                            {
                                log.Debug(String.Format("Created: {0} ({1})", e.FileEvent.Name, e.Order)); 
                                MediaCreated(this, e.FileEvent);
                                break;
                            }
                        case WatcherChangeTypes.Deleted:
                            {
                                log.Debug(String.Format("Deleted: {0} ({1})", e.FileEvent.Name, e.Order)); 
                                MediaDeleted(this, e.FileEvent);
                                break;
                            }
                        case WatcherChangeTypes.Renamed:
                            {
                                log.Debug(String.Format("Renamed: {0} to {1} ({2})", (e.FileEvent as RenamedEventArgs).OldName, e.FileEvent.Name, e.Order));
                                MediaRenamed(this, e.FileEvent as RenamedEventArgs);
                                break;
                            }
                    }

                }
         */

    }
}
