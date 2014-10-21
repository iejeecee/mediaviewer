using HtmlAgilityPack;
using MediaViewer.MediaFileBrowser;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.MetaData;
using MediaViewer.Pager;
using Microsoft.Practices.Prism.Regions;
using MvvmFoundation.Wpf;
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

namespace PluginTest
{
    [Export]
    public class GoogleEarthGeoTagViewModel : ObservableObject, IDisposable, INavigationAware
    {

        MediaStackPanelViewModel mediaStackPanel;

        MediaFileItem SelectedItem { get; set; }

        MediaFileBrowserView MediaFileBrowserView { get; set; }
        IEventAggregator EventAggregator { get; set; }

        GeoTagScriptInterface script;
      
        [ImportingConstructor]
        public GoogleEarthGeoTagViewModel(MediaFileBrowserView mediaFileBrowserView, IEventAggregator eventAggregator)
        {
            mediaStackPanel = new MediaStackPanelViewModel(MediaFileWatcher.Instance.MediaState, eventAggregator);
            mediaStackPanel.IsVisible = true;      
            mediaStackPanel.MediaStateCollectionView.NrItemsInStateChanged += mediaState_NrItemsInStateChanged;
           
            EventAggregator = eventAggregator;

            MediaFileBrowserView = mediaFileBrowserView;
            WebBrowser = new System.Windows.Controls.WebBrowser();
       
            // get html and javascript embedded resources
            Stream htmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PluginTest.GeoTag.html");
            Stream scriptStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PluginTest.GeoTag.js");

            // insert javascript into html document
            scriptStream.Position = 0;
            String scriptString;
            using (StreamReader reader = new StreamReader(scriptStream, Encoding.UTF8))
            {
                scriptString = reader.ReadToEnd();
            }

            scriptStream.Close();

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
          
            script = new GeoTagScriptInterface(WebBrowser);

            WebBrowser.ObjectForScripting = script;
        
            script.PlaceMarkClicked += script_PlaceMarkClicked;
            script.AddressUpdate += script_AddressUpdate;
            script.PlaceMarkMoved += script_PlaceMarkMoved;
            script.EndPlaceMarkMoved += script_EndPlaceMarkMoved;

            SearchCommand = new Command(() =>
            {
                if (String.IsNullOrEmpty(searchText) || String.IsNullOrWhiteSpace(searchText)) return;

                script.flyTo(searchText);
            });
                 
            CreateGeoTagCommand = new Command(() =>
                {
                    GeoTagFileData geoTagItem = getSelectedGeoTag();
                    if(geoTagItem != null && geoTagItem.HasGeoTag == false) {

                        script.createPlaceMark(geoTagItem, true);

                        DeleteGeoTagCommand.CanExecute = true;
                        CreateGeoTagCommand.CanExecute = false;
                    }

                    selectPlaceMark(geoTagItem, false);
                   
                });

            DeleteGeoTagCommand = new Command(() =>
                {
                    GeoTagFileData geoTagItem = getSelectedGeoTag();
                    if (geoTagItem != null && geoTagItem.HasGeoTag == true)
                    {
                        script.deletePlaceMark(geoTagItem);

                        Latitude = "";
                        Longitude = "";
                        GeoTagLocation = "";

                        CreateGeoTagCommand.CanExecute = true;
                        DeleteGeoTagCommand.CanExecute = false;
                    }

                    selectPlaceMark(geoTagItem, false);
                }, false);
        

            drawRoads = false;
            drawBorders = false;
        }

        void script_EndPlaceMarkMoved(object sender, GeoTagFileData e)
        {
            selectPlaceMark(e, false);
        }

        void script_PlaceMarkMoved(object sender, GeoTagFileData e)
        {
            Latitude = e.GeoTag.Latitude.Coord;
            Longitude = e.GeoTag.Longitude.Coord;
        }

        void script_AddressUpdate(object sender, string location)
        {
            GeoTagLocation = location;
        }

        String geoTagLocation;

        public String GeoTagLocation
        {
            get { return geoTagLocation; }
            set { geoTagLocation = value;
            NotifyPropertyChanged();
            }
        }

        String latitude;

        public String Latitude
        {
            get { return latitude; }
            set { latitude = value;
            NotifyPropertyChanged();
            }
        }

        String longitude;

        public String Longitude
        {
            get { return longitude; }
            set { longitude = value;
            NotifyPropertyChanged();
            }
        }

        GeoTagFileData getSelectedGeoTag()
        {
           
            foreach (GeoTagFileData geoTagItem in script.GeoTagFileItems)
            {
                if (SelectedItem.Equals(geoTagItem.MediaFileItem))
                {
                    return (geoTagItem);
                }
            }

            return (null);
        }

