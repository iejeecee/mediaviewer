using HtmlAgilityPack;
using MediaViewer.MediaFileBrowser;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.MetaData;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Resources;
using MediaViewer.Model.Media.File;
using Microsoft.Practices.Prism.PubSubEvents;
using MediaViewer.MediaGrid;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer;
using MediaViewer.Model.Media.State;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Commands;
using MediaViewer.Infrastructure.Global.Commands;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Media.Base;

namespace GoogleEarthGeoTagPlugin
{
    [Export]
    public class GoogleEarthGeoTagViewModel : BindableBase, IDisposable, INavigationAware, IMediaFileBrowserContentViewModel
    {

        MediaStackPanelViewModel mediaStackPanel;

        GeoTagFileItem selectedItem;

        public GeoTagFileItem SelectedItem
        {
            get
            {
                return (selectedItem);
            }
            set
            {
                SetProperty(ref selectedItem, value);
            }
        }

        MediaFileBrowserView MediaFileBrowserView { get; set; }
        IEventAggregator EventAggregator { get; set; }
        ICollection<MediaFileItem> SelectedItems { get; set; }

        GoogleEarthScriptInterface script;
      
        [ImportingConstructor]
        public GoogleEarthGeoTagViewModel(MediaFileBrowserView mediaFileBrowserView, IEventAggregator eventAggregator)
        {
            mediaStackPanel = new MediaStackPanelViewModel(MediaFileWatcher.Instance.MediaFileState, eventAggregator);
            mediaStackPanel.IsVisible = true;      
            mediaStackPanel.MediaStateCollectionView.NrItemsInStateChanged += mediaState_NrItemsInStateChanged;

            EventAggregator = eventAggregator;

            MediaFileBrowserView = mediaFileBrowserView;
            WebBrowser = new System.Windows.Controls.WebBrowser();
       
            // get html and javascript embedded resources
            Stream htmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GoogleEarthGeoTagPlugin.GeoTag.html");
            Stream scriptStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GoogleEarthGeoTagPlugin.GeoTag.js");

            // insert javascript into html document
            scriptStream.Position = 0;
            String scriptString;
            using (StreamReader reader = new StreamReader(scriptStream, Encoding.UTF8))
            {
                scriptString = reader.ReadToEnd();
            }

            HtmlDocument document = new HtmlDocument();
            document.Load(htmlStream);

            htmlStream.Close();

            HtmlNode head = document.DocumentNode.SelectSingleNode("/html/head");
            HtmlNode node = document.CreateElement("script");
            head.AppendChild(node);
            node.SetAttributeValue("type", "text/javascript");
            node.AppendChild(document.CreateTextNode(scriptString));
       
            // navigate to html document in webbrowser
            MemoryStream stream = new MemoryStream();
            //document.Save("d:\\mijnhtml.html");
            document.Save(stream);
            stream.Position = 0;                               

            WebBrowser.NavigateToStream(stream);
          
            script = new GoogleEarthScriptInterface(WebBrowser);

            WebBrowser.ObjectForScripting = script;
        
            script.PlaceMarkClicked += script_PlaceMarkClicked;               
           

            SearchCommand = new Command(async () =>
            {
                if (String.IsNullOrEmpty(searchText) || String.IsNullOrWhiteSpace(searchText)) return;

                await script.flyTo(searchText);
            });
                 
            CreateGeoTagCommand = new Command(async () =>
                {
                    if (SelectedItem != null && SelectedItem.HasGeoTag == false)
                    {
                        await script.createPlaceMark(SelectedItem, true);

                        DeleteGeoTagCommand.IsExecutable = true;
                        CreateGeoTagCommand.IsExecutable = false;

                        await selectPlaceMark(SelectedItem, false, true);
                    }
                                      
                });

            DeleteGeoTagCommand = new Command(async () =>
                {
                    if (SelectedItem != null && SelectedItem.HasGeoTag == true)
                    {
                        await script.deletePlaceMark(SelectedItem);                 

                        CreateGeoTagCommand.IsExecutable = true;
                        DeleteGeoTagCommand.IsExecutable = false;

                        await selectPlaceMark(SelectedItem, false, true);
                    }                   

                }, false);

            MetaDataUpdateCommand = new Command<MetaDataUpdateViewModelAsyncState>((state) =>
            {
                foreach (MediaFileItem item in state.ItemList)
                {
                    GeoTagFileItem tagItem = script.get(item);

                    if (tagItem != null && item.Metadata != null)
                    {
                        item.RWLock.EnterWriteLock();
                        try
                        {
                            String latitude = tagItem.HasGeoTag ? tagItem.GeoTag.Latitude.Coord : null;
                            String longitude = tagItem.HasGeoTag ? tagItem.GeoTag.Longitude.Coord : null;

                            if (!EqualityComparer<String>.Default.Equals(item.Metadata.Latitude, latitude))
                            {
                                item.Metadata.Latitude = latitude;
                                item.Metadata.IsModified = true;
                            }

                            if (!EqualityComparer<String>.Default.Equals(item.Metadata.Longitude, longitude))
                            {
                                item.Metadata.Longitude = longitude;
                                item.Metadata.IsModified = true;
                            }
                        }
                        finally
                        {
                            item.RWLock.ExitWriteLock();
                        }

                    }

                }
            });

            drawRoads = false;
            drawBorders = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool safe)
        {
            if (safe)
            {
                if (WebBrowser != null)
                {
                    WebBrowser.Dispose();
                    WebBrowser = null;
                }
            }
        }
                 
