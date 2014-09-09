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

namespace PluginTest
{
    [Export]
    public class GoogleEarthGeoTagViewModel : ObservableObject, IDisposable, ISelectedMedia, INavigationAware, IPageable
    {
        MediaFileBrowserView MediaFileBrowserView { get; set; }

        GeoTagScriptInterface script;
      
        [ImportingConstructor]
        public GoogleEarthGeoTagViewModel(MediaFileBrowserView mediaFileBrowserView)
        {
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

            script.Initialized += script_Initialized;
            script.PlaceMarkClicked += script_PlaceMarkClicked;
            script.AddressUpdate += script_AddressUpdate;
            script.PlaceMarkMoved += script_PlaceMarkMoved;
            script.EndPlaceMarkMoved += script_EndPlaceMarkMoved;

            SearchCommand = new Command(() =>
            {
                if (String.IsNullOrEmpty(searchText) || String.IsNullOrWhiteSpace(searchText)) return;

                script.flyTo(searchText);
            });

            SelectedMedia = new ObservableCollection<MediaFileItem>();

            currentPage = 0;
            nrPages = 0;
            isPagingEnabled = false;

            NextPageCommand = new Command(() =>
            {
                if (NrPages != 0 && CurrentPage != NrPages)
                {
                    CurrentPage = CurrentPage + 1;
                }

            });

            PrevPageCommand = new Command(() =>
            {
                if (NrPages != 0 && CurrentPage > 1)
                {
                    CurrentPage = CurrentPage - 1;
                }
            });

            LastPageCommand = new Command(() =>
            {
                if (NrPages != 0)
                {
                    CurrentPage = NrPages;
                }
            });

            FirstPageCommand = new Command(() =>
            {
                if (NrPages != 0)
                {
                    CurrentPage = 1;
                }
             
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
            MediaFileItem item = SelectedMedia[0];

            foreach (GeoTagFileData geoTagItem in script.GeoTagFileItems)
            {
                if (item.Equals(geoTagItem.MediaFileItem))
                {
                    return (geoTagItem);
                }
            }

            return (null);
        }

        void selectPlaceMark(GeoTagFileData geoTagItem, bool lookAtGeoTag = true)
        {
            if (!(SelectedMedia.Count > 0 && SelectedMedia[0].Equals(geoTagItem.MediaFileItem)))
            {
                SelectedMedia.Clear();
                SelectedMedia.Add(geoTagItem.MediaFileItem);
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

         
        }

        private void script_PlaceMarkClicked(object sender, GeoTagFileData e)
        {
            selectPlaceMark(e, false);
        }

        void script_Initialized(object sender, EventArgs e)
        {
            if (Loaded != null)
            {
                Loaded(this, EventArgs.Empty);
            }
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

        public event EventHandler Loaded;

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

        public ObservableCollection<MediaFileItem> SelectedMedia
        {
            get; set;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return (true);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await script.initializing.WaitAsync();
           
            script.clearAll();

            SelectedMedia.Clear();

            /*for (int i = 0; i < MediaFileBrowserView.MediaFileBrowserViewModel.CurrentViewModel.SelectedMedia.Count; i++)
            {
                MediaFileItem item = MediaFileBrowserView.MediaFileBrowserViewModel.CurrentViewModel.SelectedMedia[i];

                GeoTagFileData geoTagItem = new GeoTagFileData(item);
                script.addGeoTagItem(geoTagItem);

                if (i == 0)
                {
                    selectPlaceMark(geoTagItem);
                }
            }*/
           
            MediaFileBrowserView.MediaFileBrowserViewModel.CurrentViewModel = this;

            NrPages = script.GeoTagFileItems.Count;
            CurrentPage = 1;
            
            script.initializing.Release();
        }

        int nrPages;

        public int NrPages
        {
            get
            {
                return(nrPages);
            }
            set
            {
                nrPages = value;
                if (nrPages > 0)
                {
                    IsPagingEnabled = true;
                }
                else
                {
                    IsPagingEnabled = false;
                }
                NotifyPropertyChanged();
            }
        }

        int currentPage;

        public int CurrentPage
        {
            get
            {
                return (currentPage);
            }
            set
            {
                if (NrPages != 0)
                {
                    int newValue = value <= 0 ? 1 : (value > NrPages ? NrPages : value);

                    GeoTagFileData geoTagItem = script.GeoTagFileItems[newValue - 1];
                    selectPlaceMark(geoTagItem);

                    currentPage = newValue;

                }
                else
                {
                    currentPage = 0;
                }
               
                NotifyPropertyChanged();
            }
        }

        bool isPagingEnabled;

        public bool IsPagingEnabled
        {
            get
            {
                return (isPagingEnabled);
            }
            set
            {
                isPagingEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public Command NextPageCommand
        {
            get;
            set;

        }

        public Command PrevPageCommand
        {
            get;
            set;
        }

        public Command FirstPageCommand
        {
            get;
            set;

        }

        public Command LastPageCommand
        {
            get;
            set;
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