        void selectPlaceMark(GeoTagFileData geoTagItem, bool lookAtGeoTag = true)
        {
            if (Object.Equals(SelectedItem,geoTagItem.MediaFileItem))
            {
                return;
            }
            else
            {
                SelectedItem = geoTagItem.MediaFileItem;
            }
                  
            if (geoTagItem.HasGeoTag)
            {
                script.reverseGeoCodePlaceMark(geoTagItem);
                Latitude = geoTagItem.GeoTag.Latitude.Coord;
                Longitude = geoTagItem.GeoTag.Longitude.Coord;

                if (lookAtGeoTag)
                {
                    script.lookAtPlaceMark(geoTagItem);
                }

                DeleteGeoTagCommand.CanExecute = true;
                CreateGeoTagCommand.CanExecute = false;
            }
            else
            {
                Latitude = "";
                Longitude = "";
                GeoTagLocation = "";

                DeleteGeoTagCommand.CanExecute = false;
                CreateGeoTagCommand.CanExecute = true;
            }

            EventAggregator.GetEvent<MediaViewer.Model.GlobalEvents.MediaSelectionEvent>().Publish(SelectedItem);
         
        }

        private void script_PlaceMarkClicked(object sender, GeoTagFileData e)
        {
            selectPlaceMark(e, false);
        }

     

        WebBrowser webBrowser;

        public WebBrowser WebBrowser
        {
            get { return webBrowser; }
            set { webBrowser = value;
            NotifyPropertyChanged();
            }
        }

     
        public void Dispose()
        {
            if (WebBrowser != null)
            {
                WebBrowser.Dispose();
                WebBrowser = null;
            }
        }   

        String searchText;

        public String SearchText
        {
            get { return searchText; }
            set { searchText = value;
            NotifyPropertyChanged();
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

        private void mediaState_NrItemsInStateChanged(object sender, MediaStateChangedEventArgs e)
        {
            if (script.IsInitialized == false) return;
           
            switch (e.Action)
            {
                case MediaStateChangedAction.Add:
                    foreach (MediaFileItem item in e.NewItems)
                    {
                        script.addGeoTagItem(new GeoTagFileData(item));
                    }
                    break;
                case MediaStateChangedAction.Remove:
                    foreach (MediaFileItem item in e.OldItems)
                    {
                        script.removeGeoTagItem(new GeoTagFileData(item));
                    }                    
                    break;
                case MediaStateChangedAction.Modified:
                    break;
                case MediaStateChangedAction.Clear:
                    script.clearAll();
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
            EventAggregator.GetEvent<MediaViewer.Model.GlobalEvents.MediaSelectionEvent>().Unsubscribe(geoTagViewModel_MediaSelectionEvent);
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            bool isScriptInitialized = script.IsInitialized;

            ICollection<MediaFileItem> selectedItems = (ICollection<MediaFileItem>)navigationContext.Parameters["selectedItems"];

            mediaStackPanel.MediaStateCollectionView.Filter = new Func<SelectableMediaFileItem, bool>((item) =>
            {
                return (selectedItems.Contains(item.Item));
            });

            Shell.ShellViewModel.navigateToMediaStackPanelView(mediaStackPanel);

            await script.InitializingSemaphore.WaitAsync();
                                  
            MediaFileBrowserView.MediaFileBrowserViewModel.CurrentViewModel = this;         

            EventAggregator.GetEvent<MediaViewer.Model.GlobalEvents.MediaSelectionEvent>().Subscribe(geoTagViewModel_MediaSelectionEvent);

            // initialize if first run and select first item
            mediaStackPanel.MediaStateCollectionView.Media.EnterReaderLock();
            try
            {
                if (!isScriptInitialized && script.IsError == false)
                {
                    foreach (SelectableMediaFileItem item in mediaStackPanel.MediaStateCollectionView.Media)
                    {
                        script.addGeoTagItem(new GeoTagFileData(item.Item));
                    }
                }

                if (mediaStackPanel.MediaStateCollectionView.Media.Count > 0)
                {
                    EventAggregator.GetEvent<MediaViewer.Model.GlobalEvents.MediaSelectionEvent>().Publish(mediaStackPanel.MediaStateCollectionView.Media[0].Item);
                }
            }
            finally
            {
                mediaStackPanel.MediaStateCollectionView.Media.ExitReaderLock();
            }
            
            script.InitializingSemaphore.Release();
        }

        private void geoTagViewModel_MediaSelectionEvent(MediaFileItem selectedItem)
        {
            GeoTagFileData geoTagItem = script.getGeoTagFileData(selectedItem);

            if (geoTagItem != null)
            {
                selectPlaceMark(geoTagItem);
            }
        }
        
        bool drawRoads;

        public bool DrawRoads
        {
            get { return drawRoads; }
            set { drawRoads = value;
            script.setRoads(drawRoads);
            NotifyPropertyChanged();
            }
        }
        bool drawBorders;

        public bool DrawBorders
        {
            get { return drawBorders; }
            set { drawBorders = value;
            script.setBorders(drawBorders);
            NotifyPropertyChanged();
            }
        }
    }
}
