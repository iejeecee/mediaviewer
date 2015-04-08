using MediaViewer;
using MediaViewer.Infrastructure;
using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.State.CollectionView;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GeoTagPlugin
{
    /// <summary>
    /// Interaction logic for GoogleEarthGeoTagNavigationItem.xaml
    /// </summary>
    [Export]
    public partial class GeoTagNavigationItemView : UserControl
    {
        [Import]
        public IRegionManager RegionManager;

        IEventAggregator EventAggregator { get; set; }
        ICollection<MediaFileItem> SelectedItems { get; set; }

        [ImportingConstructor]
        public GeoTagNavigationItemView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            EventAggregator = eventAggregator;

            SelectedItems = new List<MediaFileItem>();
         
            EventAggregator.GetEvent<MediaViewer.Model.Global.Events.MediaBatchSelectionEvent>().Subscribe(mediaBatchSelectionEvent);
            EventAggregator.GetEvent<MediaViewer.Model.Global.Events.MediaSelectionEvent>().Subscribe(mediaSelectionEvent);
        }

        private void mediaBatchSelectionEvent(ICollection<MediaItem> selectedItems)
        {
            List<MediaFileItem> items = new List<MediaFileItem>();

            foreach(MediaItem item in selectedItems) {

                items.Add(item as MediaFileItem);
            }

            SelectedItems = items;
        }

        private void mediaSelectionEvent(MediaItem selectedItem)
        {
            SelectedItems = new List<MediaFileItem>();
            SelectedItems.Add(selectedItem as MediaFileItem);
        }

        private void navigationButton_Click(object sender, RoutedEventArgs e)
        {
            Uri googleEarthGeoTagViewUri = new Uri(typeof(GeoTagView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();
            navigationParams.Add("selectedItems", SelectedItems);

            RegionManager.RequestNavigate(RegionNames.MediaFileBrowserContentRegion, googleEarthGeoTagViewUri, navigationParams);
        
        }
    }
}
