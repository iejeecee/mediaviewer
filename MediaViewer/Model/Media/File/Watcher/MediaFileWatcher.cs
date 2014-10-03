using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;
using MediaViewer.MediaGrid;
using System.Windows.Data;
using System.ComponentModel.Composition;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Utils;

namespace MediaViewer.Model.Media.File.Watcher
{
 
    public class MediaFileWatcher : IDisposable
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        MediaState mediaState;
        /// <summary>
        /// All the mediafiles that are currently being watched using a mediafilewatcher
        /// Several event's can be fired on changes to the file(s)
        /// Note that mediafilewatcher becomes unstable if the program is slow or many events are happening at once
        /// events will start missing and/or be send out of order.
        /// </summary>
        public MediaState MediaState
        {
            get
            {
                return (mediaState);
            }

            private set
            {

                this.mediaState = value;
            }
        }

        FileSystemWatcher watcher;
        MediaFileWatcherQueue fileWatcherQueue;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]  
        protected MediaFileWatcher()
        {
            watcher = new FileSystemWatcher();
            mediaState = new MediaState();                     
                    
            /* Watch for changes in LastAccess and LastWrite times, and 
            the renaming of files or directories. */
            watcher.NotifyFilter = (NotifyFilters)(NotifyFilters.LastAccess |
                NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName);

            // Only watch text files.
            watcher.Filter = "*.*";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(FileChanged);
            watcher.Created += new FileSystemEventHandler(FileCreated);
            watcher.Deleted += new FileSystemEventHandler(FileDeleted);
            watcher.Renamed += new System.IO.RenamedEventHandler(FileRenamed);

            fileWatcherQueue = new MediaFileWatcherQueue(this);

            DebugOutput = false;
         
        }

        static MediaFileWatcher instance;

        public static MediaFileWatcher Instance
        {

            get
            {
                if (instance == null)
                {
                    instance = new MediaFileWatcher();
                }

                return (instance);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool safe)
        {
            if (safe)
            {
                if (fileWatcherQueue != null)
                {
                    fileWatcherQueue.Dispose();
                }

                if (watcher != null)
                {
                    watcher.Dispose();
                }

                if (mediaState != null)
                {
                    mediaState.Dispose();
                }
            }
        }

        bool debugOutput;

        public bool DebugOutput
        {
            get { return debugOutput; }
            set { debugOutput = value; }
        }

        public bool IsWatcherEnabled
        {
            get { return watcher.EnableRaisingEvents; }
            set { watcher.EnableRaisingEvents = value; }
        }
            
        private void listMediaFiles(string path)
        {

            DirectoryInfo imageDirInfo = new DirectoryInfo(path);

            FileInfo[] fileInfo = imageDirInfo.GetFiles();

            List<MediaFileItem> items = new List<MediaFileItem>();

            for (int i = 0; i < fileInfo.Length; i++)
            {
                if (MediaFormatConvert.isMediaFile(fileInfo[i].FullName))
                {
                    items.Add(MediaFileItem.Factory.create(fileInfo[i].FullName));
                }
            }

            mediaState.clearUIState(imageDirInfo.Name, imageDirInfo.CreationTime, MediaStateType.Directory);
            mediaState.addUIState(items);
       
        }

        private void FileChanged(System.Object sender, System.IO.FileSystemEventArgs e)
        {
            if (DebugOutput)
            {
                log.Info("Changed watcher event: " + e.FullPath);
            }

            fileWatcherQueue.EventItems.Add(e);
        }

        private void FileCreated(System.Object sender, System.IO.FileSystemEventArgs e)
        {
            if (DebugOutput)
            {
                log.Info("Created watcher event: " + e.FullPath);
            }

            fileWatcherQueue.EventItems.Add(e);
        }

        private void FileDeleted(System.Object sender, System.IO.FileSystemEventArgs e)
        {
            if (DebugOutput)
            {
                log.Info("Deleted watcher event: " + e.FullPath);
            }

            fileWatcherQueue.EventItems.Add(e);
        }

        private void FileRenamed(System.Object sender, System.IO.RenamedEventArgs e)
        {
            if (DebugOutput)
            {
                log.Info("Renamed watcher event: " + e.OldFullPath + " " + e.FullPath);
            }

            fileWatcherQueue.EventItems.Add(e);
        }
         
        public string Path
        {
            set
            {
                listMediaFiles(value);

                watcher.Path = value;
                watcher.EnableRaisingEvents = true;
            }

            get
            {
                return (watcher.Path);
            }

        }

       
               

    }
}
