using MediaViewer.Model.Media.Metadata;
using MediaViewer.Model.Utils;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace MediaViewer.UserControls.GeoTagEditor
{
    /// <summary>
    /// Interaction logic for GeoTagEditorView.xaml
    /// </summary>
    public partial class GeoTagEditorView : UserControl
    {
        static string geoTagClipboardFormat = "geoTagCoordinatePair";

        bool IsToolTipLocationSet { get; set; }
        Pushpin Pin { get; set; }
      
        static GeoTagEditorView()
        {
            UserControl.IsEnabledProperty.OverrideMetadata(typeof(GeoTagEditorView), new UIPropertyMetadata(true, isEnabledChangedCallback));
        }

        private static void isEnabledChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GeoTagEditorView view = (GeoTagEditorView)d;
            if ((bool)e.NewValue == false)
            {
                view.toggleMapButton.IsChecked = false;
            }
        }

        public GeoTagEditorView()
        {
            InitializeComponent();
            
            map.CredentialsProvider = new ApplicationIdCredentialsProvider(BingMapsKey.Key);
            map.PreviewMouseDoubleClick += map_PreviewMouseDoubleClick;
            map.PreviewMouseWheel += map_PreviewMouseWheel;
            
            map.CredentialsProvider.GetCredentials((c) =>
            {
                BingMapsKey.SessionKey = c.ApplicationId;
            });

            Pin = new Pushpin();
            Pin.ToolTip = "Finding Location...";
            Pin.ToolTipOpening += Pin_ToolTipOpening;

            IsToolTipLocationSet = false;
        }

        async void Pin_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (IsToolTipLocationSet) return;

            Pin.ToolTip = "Finding Location...";
            Location location = new Location(Coordinate.LatDecimal.Value, Coordinate.LonDecimal.Value);

            Task<LocationResult> t = Task.Factory.StartNew<LocationResult>(() =>
            {
                return BingMapsService.findLocation(location, BingMapsKey.SessionKey);
            });

            try
            {
                await t;

                if (t.Result != null)
                {
                    Pin.ToolTip = t.Result.Name;
                    IsToolTipLocationSet = true;
                }
                else
                {
                    Pin.ToolTip = "Unknown location";
                    IsToolTipLocationSet = false;
                }

            }
            catch (Exception ex)
            {
                Pin.ToolTip = "Error: " + ex.Message;
                IsToolTipLocationSet = false;
            }
        }
       
        public GeoTagCoordinatePair Coordinate
        {
            get { return (GeoTagCoordinatePair)GetValue(CoordinateProperty); }
            set { SetValue(CoordinateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Coordinate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CoordinateProperty =
            DependencyProperty.Register("Coordinate", typeof(GeoTagCoordinatePair), typeof(GeoTagEditorView), new PropertyMetadata(null, coordinateChangedCallback));

        private static void coordinateChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GeoTagEditorView view = (GeoTagEditorView)d;

            if (e.OldValue != null)
            {
                GeoTagCoordinatePair coord = (GeoTagCoordinatePair)e.OldValue;

                WeakEventManager<GeoTagCoordinatePair, EventArgs>.RemoveHandler(coord, "GeoTagChanged", view.coordinateChanged);
            }

            if (e.NewValue != null)
            {
                GeoTagCoordinatePair coord = (GeoTagCoordinatePair)e.NewValue;

                view.showValues(true);
                WeakEventManager<GeoTagCoordinatePair, EventArgs>.AddHandler(coord, "GeoTagChanged", view.coordinateChanged);

                view.IsToolTipLocationSet = false;
            }
            
        }

        private void coordinateChanged(object sender, EventArgs e)
        {
            showValues();
        }

        public bool IsDecimal
        {
            get { return (bool)GetValue(IsDecimalProperty); }
            set { SetValue(IsDecimalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDecimal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDecimalProperty =
            DependencyProperty.Register("IsDecimal", typeof(bool), typeof(GeoTagEditorView), new PropertyMetadata(false,isDecimalChanged));

        private static void isDecimalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GeoTagEditorView view = (GeoTagEditorView)d;
            view.showValues();
        }

        void showValues(bool isNewValue = false)
        {
            latTextBox.Text = Coordinate.LatCoord;
            lonTextBox.Text = Coordinate.LonCoord;

            if (mapGrid.Visibility != System.Windows.Visibility.Visible)
            {
                // don't do unneccesary requests
                return;
            }

            if (Coordinate.LatCoord == null)
            {             
                map.Children.Remove(Pin);
                map.ZoomLevel = 1;
                findLocationComboBox.ItemsSource = null;
                return;
            }
                  
            Pin.Location = new Location(Coordinate.LatDecimal.Value, Coordinate.LonDecimal.Value);  
            
            if (isNewValue)
            {
                map.Center = Pin.Location;
                map.ZoomLevel = 16;
                findLocationComboBox.ItemsSource = null;
            }
                         
            if (!map.Children.Contains(Pin))
            {
                map.Children.Add(Pin);
            }
                                    
        }

        private void toggleMapButton_Checked(object sender, RoutedEventArgs e)
        {
            mapGrid.Visibility = System.Windows.Visibility.Visible;
            showValues(true);
        }

        private void toggleMapButton_Unchecked(object sender, RoutedEventArgs e)
        {
            mapGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void geoTagContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ContextMenu contextMenu = (ContextMenu)Resources["geoTagContextMenu"];
                                  
            ((MenuItem)contextMenu.Items[0]).IsEnabled = !Coordinate.IsEmpty;
            ((MenuItem)contextMenu.Items[2]).IsEnabled = !Coordinate.IsEmpty;                       
            ((MenuItem)contextMenu.Items[3]).IsEnabled = Clipboard.ContainsData(geoTagClipboardFormat);
            
        }

        private void clearMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Coordinate.clear();
        }

        private void copyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!Coordinate.IsEmpty)
            {
                String text = Coordinate.LatCoord + " " + Coordinate.LonCoord;
                Clipboard.SetData(geoTagClipboardFormat, text);  
            }        
        }

        private void pasteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            String coord = Clipboard.GetData(geoTagClipboardFormat) as String;

            if (coord != null)
            {
                char[] seperator = new char[]{' '};

                string[] values = coord.Split(seperator);

                Coordinate.set(values[0], values[1]);
            }
            
        }

        private void map_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Disables the default mouse double-click action.
            e.Handled = true;

            // Determin the location to place the pushpin at on the map.

            //Get the mouse click coordinates
            Point mousePosition = e.GetPosition(map);
            //Convert the mouse coordinates to a locatoin on the map
            Location pinLocation = map.ViewportPointToLocation(mousePosition);

            Coordinate.set(pinLocation.Latitude, pinLocation.Longitude);

            IsToolTipLocationSet = false;
        }

        private void map_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            map.ZoomLevel = MiscUtils.clamp<double>(map.ZoomLevel + e.Delta / 500.0, 1, 20);           
        }

        private async void findLocationComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await findLocationAsync();
            }                     
        }        

        private async void findLocationButton_Click(object sender, RoutedEventArgs e)
        {
            await findLocationAsync();
        }

        async Task findLocationAsync()
        {            
            String location = findLocationComboBox.Text;
            LocationRect usermapView = map.BoundingRectangle;

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

        private void centerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            map.Center = Pin.Location;
            map.ZoomLevel = 16;
        }

        private void map_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {           
           centerMenuItem.IsEnabled = !Coordinate.IsEmpty;            
        }
        

        
    }
}
