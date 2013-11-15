using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Utils;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.Search
{
    class SearchViewModel : ObservableObject
    {
        class IgnoreCaseComparer : IEqualityComparer<String>
        {
            public bool Equals(string x, string y)
            {
                return (x.ToLower().Equals(y.ToLower()));
            }

            public int GetHashCode(string obj)
            {
                return (obj.GetHashCode());
            }
        }

        public SearchViewModel()
        {
            AllMedia = true;
            RecurseSubDirectories = false;
            Query = "Search";

            searchCommand = new Command<String>(new Action<String>((query) =>
            {
                doSearch(query);
            }));
        }

        Command<String> searchCommand;

        public Command<String> SearchCommand
        {
            get { return searchCommand; }
            set
            {
                searchCommand = value;
                NotifyPropertyChanged();
            }
        }

        String query;

        public String Query
        {
            get { return query; }
            set
            {
                query = value;
                NotifyPropertyChanged();
            }
        }

        bool recurseSubDirectories;

        public bool RecurseSubDirectories
        {
            get { return recurseSubDirectories; }
            set
            {
                recurseSubDirectories = value;
                NotifyPropertyChanged();
            }
        }

        bool allMedia;

        public bool AllMedia
        {
            get { return allMedia; }
            set
            {
                if (value == false && VideoOnly == false && ImagesOnly == false)
                {
                    NotifyPropertyChanged();
                    return;
                }

                allMedia = value;
                if (value == true)
                {
                    VideoOnly = false;
                    ImagesOnly = false;
                }
                NotifyPropertyChanged();
            }
        }
        bool videoOnly;

        public bool VideoOnly
        {
            get { return videoOnly; }
            set
            {
                if (value == false && AllMedia == false && ImagesOnly == false)
                {
                    NotifyPropertyChanged();
                    return;
                }

                videoOnly = value;
                if (value == true)
                {
                    AllMedia = false;
                    ImagesOnly = false;
                }
                NotifyPropertyChanged();
            }
        }
        bool imagesOnly;

        public bool ImagesOnly
        {
            get { return imagesOnly; }
            set
            {
                if (value == false && AllMedia == false && VideoOnly == false)
                {
                    NotifyPropertyChanged();
                    return;
                }

                imagesOnly = value;
                if (value == true)
                {
                    VideoOnly = false;
                    AllMedia = false;
                }
                NotifyPropertyChanged();
            }
        }

        private void doSearch(String query)
        {
            if (String.IsNullOrEmpty(query) || String.IsNullOrWhiteSpace(query)) return;

            List<String> tags = new List<string>();
            tags.Add(query);

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            List<MediaFileItem> results = tagSearch(tags, RecurseSubDirectories, tokenSource.Token);

            foreach (MediaFileItem item in results)
            {
                item.ItemState = MediaFileItemState.EMPTY;
            }

            MediaFileWatcher.Instance.MediaFiles.Clear();
            MediaFileWatcher.Instance.MediaFiles.AddRange(results);
        }


        public List<MediaFileItem> tagSearch(List<String> tags, bool recurseSubDirectories, CancellationToken token)
        {

            String searchPath = MediaFileWatcher.Instance.Path;

            List<MediaFileItem> mediaItems = new List<MediaFileItem>();
            List<MediaFileItem> matches = new List<MediaFileItem>();

            FileUtils.WalkDirectoryTreeDelegate callback = new FileUtils.WalkDirectoryTreeDelegate(fileWalkerCallback);

            FileUtils.walkDirectoryTree(new System.IO.DirectoryInfo(searchPath), callback, mediaItems, recurseSubDirectories);

            foreach (MediaFileItem item in mediaItems)
            {
                item.loadMetaData(MediaFileModel.MediaFile.MetaDataLoadOptions.LOAD_FROM_DISK, token);
                if (item.ItemState == MediaFileItemState.LOADED)
                {
                    foreach (String tag in tags)
                    {
                        if (item.Media.MetaData == null)
                        {
                            continue;
                        }

                        if (item.Media.MetaData.Tags.Contains(tag, new IgnoreCaseComparer()))
                        {
                            matches.Add(item);
                            continue;
                        }
                    }
                }
            }


            return (matches);
        }

        void fileWalkerCallback(FileInfo info, Object state)
        {
            List<MediaFileItem> mediaItems = (List<MediaFileItem>)state;

            if (AllMedia == true && MediaFormatConvert.isMediaFile(info.Name))
            {
                mediaItems.Add(new MediaFileItem(info.FullName));
            }
            else if (VideoOnly == true && MediaFormatConvert.isVideoFile(info.Name))
            {
                mediaItems.Add(new MediaFileItem(info.FullName));
            }
            else if (ImagesOnly == true && MediaFormatConvert.isImageFile(info.Name))
            {
                mediaItems.Add(new MediaFileItem(info.FullName));
            }
        }

    }
}
