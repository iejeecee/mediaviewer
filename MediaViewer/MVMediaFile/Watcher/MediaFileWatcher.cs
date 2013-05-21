using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.Utils;

namespace MediaViewer.MVMediaFile.Watcher
{
    class MediaFileWatcher
    {

        private FileSystemWatcher watcher;
        private List<string> mediaFiles;
        private string currentMediaFile;

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

                if (currentMediaFile.Equals(e.FullPath))
                {

                    FileSystemEventArgs args = newFileSystemEventArgs(System.IO.WatcherChangeTypes.Changed, currentMediaFile);

                    CurrentMediaChanged(this, args);
                }

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

                if (currentMediaFile.Equals(e.FullPath))
                {

                    currentMediaFile = "";

                    FileSystemEventArgs args = newFileSystemEventArgs(System.IO.WatcherChangeTypes.Deleted, e.FullPath);

                    CurrentMediaChanged(this, args);
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

                if (currentMediaFile.Equals(e.OldFullPath))
                {

                    FileSystemEventArgs args = newFileSystemEventArgs(System.IO.WatcherChangeTypes.Renamed, e.FullPath);

                    CurrentMediaChanged(this, args);
                }

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

        public event EventHandler<FileSystemEventArgs> CurrentMediaChanged;

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

        public string CurrentMediaFile
        {

            set
            {

                this.currentMediaFile = value;
            }

            private get
            {

                return (currentMediaFile);
            }

        }

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

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public MediaFileWatcher()
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

            currentMediaFile = "";
        }

        public void setNextMediaFile()
        {

            int index = getIndexOf(currentMediaFile);

            if (index == -1 || mediaFiles.Count == 1) return;

            index = (index + 1) % mediaFiles.Count;

            currentMediaFile = mediaFiles[index];

            FileSystemEventArgs e = newFileSystemEventArgs(System.IO.WatcherChangeTypes.Changed, currentMediaFile);

            CurrentMediaChanged(this, e);

        }

        public void setPrevMediaFile()
        {

            int index = getIndexOf(currentMediaFile);

            if (index == -1 || mediaFiles.Count == 1) return;

            index = (index - 1) < 0 ? mediaFiles.Count - 1 : index - 1;

            currentMediaFile = mediaFiles[index];

            FileSystemEventArgs e = newFileSystemEventArgs(System.IO.WatcherChangeTypes.Changed, currentMediaFile);

            CurrentMediaChanged(this, e);

        }

    }
}
