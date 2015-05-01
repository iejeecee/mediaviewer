using MediaViewer.Model.Utils;
using MediaViewer.UserControls.GeoTagEditor;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace MediaViewer.UserControls.GeoLocationPicker
{
    /// <summary>
    /// Interaction logic for GeoLocationPickerView.xaml
    /// </summary>
    public partial class GeoLocationPickerView : UserControl
    {
    
        LocationRect ResetView { get; set; }

        public LocationRect GeoLocationRect
        {
            get { return (LocationRect)GetValue(GeoLocationRectProperty); }
            set { SetValue(GeoLocationRectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GeoLocationRect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GeoLocationRectProperty =
            DependencyProperty.Register("GeoLocationRect", typeof(LocationRect), typeof(GeoLocationPickerView), new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,geoLocationRectChanged));

        private static void geoLocationRectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
        }

        public GeoLocationPickerView()
        {
            InitializeComponent();        
            map.CredentialsProvider = new ApplicationIdCredentialsProvider(BingMapsKey.Key);
            map.PreviewMouseWheel += map_PreviewMouseWheel;
                     
            map.CredentialsProvider.GetCredentials((c) =>
            {              
                BingMapsKey.SessionKey= c.ApplicationId;
            });

            ResetView = new LocationRect(Location.MaxLatitude,Location.MinLongitude,Location.MinLatitude,Location.MaxLongitude);
        }
      
        private void toggleMapButton_Checked(object sender, RoutedEventArgs e)
        {
            mapGrid.Height = double.NaN;
            map.MinHeight = 200;
            mapGrid.Visibility = System.Windows.Visibility.Visible;                       
        }

        private void toggleMapButton_Unchecked(object sender, RoutedEventArgs e)
        {
            mapGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private async void findLocationComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {              
                await findLocationAsync(findLocationComboBox.Text);
            }
        }

        private async void findLocationButton_Click(object sender, RoutedEventArgs e)
        {
            await findLocationAsync(findLocationComboBox.Text);
        }

        void clear()
        {
            if (GeoLocationRect != null)
            {
                findLocationComboBox.ItemsSource = new List<LocationResult>();
                GeoLocationRect = null;
                map.SetView(ResetView);
                selectedLocationTextBox.Text = null;
            }
        }

        async Task findLocationAsync(String location)
        {
            if (String.IsNullOrEmpty(location) || String.IsNullOrWhiteSpace(location))
            {
                clear();
                return;
            }

            LocationRect usermapView = null;

            if (GeoLocationRect != null)
            {
                usermapView = map.BoundingRectangle;
            }

            Task<List<LocationResult>> t = Task.Factory.StartNew<List<LocationResult>>(() =>
            {            
                return BingMapsService.findLocations(location, BingMapsKey.SessionKey, usermapView);
            });

            try
            {
                findLocationButton.IsEnabled = false;
                findLocationComboBox.IsEnabled = false;

                await t;             

                if (t.Result.Count > 0)
                {
                    findLocationComboBox.ItemsSource = t.Result;
                    findLocationComboBox.SelectedIndex = 0;
                }
                else
                {
                    findLocationComboBox.ItemsSource = null;
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error searching location\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                findLocationButton.IsEnabled = true;
                findLocationComboBox.IsEnabled = true;
            }

        }

        private void findLocationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LocationResult location = (LocationResult)findLocationComboBox.SelectedItem;

            if (location == null) return;

            map.SetView(location.BoundingBox);
            GeoLocationRect = location.BoundingBox;
            selectedLocationTextBox.Text = location.Name;
        }

        private void mapRoadMode_Click(object sender, RoutedEventArgs e)
        {
            map.Mode = new RoadMode();
            mapAerialWithLabelsMode.IsChecked = false;
            mapAerialMode.IsChecked = false;
        }

        private void mapAerialMode_Click(object sender, RoutedEventArgs e)
        {
            map.Mode = new AerialMode();
            mapRoadMode.IsChecked = false;
            mapAerialWithLabelsMode.IsChecked = false;
        }

        private void mapAerialWithLabelsMode_Click(object sender, RoutedEventArgs e)
        {
            map.Mode = new AerialMode(true);
            mapRoadMode.IsChecked = false;
            mapAerialMode.IsChecked = false;
        }

        private void clearMenuItem_Click(object sender, RoutedEventArgs e)
        {
            clear();
        }

        private void selectedLocationTextBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (GeoLocationRect == null)
            {
                clearMenuItem.IsEnabled = false;
            }
            else
            {
                clearMenuItem.IsEnabled = true;
            }
        }

        private void map_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            map.ZoomLevel = MiscUtils.clamp<double>(map.ZoomLevel + e.Delta / 500.0, 1, 20);
        }
    }
}
