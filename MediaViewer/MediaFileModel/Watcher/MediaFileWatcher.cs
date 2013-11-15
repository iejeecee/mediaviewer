using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using MediaViewer.ImageGrid;
using System.Windows.Data;

namespace MediaViewer.MediaFileModel.Watcher
{
    class MediaFileWatcher
    {
              
        ReaderWriterLockedCollection<MediaFileItem> mediaFilesInUseByOperation;

        /// <summary>
        ///  Files that are scheduled to have some operation preformed on them
        ///  e.g. modifying their metadata, moving them etc.
        /// </summary>
        public ReaderWriterLockedCollection<MediaFileItem> MediaFilesInUseByOperation
        {
            get { return mediaFilesInUseByOperation; }
            private set { mediaFilesInUseByOperation = value; }
        }

        MediaFileState mediaFiles;
        /// <summary>
        /// All the mediafiles that are currently being watched using a mediafilewatcher
        /// Several event's can be fired on changes to the file(s)
        /// </summary>
        public MediaFileState MediaFiles
        {
            get
            {
                return (mediaFiles);
            }

            private set
            {

                this.mediaFiles = value;
            }
        }

        FileSystemWatcher watcher;
        MediaFileWatcherQueue fileWatcherQueue;

        static MediaFileWatcher instance = null;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected MediaFileWatcher()
        {

            watcher = new FileSystemWatcher();
            mediaFiles = new MediaFileState();
            mediaFilesInUseByOperation = new ReaderWriterLockedCollection<MediaFileItem>();
            
                    
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
         
        }

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

        


        private void listMediaFiles(string path)
        {

            DirectoryInfo imageDirInfo = new DirectoryInfo(path);

            FileInfo[] fileInfo = imageDirInfo.GetFiles();

            List<MediaFileItem> items = new List<MediaFileItem>();

            for (int i = 0; i < fileInfo.Length; i++)
            {
                if (MediaFormatConvert.isMediaFile(fileInfo[i].FullName))
                {
                    items.Add(new MediaFileItem(fileInfo[i].FullName));
                }
            }

            mediaFiles.Clear();
            mediaFiles.AddRange(items);
        }

        private void FileChanged(System.Object sender, System.IO.FileSystemEventArgs e)
        {
            fileWatcherQueue.EventQueue.Enqueue(e);
        }

        private void FileCreated(System.Object sender, System.IO.FileSystemEventArgs e)
        {
            fileWatcherQueue.EventQueue.Enqueue(e);
        }

        private void FileDeleted(System.Object sender, System.IO.FileSystemEventArgs e)
        {
            fileWatcherQueue.EventQueue.Enqueue(e);
        }

        private void FileRenamed(System.Object sender, System.IO.RenamedEventArgs e)
        {
            fileWatcherQueue.EventQueue.Enqueue(e);
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
