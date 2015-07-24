using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.metadata.Metadata;
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

        MediaFileWatcher MediaFileWatcher { get; set; }
      
        public SearchViewModel(MediaFileWatcher mediaFileWatcher)
        {
            MediaFileWatcher = mediaFileWatcher;

            RecurseSubDirectories = false;
            Query = new SearchQuery();      

            SearchCommand = new AsyncCommand<SearchQuery>(async (query) =>
            {
                SearchCommand.IsExecutable = false;

                await Task.Run(() => doSearch(query));

                SearchCommand.IsExecutable = true;

            });

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

        public AsyncCommand<SearchQuery> SearchCommand { get; set; }
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

            MediaFileWatcher.IsWatcherEnabled = false;
            MediaFileWatcher.MediaFileState.clearUIState("Search Result", DateTime.Now, MediaStateType.SearchResult);
            MediaFileWatcher.MediaFileState.addUIState(results);
        }

        public List<MediaFileItem> dbSearch(SearchQuery searchQuery)
        {
            using (MetadataDbCommands mediaCommands = new MetadataDbCommands())
            {
                List<BaseMetadata> results = mediaCommands.findMetadataByQuery(searchQuery);
                List<MediaFileItem> items = new List<MediaFileItem>();

                foreach (BaseMetadata result in results)
                {
                    items.Add(MediaFileItem.Factory.create(result.Location));
                }

                return (items);
            }
           
        }

    }
}
