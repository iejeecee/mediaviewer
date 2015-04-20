using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maps.MapControl.WPF;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.Metadata;
using MediaViewer.Model.Media.Base;
using MediaViewer.MediaFileBrowser;
using MediaViewer.UserControls.GeoTagEditor;
using Microsoft.Practices.Prism.PubSubEvents;
using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Mvvm;
using System.Windows.Threading;
using System.Windows.Media;

namespace MediaViewer.GeotagFileBrowser
{

    class GeotagFileBrowserViewModel : BindableBase, IMediaFileBrowserContentViewModel
    {       
        public Map Map { get; set; }
        MediaFileState MediaFileState {get;set;}
        IEventAggregator EventAggregator { get; set; }

        bool isRoadMode;

        public bool IsRoadMode
        {
            get { return isRoadMode; }
            set { SetProperty(ref isRoadMode, value); }
        }
        bool isAerialMode;

        public bool IsAerialMode
        {
            get { return isAerialMode; }
            set { SetProperty(ref isAerialMode, value); }
        }
        bool isAerialModeWithLabels;

        public bool IsAerialModeWithLabels
        {
            get { return isAerialModeWithLabels; }
            set {
                SetProperty(ref isAerialModeWithLabels, value);               
            }
        }

        public Command ResetMapViewCommand { get; set; }
        public Command MapRoadModeCommand { get; set; }        
        public Command MapAerialModeCommand { get; set; }
        public Command MapViewSelectedCommand { get; set; }
        public Command MapAerialModeWithLabelsCommand { get; set; }

        Brush defaultPushpinBackground;
        Brush selectedPushpinBackground;

        public GeotagFileBrowserViewModel(MediaFileState mediaFileState, IEventAggregator eventAggregator)
        {
            Map = new Map();
            Map.CredentialsProvider = new ApplicationIdCredentialsProvider(BingMapsKey.key);
            Map.PreviewMouseDoubleClick += Map_PreviewMouseDoubleClick;
                        
            MediaFileState = mediaFileState;
            EventAggregator = eventAggregator;

            IsRoadMode = true;

            MapRoadModeCommand = new Command(() =>
            {
                Map.Mode = new RoadMode();
                IsRoadMode = true;
                IsAerialMode = false;
                IsAerialModeWithLabels = false;
            });

            MapAerialModeCommand = new Command(() =>
            {
                Map.Mode = new AerialMode(false);
                IsRoadMode = false;
                IsAerialMode = true;
                IsAerialModeWithLabels = false;
            });

            MapAerialModeWithLabelsCommand = new Command(() =>
            {
                Map.Mode = new AerialMode(true);
                IsRoadMode = false;
                IsAerialMode = false;
                IsAerialModeWithLabels = true;
            });

            ResetMapViewCommand = new Command(() =>
                {
                    Map.ZoomLevel = 1;
                    Map.Center = new Location((Location.MaxLatitude + Location.MinLatitude) / 2, (Location.MaxLongitude + Location.MinLongitude) / 2);
                });

            MapViewSelectedCommand = new Command(() =>
                {
                    foreach(Pushpin pin in Map.Children) 
                    {
                        if(pin.Background.Equals(selectedPushpinBackground)) {

                            Map.Center = pin.Location;
                            Map.ZoomLevel = 12;
                            return;
                        }
                    }
                });

            Pushpin dummy = new Pushpin();

            defaultPushpinBackground = dummy.Background;
            selectedPushpinBackground = new SolidColorBrush(Colors.Red);

        }

        private void Map_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        bool mapHasPushpin(MediaItem item)
        {
            foreach (Pushpin pin in Map.Children)
            {
                if ((pin.Tag as MediaItem).Equals(item))
                {
                    return (true);
                }
            }

            return (false);
        }

        void addPushpin(MediaItem item)
        {                       
            item.RWLock.EnterReadLock();
            try
            {
                if (item.Metadata == null || mapHasPushpin(item)) return;
                GeoTagCoordinatePair coord = new GeoTagCoordinatePair(item.Metadata.Latitude, item.Metadata.Longitude);
                       
                if (!coord.IsEmpty)
                {
                    Pushpin pin = new Pushpin();
                    pin.Tag = item;
                    pin.PreviewMouseDoubleClick += pin_PreviewMouseDoubleClick;
                    pin.Location = new Location(coord.LatDecimal.Value, coord.LonDecimal.Value);

                    Map.Children.Add(pin);

                }
            }
            finally
            {
                item.RWLock.ExitReadLock();
            }
              
        }

