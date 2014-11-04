using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Utils;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.Search
{
  

    class SearchViewModel : BindableBase
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

        MediaFileWatcher mediaFileWatcher;

      
        public SearchViewModel(MediaFileWatcher mediaFileWatcher)
        {
            this.mediaFileWatcher = mediaFileWatcher;

            RecurseSubDirectories = false;
            Query = new SearchQuery();      

            SearchCommand = new Command<SearchQuery>(new Action<SearchQuery>(async (query) =>
            {
                SearchCommand.IsExecutable = false;
                await Task.Run(() => doSearch(query));
                SearchCommand.IsExecutable = true;

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
            set {  
                SetProperty(ref query, value);
            }
        }

        MediaType searchType;

        public MediaType SearchType
        {
            get { return searchType; }
            set {  
                SetProperty(ref searchType, value);
            }
        }

        public Command<SearchQuery> SearchCommand { get; set; }
        public Command ClearRatingStartCommand { get; set; }
        public Command ClearRatingEndCommand { get; set; }
            
        bool recurseSubDirectories;

        public bool RecurseSubDirectories
        {
            get { return recurseSubDirectories; }
            set
            {             
                SetProperty(ref recurseSubDirectories, value);
            }
        }
        
        private void doSearch(SearchQuery searchQuery)
        {
            if (searchQuery.IsEmpty) return;
       
            CancellationTokenSource tokenSource = new CancellationTokenSource();
                   
            List<MediaFileItem> results = dbSearch(searchQuery);
/*
            foreach (MediaFileItem item in results)
            {
                item.ItemState = MediaFileItemState.EMPTY;
            }
*/
            mediaFileWatcher.IsWatcherEnabled = false;
            mediaFileWatcher.MediaState.clearUIState("Search Result", DateTime.Now, MediaStateType.SearchResult);
            mediaFileWatcher.MediaState.addUIState(results);
        }

        public List<MediaFileItem> dbSearch(SearchQuery searchQuery)
        {
            MediaDbCommands mediaCommands = new MediaDbCommands();

            List<BaseMedia> results = mediaCommands.findMediaByQuery(searchQuery);
            List<MediaFileItem> items = new List<MediaFileItem>();

            foreach (BaseMedia result in results)
            {
                items.Add(MediaFileItem.Factory.create(result.Location));
            }

            return (items);
        }


        public List<MediaFileItem> diskTagSearch(List<Tag> tags, bool recurseSubDirectories, CancellationToken token)
        {

            String searchPath = mediaFileWatcher.Path;

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
