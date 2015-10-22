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
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using MediaViewer.Model.Utils;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Infrastructure.Global.Events;
using System.IO;

namespace MediaViewer.GeotagFileBrowser
{

    class GeotagFileBrowserViewModel : BindableBase, IMediaFileBrowserContentViewModel
    {       
        public Map Map { get; set; }
        MediaStateCollectionView MediaCollectionView { get; set; }
        IEventAggregator EventAggregator { get; set; }
        MapLayer imageLayer,pinLayer;

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

        bool isMediaPinMode;

        public bool IsMediaPinMode
        {
            get { return isMediaPinMode; }
            set { SetProperty(ref isMediaPinMode, value); }
        }

        bool isMediaThumbnailMode;

        public bool IsMediaThumbnailMode
        {
            get { return isMediaThumbnailMode; }
            set { SetProperty(ref isMediaThumbnailMode, value); }
        }

        public Command MediaThumbnailModeCommand { get; set; }
        public Command MediaPinModeCommand { get; set; }
        public Command ResetMapViewCommand { get; set; }
        public Command MapRoadModeCommand { get; set; }        
        public Command MapAerialModeCommand { get; set; }
        public Command MapViewSelectedCommand { get; set; }
        public Command MapAerialModeWithLabelsCommand { get; set; }

        Brush defaultPushpinBackground, selectedPushpinBackground, defaultImageBackground, selectedImageBackground;

        public GeotagFileBrowserViewModel(MediaStateCollectionView mediaFileState, IEventAggregator eventAggregator)
        {
            Map = new Map();
            Map.CredentialsProvider = new ApplicationIdCredentialsProvider(BingMapsKey.Key);
            Map.CredentialsProvider.GetCredentials((c) =>
            {
                BingMapsKey.SessionKey = c.ApplicationId;
            });

            //Map.PreviewMouseWheel += map_PreviewMouseWheel;
            Map.PreviewMouseDoubleClick += map_PreviewMouseDoubleClick;

            MediaCollectionView = mediaFileState;
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
                    foreach(Pushpin pin in pinLayer.Children) 
                    {                      
                        if (pin.Background.Equals(selectedPushpinBackground))
                        {
                            Map.Center = pin.Location;
                            Map.ZoomLevel = 12;
                            return;
                        }
                    }
                });

            MediaThumbnailModeCommand = new Command(() =>
                {
                    imageLayer.Visibility = Visibility.Visible;
                    pinLayer.Visibility = Visibility.Collapsed;
                    IsMediaThumbnailMode = true;
                    IsMediaPinMode = false;

                });

            MediaPinModeCommand = new Command(() =>
                {
                    imageLayer.Visibility = Visibility.Collapsed;
                    pinLayer.Visibility = Visibility.Visible;

                    IsMediaThumbnailMode = false;
                    IsMediaPinMode = true;
                });

            Pushpin dummy = new Pushpin();

            defaultPushpinBackground = dummy.Background;
            selectedPushpinBackground = new SolidColorBrush(Colors.Red);

            defaultImageBackground = new SolidColorBrush(Colors.Black);
            selectedImageBackground = new SolidColorBrush(Colors.Red);

            imageLayer = new MapLayer();
            pinLayer = new MapLayer();

            Map.Children.Add(imageLayer);
            Map.Children.Add(pinLayer);