        void removePushpin(MediaItem item)
        {
            foreach (Pushpin pin in Map.Children)
            {
                if ((pin.Tag as MediaItem).Equals(item))
                {                    
                    Map.Children.Remove(pin);                      
                    return;
                }
            }
        }

        void loadItems()
        {
            MediaFileState.UIMediaCollection.EnterReaderLock();
            try
            {
                foreach (MediaFileItem item in MediaFileState.UIMediaCollection)
                {
                    if (item.ItemState == MediaItemState.LOADED)
                    {
                        item.RWLock.EnterReadLock();
                        addPushpin(item);
                        item.RWLock.ExitReadLock();
                    }
                }
            }
            finally
            {
                MediaFileState.UIMediaCollection.ExitReaderLock();
            }
        }

        void pin_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Pushpin pin = (Pushpin)sender;
            MediaItem item = pin.Tag as MediaItem;

            selectPin(item);
            EventAggregator.GetEvent<MediaSelectionEvent>().Publish(item);
        }

        public void OnNavigatedTo(Microsoft.Practices.Prism.Regions.NavigationContext navigationContext)
        {
            loadItems();
            EventAggregator.GetEvent<MediaSelectionEvent>().Subscribe(mediaSelectionEvent);
            MediaFileState.ItemPropertiesChanged += MediaFileState_ItemPropertiesChanged;
            MediaFileState.NrItemsInStateChanged += MediaFileState_NrItemsInStateChanged;
        }

        private void MediaFileState_NrItemsInStateChanged(object sender, Model.Media.State.MediaStateChangedEventArgs e)
        {
            switch (e.Action)
            {
                case MediaViewer.Model.Media.State.MediaStateChangedAction.Add:
                    foreach (MediaItem item in e.NewItems)
                    {
                        App.Current.Dispatcher.Invoke(new Action(() => addPushpin(item)));
                    }
                    break;
                case MediaViewer.Model.Media.State.MediaStateChangedAction.Remove:
                    foreach (MediaItem item in e.NewItems)
                    {
                        App.Current.Dispatcher.Invoke(new Action(() => removePushpin(item)));
                    }
                    break;
                case MediaViewer.Model.Media.State.MediaStateChangedAction.Clear:
                    App.Current.Dispatcher.Invoke(new Action(() => Map.Children.Clear()));
                    break;
                case MediaViewer.Model.Media.State.MediaStateChangedAction.Modified:
                    break;
                case MediaViewer.Model.Media.State.MediaStateChangedAction.Replace:
                    break;
                default:
                    break;
            }
        }

        private void MediaFileState_ItemPropertiesChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MediaFileItem item = (MediaFileItem)sender;
           
            if (e.PropertyName.Equals("HasGeoTag"))
            {               
                if (item.HasGeoTag)
                {
                    App.Current.Dispatcher.Invoke(new Action(() => addPushpin(item)));
                }
                else
                {
                    App.Current.Dispatcher.Invoke(new Action(() => removePushpin(item)));
                }               
            }
       
        }

        public void OnNavigatedFrom(Microsoft.Practices.Prism.Regions.NavigationContext navigationContext)
        {
            EventAggregator.GetEvent<MediaSelectionEvent>().Unsubscribe(mediaSelectionEvent);
            MediaFileState.ItemPropertiesChanged -= MediaFileState_ItemPropertiesChanged;
            MediaFileState.NrItemsInStateChanged -= MediaFileState_NrItemsInStateChanged;
            Map.Children.Clear();
        }

        Pushpin selectPin(MediaItem item)
        {
            Pushpin selected = null;

            foreach (Pushpin pin in Map.Children)
            {
                if ((pin.Tag as MediaItem).Equals(item))
                {
                    pin.Background = selectedPushpinBackground;
                    selected = pin;
                }
                else
                {
                    pin.Background = defaultPushpinBackground;
                }

            }

            return (selected);
        }

        void mediaSelectionEvent(MediaItem item)
        {
            Pushpin pin = selectPin(item);

            if (pin != null)
            {
                Map.Center = pin.Location;
            }
            
        }
    }
}