        async Task selectPlaceMark(GeoTagFileItem item, bool lookAtGeoTag = true, bool reselect = false)
        {
            if (!reselect && Object.Equals(SelectedItem, item))
            {
                return;
            }
           
            SelectedItem = item;
                              
            if (item.HasGeoTag)
            {
                if (String.IsNullOrEmpty(item.GeoTagLocation))
                {
                    await script.reverseGeoCodePlaceMark(item);
                }
              
                if (lookAtGeoTag)
                {
                    await script.lookAtPlaceMark(item);
                }

                DeleteGeoTagCommand.IsExecutable = true;
                CreateGeoTagCommand.IsExecutable = false;
            }
            else
            {              
                DeleteGeoTagCommand.IsExecutable = false;
                CreateGeoTagCommand.IsExecutable = true;
            }

            EventAggregator.GetEvent<MediaViewer.Model.Global.Events.MediaSelectionEvent>().Publish(SelectedItem.MediaFileItem);
         
        }

        private async void script_PlaceMarkClicked(object sender, GeoTagFileItem e)
        {
            await selectPlaceMark(e, false);
        }

        WebBrowser webBrowser;

        public WebBrowser WebBrowser
        {
            get { return webBrowser; }
            set { 
                SetProperty(ref webBrowser, value);           
            }
        }

     
      

        String searchText;

        public String SearchText
        {
            get { return searchText; }
            set { 
                SetProperty(ref searchText, value);      
            }
        }
   
        public Command SearchCommand
        {
            get;
            set;
        }

        public Command CreateGeoTagCommand
        {
            get;
            set;
        }

        public Command DeleteGeoTagCommand
        {
            get;
            set;
        }

        public Command<MetaDataUpdateViewModelAsyncState> MetaDataUpdateCommand { get; set; }

        private async void mediaState_NrItemsInStateChanged(object sender, MediaStateChangedEventArgs e)
        {
 
            switch (e.Action)
            {
                case MediaStateChangedAction.Add:
                    foreach (MediaFileItem item in e.NewItems)
                    {
                        await script.add(new GeoTagFileItem(item));
                    }
                    break;
                case MediaStateChangedAction.Remove:
                    foreach (MediaFileItem item in e.OldItems)
                    {
                        await script.remove(new GeoTagFileItem(item));
                        if (Object.Equals(item, SelectedItem))
                        {
                            SelectedItem = null;  
                        }
                    }                    
                    break;
                case MediaStateChangedAction.Modified:
                    break;
                case MediaStateChangedAction.Clear:
                    await script.clearAll();
                    SelectedItem = null;  
                    break;
                default:
                    break;
            }
                                  
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return (true);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            GlobalCommands.MetaDataUpdateCommand.UnregisterCommand(MetaDataUpdateCommand);

            EventAggregator.GetEvent<MediaViewer.Model.Global.Events.MediaSelectionEvent>().Unsubscribe(geoTagViewModel_MediaSelectionEvent);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            ICollection<MediaFileItem> selectedItems = (ICollection<MediaFileItem>)navigationContext.Parameters["selectedItems"];

            if (selectedItems != null)
            {
                SelectedItems = selectedItems;

                mediaStackPanel.MediaStateCollectionView.Filter = new Func<SelectableMediaItem, bool>((item) =>
                {
                    return (SelectedItems.Contains(item.Item));
                });
            }
         
            Shell.ShellViewModel.navigateToMediaStackPanelView(mediaStackPanel);

            MediaFileBrowserView.MediaFileBrowserViewModel.CurrentViewModel = this;

            GlobalCommands.MetaDataUpdateCommand.RegisterCommand(MetaDataUpdateCommand);

            EventAggregator.GetEvent<MediaViewer.Model.Global.Events.MediaSelectionEvent>().Subscribe(geoTagViewModel_MediaSelectionEvent);
     
        }

        private async void geoTagViewModel_MediaSelectionEvent(MediaItem selectedItem)
        {
            GeoTagFileItem item = script.get(selectedItem as MediaFileItem);

            if (item != null)
            {
                await selectPlaceMark(item);
            }
        }
        
        bool drawRoads;

        public bool DrawRoads
        {
            get { return drawRoads; }
            set
            {
                if (script.IsInitialized == false) return;

                SetProperty(ref drawRoads, value);

                Action func = async () =>
                {
                    await script.setRoads(drawRoads);
                };

                func();       
            }
        }
        bool drawBorders;

        public bool DrawBorders
        {
            get { return drawBorders; }
            set
            {
                if (script.IsInitialized == false) return;

                SetProperty(ref drawBorders, value);

                Action func = async () =>
                {
                    await script.setBorders(drawBorders);                   
                }; 

                func();
            }
        }
    }
}