            MediaPinModeCommand.Execute();
            
        }

        private void map_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
               
                Point mousePosition = e.GetPosition(Map);
                //Convert the mouse coordinates to a locatoin on the map
                Location mouseLocation = Map.ViewportPointToLocation(mousePosition);
                

                Map.Center = mouseLocation;
            }

            e.Handled = true;
            Map.ZoomLevel = MiscUtils.clamp<double>(Map.ZoomLevel + e.Delta / 500.0, 1, 20);
            
        }

        private void map_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //e.Handled = true;
        }

        bool mapContainsItem(MediaItem item)
        {
            foreach (FrameworkElement elem in pinLayer.Children)
            {
                if (elem.Tag.Equals(item))
                {
                    return(true);
                }
            }

            return (false); 
        }

        void mapAddItem(SelectableMediaItem media)
        {
            MediaItem item = media.Item;

            item.EnterReadLock();
            try
            {
                if (item.Metadata == null || item.HasGeoTag == false || mapContainsItem(item)) return;
                GeoTagCoordinatePair coord = new GeoTagCoordinatePair(item.Metadata.Latitude, item.Metadata.Longitude);
                       
                if (!coord.IsEmpty)
                {
                    Location itemLocation = new Location(coord.LatDecimal.Value, coord.LonDecimal.Value);
                  
                    Border border = new Border();
                    border.BorderThickness = new Thickness(8);                      
                    border.Tag = item;
                    border.MaxWidth = 100;
                    border.MaxHeight = 100;
                    if (media.IsSelected)
                    {
                        border.Background = selectedImageBackground;
                    }
                    else
                    {
                        border.Background = defaultImageBackground;
                    }
                       
                    Image image = new Image();                      
                    image.Tag = item;
                    image.Source = item.Metadata.Thumbnails.ElementAt(0).Image;                    
                    image.PreviewMouseDown += item_PreviewMouseDown;
                    image.ToolTip = item.Metadata.DefaultFormatCaption;

                    border.Child = image;

                    imageLayer.AddChild(border, itemLocation, PositionOrigin.Center);
                  
                    Pushpin pin = new Pushpin();
                    pin.Tag = item;                  
                    pin.ToolTip = item.Metadata.DefaultFormatCaption;
                    pin.PreviewMouseDown += item_PreviewMouseDown;
                    pin.Location = itemLocation;

                    if (media.IsSelected)
                    {
                        pin.Background = selectedPushpinBackground;                        
                    }
                    else
                    {
                        pin.Background = defaultPushpinBackground;
                    }

                    pinLayer.AddChild(pin,itemLocation,PositionOrigin.BottomCenter);
                    
                }
            }
            finally
            {
                item.ExitReadLock();
            }
              
        }

        void mapRemoveItem(SelectableMediaItem media)
        {
            for (int i = 0; i < pinLayer.Children.Count; i++)
            {
                if ((pinLayer.Children[i] as FrameworkElement).Tag.Equals(media.Item))
                {
                    pinLayer.Children.RemoveAt(i);
                    imageLayer.Children.RemoveAt(i);

                    return;
                }
            }
            
        }

        void mapDeselectAll()
        {
            for (int i = 0; i < pinLayer.Children.Count; i++)
            {                               
                (pinLayer.Children[i] as Pushpin).Background = defaultPushpinBackground;
                (imageLayer.Children[i] as Border).Background = defaultImageBackground;               
            }

        }

        FrameworkElement mapSelectItem(MediaItem item)
        {
            FrameworkElement selected = null;
           
            for(int i = 0; i < pinLayer.Children.Count; i++)
            {
                if ((pinLayer.Children[i] as FrameworkElement).Tag.Equals(item))
                {
                    (pinLayer.Children[i] as Pushpin).Background = selectedPushpinBackground;
                    (imageLayer.Children[i] as Border).Background = selectedImageBackground;

                    selected = pinLayer.Children[i] as FrameworkElement;
                }                
            }

            EventAggregator.GetEvent<TitleChangedEvent>().Publish(Path.GetFileName(item.Location));

            return (selected);
        }
       

        void mapLoadItems()
        {
            MediaCollectionView.MediaState.UIMediaCollection.EnterReadLock();
            try
            {
                foreach (SelectableMediaItem media in MediaCollectionView.Media)
                {
                    if (media.Item.ItemState == MediaItemState.LOADED)
                    {                       
                        mapAddItem(media);                    
                    }
                }
            }
            finally
            {
                MediaCollectionView.MediaState.UIMediaCollection.ExitReadLock();
            }
        }

        void mapClearItems()
        {
            pinLayer.Children.Clear();
            imageLayer.Children.Clear();
        }

        void item_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                FrameworkElement elem = (FrameworkElement)sender;
                MediaItem item = elem.Tag as MediaItem;

                mapDeselectAll();
                mapSelectItem(item);
                EventAggregator.GetEvent<MediaSelectionEvent>().Publish(new MediaSelectionPayload(MediaCollectionView.Guid, item));

                e.Handled = true;
            }
        }

        public void OnNavigatedTo(Microsoft.Practices.Prism.Regions.NavigationContext navigationContext)
        {
            mapLoadItems();
            EventAggregator.GetEvent<MediaSelectionEvent>().Subscribe(mediaSelectionEvent, ThreadOption.PublisherThread, false, selection => selection.SenderId.Equals(MediaCollectionView.Guid));
            MediaCollectionView.ItemPropertyChanged += MediaCollectionView_ItemPropertyChanged;
            MediaCollectionView.NrItemsInStateChanged += MediaCollectionView_NrItemsInStateChanged;
        }

        private void MediaCollectionView_NrItemsInStateChanged(object sender, MediaStateCollectionViewChangedEventArgs e)
        {
            switch (e.Action)
            {
                case MediaViewer.Model.Media.State.MediaStateChangedAction.Add:
                    foreach (SelectableMediaItem item in e.NewItems)
                    {
                        App.Current.Dispatcher.BeginInvoke(new Action(() => mapAddItem(item)));
                    }
                    break;
                case MediaViewer.Model.Media.State.MediaStateChangedAction.Remove:
                    foreach (SelectableMediaItem item in e.OldItems)
                    {
                        App.Current.Dispatcher.BeginInvoke(new Action(() => mapRemoveItem(item)));
                    }
                    break;
                case MediaViewer.Model.Media.State.MediaStateChangedAction.Clear:
                    App.Current.Dispatcher.BeginInvoke(new Action(() => mapClearItems()));                                         
                    break;
                case MediaViewer.Model.Media.State.MediaStateChangedAction.Modified:
                    break;
                case MediaViewer.Model.Media.State.MediaStateChangedAction.Replace:
                    break;
                default:
                    break;
            }
        }

        private void MediaCollectionView_ItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SelectableMediaItem item = (SelectableMediaItem)sender;

            if (e.PropertyName.Equals("Metadata"))
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() => {
                    mapRemoveItem(item);
                    mapAddItem(item);
                }));

            }
       
        }

        public void OnNavigatedFrom(Microsoft.Practices.Prism.Regions.NavigationContext navigationContext)
        {
            EventAggregator.GetEvent<MediaSelectionEvent>().Unsubscribe(mediaSelectionEvent);
            MediaCollectionView.ItemPropertyChanged -= MediaCollectionView_ItemPropertyChanged;
            MediaCollectionView.NrItemsInStateChanged -= MediaCollectionView_NrItemsInStateChanged;
            mapClearItems();
        }

        

        void mediaSelectionEvent(MediaSelectionPayload selection)
        {
            mapDeselectAll();

            if (selection.Items.Count() == 0) return;           

            for(int i = 0; i < selection.Items.Count(); i++)
            {
                MediaItem item = selection.Items.ElementAt(i);

                FrameworkElement elem = mapSelectItem(item);

                if (elem != null && i == selection.Items.Count() - 1)
                {
                    item.EnterReadLock();
                    if (item.Metadata.Latitude != null)
                    {
                        Map.Center = new Location(item.Metadata.Latitude.Value, item.Metadata.Longitude.Value);
                    }
                    item.ExitReadLock();

                }
            }
        }
    }
}
