using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.MediaFileModel;
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
               
        class IgnoreCaseComparer : IEqualityComparer<Tag>
        {
            public bool Equals(Tag x, Tag y)
            {
                return (x.Name.ToLower().Equals(y.Name.ToLower()));
            }

            public int GetHashCode(Tag obj)
            {
                return (obj.GetHashCode());
            }
        }

        public SearchViewModel()
        {
            
            RecurseSubDirectories = false;
            Query = new SearchQuery();      

            SearchCommand = new Command<SearchQuery>(new Action<SearchQuery>(async (query) =>
            {
                SearchCommand.CanExecute = false;
                await Task.Run(() => doSearch(query));
                SearchCommand.CanExecute = true;

            }));

            ClearRatingStartCommand = new Command(new Action(() =>
            {
                Query.RatingStart = null;
            }));

            ClearRatingEndCommand = new Command(new Action(() =>
            {
                Query.RatingEnd = null;
            }));
        }

        SearchQuery query;

        public SearchQuery Query
        {
            get { return query; }
            set { query = value;
            NotifyPropertyChanged();
            }
        }

        MediaType searchType;

        public MediaType SearchType
        {
            get { return searchType; }
            set { searchType = value;
            NotifyPropertyChanged();
            }
        }

        Command<SearchQuery> searchCommand;

        public Command<SearchQuery> SearchCommand
        {
            get { return searchCommand; }
            set
            {
                searchCommand = value;
                NotifyPropertyChanged();
            }
        }

        Command clearRatingStartCommand;

        public Command ClearRatingStartCommand
        {
            get { return clearRatingStartCommand; }
            set { clearRatingStartCommand = value; }
        }
        Command clearRatingEndCommand;

        public Command ClearRatingEndCommand
        {
            get { return clearRatingEndCommand; }
            set { clearRatingEndCommand = value; }
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
        
        private void doSearch(SearchQuery searchQuery)
        {
            if (searchQuery.IsEmpty) return;
       
            CancellationTokenSource tokenSource = new CancellationTokenSource();
                   
            List<MediaFileItem> results = dbSearch(searchQuery);

            foreach (MediaFileItem item in results)
            {
                item.ItemState = MediaFileItemState.EMPTY;
            }

            MediaFileWatcher.Instance.IsWatcherEnabled = false;          
            MediaFileWatcher.Instance.MediaState.clearUIState();
            MediaFileWatcher.Instance.MediaState.addUIState(results);
        }

        public List<MediaFileItem> dbSearch(SearchQuery searchQuery)
        {
            MediaDbCommands mediaCommands = new MediaDbCommands();

            List<Media> results = mediaCommands.findMediaByQuery(searchQuery);
            List<MediaFileItem> items = new List<MediaFileItem>();

            foreach (Media result in results)
            {
                items.Add(MediaFileItem.Factory.create(result.Location));
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
                Task task = item.readMetaDataAsync(MediaFactory.ReadOptions.READ_FROM_DISK, token);
                task.Wait();
                if (item.ItemState == MediaFileItemState.LOADED)
                {
                    foreach (Tag tag in tags)
                    {                      
                        if (item.Media.Tags.Contains(tag, new IgnoreCaseComparer()))
                        {
                            matches.Add(item);
                            continue;
                        }
                    }
                }
            }


            return (matches);
        }

        bool fileWalkerCallback(FileInfo info, Object state)
        {
            List<MediaFileItem> mediaItems = (List<MediaFileItem>)state;

            if (SearchType == MediaType.All && MediaFormatConvert.isMediaFile(info.Name))
            {
                mediaItems.Add(MediaFileItem.Factory.create(info.FullName));
            }
            else if (SearchType == MediaType.Video && MediaFormatConvert.isVideoFile(info.Name))
            {
                mediaItems.Add(MediaFileItem.Factory.create(info.FullName));
            }
            else if (SearchType == MediaType.Images && MediaFormatConvert.isImageFile(info.Name))
            {
                mediaItems.Add(MediaFileItem.Factory.create(info.FullName));
            }

            return (true);
        }

        

    }
}
