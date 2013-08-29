using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.Utils;

namespace MediaViewer.MediaFileModel.Watcher
{
    class MediaFileWatcher
    {

        private FileSystemWatcher watcher;

        private List<string> mediaFiles;
   
        public List<string> MediaFiles
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

                int removeIndex = getIndexOf(e.FullPath);

                if (removeIndex != -1)
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

                int removeIndex = getIndexOf(e.OldFullPath);

                if (removeIndex != -1)
                {

                    mediaFiles.RemoveAt(removeIndex);
                }

                mediaFiles.Add(e.FullPath);
            
                MediaRenamed(this, e);
            }

        }

        private int getIndexOf(string imageFile)
        {

            for (int i = 0; i < mediaFiles.Count; i++)
            {

                if (mediaFiles[i].Equals(imageFile))
                {

                    return (i);
                }
            }

            return (-1);
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
            mediaFiles = new List<string>();

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

        private string getNewMediaFile(string currentMediaFile, int step)
        {
            int index = getIndexOf(currentMediaFile);

            if (index == -1)
            {
                if (mediaFiles.Count == 0)
                {
                    return ("");
                }

                index = 0;
            }

            index = (index + step) < 0 ? mediaFiles.Count + step : (index + step) % mediaFiles.Count;

            return(mediaFiles[index]);
        }

        public enum MediaType
        {
            IMAGE,
            VIDEO
        }

        public enum Direction
        {
            NEXT = 1,
            PREVIOUS = -1
        }

        public string getNewMediaFile(String currentMedia, MediaType newType, Direction direction)
        {
           
            string newMedia = currentMedia;

            int i = 0;

            do
            {
                newMedia = getNewMediaFile(newMedia, (int)direction);

                if(newType == MediaType.IMAGE && MediaFormatConvert.isImageFile(newMedia)) {

                    break;

                }
                else if (newType == MediaType.VIDEO && MediaFormatConvert.isVideoFile(newMedia))
                {
                    break;
                }
                else if (i >= mediaFiles.Count)
                {
                    newMedia = "";
                    break;
                }

                i++;

            } while (!newMedia.Equals(currentMedia));
           

            return (newMedia);
        }

       

    }
}
