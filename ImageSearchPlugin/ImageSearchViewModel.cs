using MediaViewer;
using MediaViewer.DirectoryPicker;
using MediaViewer.MediaFileGrid;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Media.Metadata;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ImageSearchPlugin
{
  
    class ImageSearchViewModel : CloseableBindableBase
    {
        public static string[] safeSearch = { "Strict", "Moderate", "Off" };
        public static string[] size = { "All", "Small", "Medium", "Large" };
        public static string[] layout = { "All", "Square", "Wide", "Tall" };
        public static string[] type = { "All", "Photo", "Graphics" };
        public static string[] people = { "All", "Face", "Portrait", "Other" };
        public static string[] color = { "All", "Color", "Monochrome" };

        const String rootUri = "https://api.datamarket.azure.com/Bing/Search";
      
        public Command SearchCommand { get; set; }
        public Command CloseCommand { get; set; }
        public Command SelectAllCommand { get; set; }
        public Command DeselectAllCommand { get; set; }
        public Command<SelectableMediaItem> ViewCommand { get; set; }
        public Command<SelectableMediaItem> ViewSourceCommand { get; set; }
        public Command<SelectableMediaItem> DownloadCommand { get; set; }
        public GeoTagCoordinatePair GeoTag { get; set; }

        ImageSearchSettingsViewModel SettingsViewModel { get; set; }

        public ImageSearchViewModel()           
        {        
            NrColumns = 4;

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

            ViewCommand = new Command<SelectableMediaItem>((selectableItem) =>
                {
                    ImageResultItem item = (ImageResultItem)selectableItem.Item;
                    
                    if(item.ImageInfo.ContentType.Equals("image/animatedgif")) {

                        Shell.ShellViewModel.navigateToVideoView(item.ImageInfo.MediaUrl);  

                    } else {

                        Shell.ShellViewModel.navigateToImageView(item.ImageInfo.MediaUrl);     
                    }
                });

            ViewSourceCommand = new Command<SelectableMediaItem>((selectableItem) =>
                {
                    ImageResultItem item = (ImageResultItem)selectableItem.Item;

                    Process.Start(item.ImageInfo.SourceUrl);
                });

            SelectAllCommand = new Command(() =>
            {
                MediaStateCollectionView.selectAll();
            }, false);

            DeselectAllCommand = new Command(() =>
            {
                MediaStateCollectionView.deselectAll();
            });

            CloseCommand = new Command(() =>
            {             
                OnClosingRequest();
            });

            DownloadCommand = new Command<SelectableMediaItem>(async (selectableItem) =>
                {
                    List<MediaItem> items = MediaStateCollectionView.getSelectedItems();
                    if (items.Count == 0)
                    {
                        items.Add(selectableItem.Item);
                    }

                    String outputPath;

                    if (SettingsViewModel.Settings.IsAskDownloadPath)
                    {

                        DirectoryPickerView directoryPicker = new DirectoryPickerView();
                        directoryPicker.DirectoryPickerViewModel.InfoString = "Select Output Directory";
                        directoryPicker.DirectoryPickerViewModel.SelectedPath = MediaFileWatcher.Instance.Path;

                        if (directoryPicker.ShowDialog() == false)
                        {
                            return;
                        }

                        outputPath = directoryPicker.DirectoryPickerViewModel.SelectedPath;

                    }
                    else if (SettingsViewModel.Settings.IsCurrentDownloadPath)
                    {
                        outputPath = MediaFileWatcher.Instance.Path;
                    }
                    else
                    {
                        outputPath = SettingsViewModel.Settings.FixedDownloadPath;
                    }

                    CancellableOperationProgressView progressView = new CancellableOperationProgressView();
                    DownloadProgressViewModel vm = new DownloadProgressViewModel();
                    progressView.DataContext = vm;

                    progressView.Show();
                    vm.OkCommand.IsExecutable = false;
                    vm.CancelCommand.IsExecutable = true;

                    await Task.Factory.StartNew(() =>
                    {
                        vm.startDownload(outputPath, items);
                    });

                    vm.OkCommand.IsExecutable = true;
                    vm.CancelCommand.IsExecutable = false;
                });

            SettingsViewModel = (ImageSearchSettingsViewModel)ServiceLocator.Current.GetInstance(typeof(ImageSearchSettingsViewModel));

            Size = new ListCollectionView(size);
            Size.MoveCurrentTo(SettingsViewModel.Settings.Size);
            SafeSearch = new ListCollectionView(safeSearch);
            SafeSearch.MoveCurrentTo(SettingsViewModel.Settings.SafeSearch);
            Layout = new ListCollectionView(layout);
            Layout.MoveCurrentTo(SettingsViewModel.Settings.Layout);
            Type = new ListCollectionView(type);
            Type.MoveCurrentTo(SettingsViewModel.Settings.Type);
            People = new ListCollectionView(people);
            People.MoveCurrentTo(SettingsViewModel.Settings.People);
            Color = new ListCollectionView(color);
            Color.MoveCurrentTo(SettingsViewModel.Settings.Color);

            GeoTag = new GeoTagCoordinatePair();

            MediaStateCollectionView = new ImageResultCollectionView(new MediaState());        
            MediaStateCollectionView.MediaState.MediaStateType = MediaStateType.SearchResult;

            WeakEventManager<MediaLockedCollection, EventArgs>.AddHandler(MediaStateCollectionView.MediaState.UIMediaCollection, "IsLoadingChanged", mediaCollection_IsLoadingChanged);
            
        }

        private void mediaCollection_IsLoadingChanged(object sender, EventArgs e)
        {
            if (MediaStateCollectionView.MediaState.UIMediaCollection.IsLoading)
            {
                SelectAllCommand.IsExecutable = false;
            }
            else
            {
                SelectAllCommand.IsExecutable = true;
            }
        }

        MediaStateCollectionView mediaStateCollectionView;

        public MediaStateCollectionView MediaStateCollectionView
        {
            get { return mediaStateCollectionView; }
            set { SetProperty(ref mediaStateCollectionView, value); }
        }

        int nrColumns;

        public int NrColumns
        {
            get { return nrColumns; }
            set { SetProperty(ref nrColumns, value); }
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

            if (String.IsNullOrEmpty(Query) || String.IsNullOrWhiteSpace(Query))             
            {
                return;
            }

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
   
            var imageQuery = bingContainer.Image(Query, null, null, SafeSearch.CurrentItem.ToString(), GeoTag.LatDecimal, GeoTag.LonDecimal, imageFilters);

            SearchCommand.IsExecutable = false;

            Task.Factory.FromAsync(imageQuery.BeginExecute(null, null), (asyncResults) =>
            {
                SearchCommand.IsExecutable = true;
                var imageResults = imageQuery.EndExecute(asyncResults);

                MediaStateCollectionView.MediaState.clearUIState(Query, DateTime.Now, MediaStateType.SearchResult);

                List<MediaItem> results = new List<MediaItem>();

                foreach (var image in imageResults)
                {
                    results.Add(new ImageResultItem(image));
                }

                MediaStateCollectionView.MediaState.addUIState(results);

            });
                                                    
        }
    }
}
