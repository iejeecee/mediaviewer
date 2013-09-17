using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.Utils;
using System.Collections.ObjectModel;

namespace MediaViewer.MediaFileModel.Watcher
{
    class MediaFileWatcher
    {

        private FileSystemWatcher watcher;

        private ObservableCollection<string> mediaFiles;

        public ObservableCollection<string> MediaFiles
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


        private void findMediaFiles(string path)
        {

			DirectoryInfo imageDirInfo = new DirectoryInfo(path);

			mediaFiles.Clear();

			FileInfo[] fileInfo = imageDirInfo.GetFiles();

			for(int i = 0; i < fileInfo.Length; i++) {

				if(MediaFormatConvert.isMediaFile(fileInfo[i].FullName)) 
				{
					mediaFiles.Add(fileInfo[i].FullName);
				}				

			}
		}

        private void FileChanged(System.Object sender, System.IO.FileSystemEventArgs e)
        {

            if (MediaFormatConvert.isMediaFile(e.FullPath))
            {
            
                MediaChanged(this, e);
            }

        }

        private void FileCreated(System.Object sender, System.IO.FileSystemEventArgs e)
        {

            if (MediaFormatConvert.isMediaFile(e.FullPath))
            {

                mediaFiles.Add(e.FullPath);
                MediaCreated(this, e);

            }

        }

        private void FileDeleted(System.Object sender, System.IO.FileSystemEventArgs e)
        {

            if (MediaFormatConvert.isMediaFile(e.FullPath))
            {

                int removeIndex = getMediaFileIndex(e.FullPath,MediaType.ANY);

                if (removeIndex >= 0)
                {

                    mediaFiles.RemoveAt(removeIndex);
                }
               
                MediaDeleted(this, e);
            }
        }

        private void FileRenamed(System.Object sender, System.IO.RenamedEventArgs e)
        {

            if (MediaFormatConvert.isMediaFile(e.FullPath))
            {

                int index = getMediaFileIndex(e.OldFullPath, MediaType.ANY);

                if (index >= 0)
                {

                    mediaFiles[index] = e.FullPath;
                }              
            
                MediaRenamed(this, e);
            }

        }

      

        private FileSystemEventArgs newFileSystemEventArgs(System.IO.WatcherChangeTypes changeType, string location)
        {

            string directory = System.IO.Path.GetDirectoryName(location);
            string name = System.IO.Path.GetFileName(location);

            FileSystemEventArgs e = new FileSystemEventArgs(changeType, directory, name);

            return (e);
        }



        public event System.IO.FileSystemEventHandler MediaChanged;
        public event System.IO.FileSystemEventHandler MediaCreated;
        public event System.IO.FileSystemEventHandler MediaDeleted;
        public event System.IO.RenamedEventHandler MediaRenamed;

        public string Path
        {

            set
            {

                findMediaFiles(value);

                watcher.Path = value;
                watcher.EnableRaisingEvents = true;

            }

            get
            {

                return (watcher.Path);
            }

        }

       

        static MediaFileWatcher instance = null;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected MediaFileWatcher()
        {

            watcher = new FileSystemWatcher();
            mediaFiles = new ObservableCollection<string>();

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

        }

        public static MediaFileWatcher Instance {

            get
            {
                if (instance == null)
                {
                    instance = new MediaFileWatcher();
                }

                return (instance);
            }
        }
        public enum MediaType
        {
            ANY,
            IMAGE,
            VIDEO
        }

        public enum Direction
        {
            NEXT = 1,
            PREVIOUS = -1
        }

        public int getMediaFileIndex(String mediaFileName, MediaType type)
        {
            if (String.IsNullOrEmpty(mediaFileName)) return (-1);

            int i = 0;
            int j = 0;
          
            foreach (string name in MediaFiles)
            {
                
                if (type == MediaType.VIDEO && MediaFormatConvert.isVideoFile(name))
                {
                    if (mediaFiles[j].Equals(mediaFileName))
                    {
                        return (i);
                    }

                    i++;

                }
                else if (type == MediaType.IMAGE && MediaFormatConvert.isImageFile(name))
                {
                    if (mediaFiles[j].Equals(mediaFileName))
                    {
                        return (i);
                    }

                    i++;
                }
                else if(type == MediaType.ANY)
                {
                    if (mediaFiles[j].Equals(mediaFileName))
                    {
                        return (i);
                    }

                    i++;
                }

                j++;
            }
          
            return (-1);
        }

        public string getMediaFileByIndex(MediaType type, int index)
        {

            int i = 0;
            int j = 0;
          
            foreach (string name in MediaFiles)
            {
               
                if (type == MediaType.VIDEO && MediaFormatConvert.isVideoFile(name))
                {
                    if (i == index)
                    {
                        return (MediaFiles[j]);
                    }

                    i++;
                }
                else if (type == MediaType.IMAGE && MediaFormatConvert.isImageFile(name))
                {
                    if (i == index)
                    {
                        return (MediaFiles[j]);
                    }

                    i++;
                }
                else if (type == MediaType.ANY)
                {
                    if (i == index)
                    {
                        return (MediaFiles[j]);
                    }

                    i++;
                }

                j++;
            }
           
            return (null);
        }

        public int getNrMediaFiles(MediaType type) {

            int count = 0;

            foreach (string name in MediaFiles)
            {
                if (type == MediaType.VIDEO && MediaFormatConvert.isVideoFile(name))
                {
                    count++;

                }
                else if (type == MediaType.IMAGE && MediaFormatConvert.isImageFile(name))
                {

                    count++;
                }
                else if (type == MediaType.ANY)
                {
                    count++;
                }
                
            }

            return (count);
        }

    }
}
