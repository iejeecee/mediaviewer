using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
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
    public enum MediaType
    {
        All,
        Video,
        Images
    }

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
            
            RecurseSubDirectories = false;
            Query = "Search";
            SearchType = MediaType.All;

            searchCommand = new Command<String>(new Action<String>((query) =>
            {
                doSearch(query);
            }));
        }

        MediaType searchType;

        public MediaType SearchType
        {
            get { return searchType; }
            set { searchType = value;
            NotifyPropertyChanged();
            }
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

        

        private void doSearch(String query)
        {
            if (String.IsNullOrEmpty(query) || String.IsNullOrWhiteSpace(query)) return;
       
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            List<Tag> tags;

            parseQuery(Query, out tags);

            //List<MediaFileItem> results = diskTagSearch(tags, RecurseSubDirectories, tokenSource.Token);
            List<MediaFileItem> results = dbTagSearch(tags);

            foreach (MediaFileItem item in results)
            {
                item.ItemState = MediaFileItemState.EMPTY;
            }

            MediaFileWatcher.Instance.MediaFiles.Clear();
            MediaFileWatcher.Instance.MediaFiles.AddRange(results);
        }

        public List<MediaFileItem> dbTagSearch(List<Tag> tags)
        {
            MediaDbCommands mediaCommands = new MediaDbCommands();

            List<Media> results = mediaCommands.findMediaByTags(tags);
            List<MediaFileItem> items = new List<MediaFileItem>();

            foreach (Media result in results)
            {
                items.Add(new MediaFileItem(result.Location));
            }

            return (items);
        }


        public List<MediaFileItem> diskTagSearch(List<Tag> tags, bool recurseSubDirectories, CancellationToken token)
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
                    foreach (Tag tag in tags)
                    {
                        if (item.Media.MetaData == null)
                        {
                            continue;
                        }

                        if (item.Media.MetaData.Tags.Contains(tag.Name, new IgnoreCaseComparer()))
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

            if (SearchType == MediaType.All && MediaFormatConvert.isMediaFile(info.Name))
            {
                mediaItems.Add(new MediaFileItem(info.FullName));
            }
            else if (SearchType == MediaType.Video && MediaFormatConvert.isVideoFile(info.Name))
            {
                mediaItems.Add(new MediaFileItem(info.FullName));
            }
            else if (SearchType == MediaType.Images && MediaFormatConvert.isImageFile(info.Name))
            {
                mediaItems.Add(new MediaFileItem(info.FullName));
            }
        }

        void parseQuery(String query, out List<Tag> tags)
        {
            TagDbCommands tagCommands = new TagDbCommands();

            tags = new List<Tag>();

            foreach (String tagName in query.Split(','))
            {
                String tagNameTrimmed = tagName.Trim();

                Tag dbTag = tagCommands.getTagByName(tagNameTrimmed);

                if (dbTag == null)
                {
                    tags.Add(new Tag() { Name = tagNameTrimmed });
                }
                else
                {
                    tags.Add(dbTag);
                }
            }
        }

    }
}
