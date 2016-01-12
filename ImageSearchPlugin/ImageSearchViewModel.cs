using ImageSearchPlugin.Properties;
using MediaViewer;
using MediaViewer.DirectoryPicker;
using MediaViewer.MediaFileGrid;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Media.Metadata;
using MediaViewer.Model.Media.Base.State;
using MediaViewer.Model.Media.Base.State.CollectionView;
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
using MediaViewer.Model.Media.Base.Item;

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
      
        public AsyncCommand<int> SearchCommand { get; set; }
        public Command CloseCommand { get; set; }
        public Command SelectAllCommand { get; set; }
        public Command DeselectAllCommand { get; set; }
        public Command<SelectableMediaItem> ViewCommand { get; set; }
        public Command<SelectableMediaItem> ViewSourceCommand { get; set; }
        public AsyncCommand<SelectableMediaItem> DownloadCommand { get; set; }
        public GeoTagCoordinatePair GeoTag { get; set; }

        ImageSearchSettingsViewModel SettingsViewModel { get; set; }

        public MediaState MediaState { get; set; }
        public MediaStateCollectionView MediaStateCollectionView { get; set; }

        ImageSearchQuery CurrentQuery { get; set; }

        public ImageSearchViewModel()           
        {                   
            NrColumns = 4;

            SearchCommand = new AsyncCommand<int>(async (imageOffset) =>
            {
                try
                {
                    if (imageOffset == 0)
                    {
                        CurrentQuery = new ImageSearchQuery(this);
                    }

                    SearchCommand.IsExecutable = false;
                    await doSearch(CurrentQuery, imageOffset);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Image search error\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    SearchCommand.IsExecutable = true;
                }
            });

            ViewCommand = new Command<SelectableMediaItem>((selectableItem) =>
                {
                    ImageResultItem item = (ImageResultItem)selectableItem.Item;
                    
                    if(item.ImageInfo.ContentType.Equals("image/animatedgif")) {

                        Shell.ShellViewModel.navigateToVideoView(item);  

                    } else {

                        Shell.ShellViewModel.navigateToImageView(item);     
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

            DownloadCommand = new AsyncCommand<SelectableMediaItem>(async (selectableItem) =>
                {
                    List<MediaItem> items = MediaStateCollectionView.getSelectedItems();
                    if (items.Count == 0)
                    {
                        items.Add(selectableItem.Item);
                    }

                    String outputPath;

                    if (SettingsViewModel.IsAskDownloadPath)
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
                    else if (SettingsViewModel.IsCurrentDownloadPath)
                    {
                        outputPath = MediaFileWatcher.Instance.Path;
                    }
                    else
                    {
                        outputPath = SettingsViewModel.FixedDownloadPath;
                    }

                    CancellableOperationProgressView progressView = new CancellableOperationProgressView();
                    DownloadProgressViewModel vm = new DownloadProgressViewModel();
                    progressView.DataContext = vm;

                    progressView.Show();
                    vm.OkCommand.IsExecutable = false;
                    vm.CancelCommand.IsExecutable = true;

                    try
                    {
                        await Task.Factory.StartNew(() =>
                        {
                            vm.startDownload(outputPath, items);

                        }, vm.CancellationToken);

                    }
                    catch (Exception)
                    {

                    }

                    vm.OkCommand.IsExecutable = true;
                    vm.CancelCommand.IsExecutable = false;
                });

            SettingsViewModel = (ImageSearchSettingsViewModel)ServiceLocator.Current.GetInstance(typeof(ImageSearchSettingsViewModel));
           
            Size = new ListCollectionView(size);
            Size.MoveCurrentTo(Settings.Default.Size);
            SafeSearch = new ListCollectionView(safeSearch);
            SafeSearch.MoveCurrentTo(Settings.Default.SafeSearch);
            Layout = new ListCollectionView(layout);
            Layout.MoveCurrentTo(Settings.Default.Layout);
            Type = new ListCollectionView(type);
            Type.MoveCurrentTo(Settings.Default.Type);
            People = new ListCollectionView(people);
            People.MoveCurrentTo(Settings.Default.People);
            Color = new ListCollectionView(color);
            Color.MoveCurrentTo(Settings.Default.Color);

            GeoTag = new GeoTagCoordinatePair();

            MediaState = new MediaState();

            MediaStateCollectionView = new ImageResultCollectionView(MediaState);        
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
       
        async Task doSearch(ImageSearchQuery state, int imageOffset)
        {
            if (String.IsNullOrEmpty(state.Query) || String.IsNullOrWhiteSpace(state.Query))             
            {
                return;
            }

            var bingContainer = new Bing.BingSearchContainer(new Uri(rootUri));
           
            // Configure bingContainer to use your credentials.

            bingContainer.Credentials = new NetworkCredential(BingAccountKey.accountKey, BingAccountKey.accountKey);           

            // Build the query.
            String imageFilters = null;

            if (!state.Size.Equals(size[0]))
            {
                imageFilters = "Size:" + state.Size;
            }

            if (!state.Layout.Equals(layout[0]))
            {
                if (imageFilters != null) imageFilters += "+";

                imageFilters += "Aspect:" + state.Layout;
            }

            if (!state.Type.Equals(type[0]))
            {
                if (imageFilters != null) imageFilters += "+";

                imageFilters += "Style:" + state.Type;
            }

            if (!state.People.Equals(type[0]))
            {
                if (imageFilters != null) imageFilters += "+";

                imageFilters += "Face:" + state.People;
            }

            if (!state.Color.Equals(type[0]))
            {
                if (imageFilters != null) imageFilters += "+";

                imageFilters += "Color:" + state.Color;
            }
               
            var imageQuery = bingContainer.Image(Query, null, null, state.SafeSearch, state.GeoTag.LatDecimal, state.GeoTag.LonDecimal, imageFilters);
            imageQuery = imageQuery.AddQueryOption("$top", 50);
            imageQuery = imageQuery.AddQueryOption("$skip", imageOffset);
                        
            IEnumerable<Bing.ImageResult> imageResults = 
                await Task.Factory.FromAsync<IEnumerable<Bing.ImageResult>>(imageQuery.BeginExecute(null, null), imageQuery.EndExecute);
                               
            if (imageOffset == 0)
            {
                MediaState.clearUIState(Query, DateTime.Now, MediaStateType.SearchResult);
            }

            List<MediaItem> results = new List<MediaItem>();

            int relevance = imageOffset;

            MediaState.UIMediaCollection.EnterReadLock();

            foreach (var image in imageResults)
            {
                MediaItem item = new ImageResultItem(image, relevance);

                if (MediaState.UIMediaCollection.Contains(item)) continue;

                results.Add(item);

                relevance++;
            }

            MediaState.UIMediaCollection.ExitReadLock();

            MediaState.addUIState(results);
                                                                
        }

        public void shutdown()
        {          
            Settings.Default.Size = (String)Size.CurrentItem;
            Settings.Default.SafeSearch = (String)SafeSearch.CurrentItem;
            Settings.Default.Layout = (String)Layout.CurrentItem;
            Settings.Default.Type = (String)Type.CurrentItem;
            Settings.Default.People = (String)People.CurrentItem;
            Settings.Default.Color = (String)Color.CurrentItem;
        }
    }
}
