using MediaViewer.MediaGrid;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GoogleImageSearchPlugin
{
  
    class GoogleImageSearchViewModel : MediaGridViewModel
    {
        string[] safeSearch = { "Strict", "Moderate", "Off" };
        string[] size = { "All", "Small", "Medium", "Large" };
        string[] layout = { "All", "Square", "Wide", "Tall" };
        string[] type = { "All", "Photo", "Graphics"};
        string[] people = { "All", "Face", "Portrait", "Other" };
        string[] color = { "All", "Color", "Monochrome"};

        const String rootUri = "https://api.datamarket.azure.com/Bing/Search";
      
        public Command SearchCommand { get; set; }
       
        public GoogleImageSearchViewModel() :
            base(new MediaState(), null)
        {        
            NrGridColumns = 4;

            SearchCommand = new Command(() =>
            {
                try
                {
                    doSearch();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Image search error\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); 
                }
            });

            Size = new ListCollectionView(size);
            SafeSearch = new ListCollectionView(safeSearch);
            Layout = new ListCollectionView(layout);
            Type = new ListCollectionView(type);
            People = new ListCollectionView(people);
            Color = new ListCollectionView(color);

            MediaState.MediaStateType = MediaStateType.SearchResult;
        }

        protected override void MediaStateCollectionView_SelectionChanged(object sender, EventArgs e)
        {
           // do nothing
        }
      
        String query;

        public String Query
        {
            get { return query; }
            set { SetProperty(ref query, value);             
            }
        }

        public ListCollectionView Size { get; set; }
        public ListCollectionView SafeSearch { get; set; }
        public ListCollectionView Layout { get; set; }
        public ListCollectionView Type { get; set; }
        public ListCollectionView People { get; set; }
        public ListCollectionView Color { get; set; }
       
        void doSearch()
        {

            if (String.IsNullOrEmpty(Query) || String.IsNullOrWhiteSpace(Query)) return;

            var bingContainer = new Bing.BingSearchContainer(new Uri(rootUri));
           
            // Configure bingContainer to use your credentials.

            bingContainer.Credentials = new NetworkCredential(AccountKey.accountKey, AccountKey.accountKey);

            // Build the query.
            String imageFilters = null;

            if (!(Size.CurrentItem as String).Equals(size[0]))
            {
                imageFilters = "Size:" + Size.CurrentItem;
            }

            if (!(Layout.CurrentItem as String).Equals(layout[0]))
            {
                if (imageFilters != null) imageFilters += "+";

                imageFilters += "Aspect:" + Layout.CurrentItem;
            }

            if (!(Type.CurrentItem as String).Equals(type[0]))
            {
                if (imageFilters != null) imageFilters += "+";

                imageFilters += "Style:" + Type.CurrentItem;
            }

            if (!(People.CurrentItem as String).Equals(type[0]))
            {
                if (imageFilters != null) imageFilters += "+";

                imageFilters += "Face:" + People.CurrentItem;
            }

            if (!(Color.CurrentItem as String).Equals(type[0]))
            {
                if (imageFilters != null) imageFilters += "+";

                imageFilters += "Color:" + Color.CurrentItem;
            }

            var imageQuery = bingContainer.Image(Query, null, null, SafeSearch.CurrentItem.ToString(), null, null, imageFilters);

            SearchCommand.IsExecutable = false;

            Task.Factory.FromAsync(imageQuery.BeginExecute(null, null), (asyncResults) =>
            {
                SearchCommand.IsExecutable = true;
                var imageResults = imageQuery.EndExecute(asyncResults);

                MediaState.clearUIState(Query, DateTime.Now, MediaStateType.SearchResult);

                List<MediaItem> results = new List<MediaItem>();

                foreach (var image in imageResults)
                {
                    results.Add(new ImageResultItem(image));
                }

                MediaState.addUIState(results);

            });
                                                    
        }
    }
}
