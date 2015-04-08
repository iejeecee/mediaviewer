using MediaViewer.Model.Media.Metadata;
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
        Pushpin Pin { get; set; }
        String SessionKey { get; set; }

        /*static GeoTagEditorView()
        {
            UserControl.IsEnabledProperty.OverrideMetadata(typeof(GeoTagEditorView), new UIPropertyMetadata(true, isEnabledChangedCallback));
        }

        private static void isEnabledChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GeoTagEditorView view = (GeoTagEditorView)d;
            view.toggleMapButton.IsChecked = false;
        }*/

        public GeoTagEditorView()
        {
            InitializeComponent();
            
            map.CredentialsProvider = new ApplicationIdCredentialsProvider(BingMapsKey.key);
            map.PreviewMouseDoubleClick += map_PreviewMouseDoubleClick;
            map.PreviewMouseWheel += map_PreviewMouseWheel;

            SessionKey = null;
            
            map.CredentialsProvider.GetCredentials((c) =>
            {
                SessionKey = c.ApplicationId;
            });

            Pin = new Pushpin();
           
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

        void showValues(bool isZoomAndCenterMap = false)
        {
            latTextBox.Text = Coordinate.LatCoord;
            lonTextBox.Text = Coordinate.LonCoord;

            if (Coordinate.LatCoord == null)
            {               
                map.Children.Remove(Pin);
                map.ZoomLevel = 1;
                return;
            }
         
            Pin.Location = new Location(Coordinate.LatDecimal.Value, Coordinate.LonDecimal.Value);  
            
            if (isZoomAndCenterMap)
            {
                map.Center = Pin.Location;
                map.ZoomLevel = 12;
            }
                         
            if (!map.Children.Contains(Pin))
            {
                map.Children.Add(Pin);
            }
                        
        }

        private void toggleMapButton_Checked(object sender, RoutedEventArgs e)
        {
            mapGrid.Visibility = System.Windows.Visibility.Visible;
        }

        private void toggleMapButton_Unchecked(object sender, RoutedEventArgs e)
        {
            mapGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void clearMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Coordinate.set(null, null);
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
           
        }

        private void map_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            map.ZoomLevel += e.Delta / 500.0;
          
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
                return BingMapsService.findLocations(location, usermapView, SessionKey);                
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
                MessageBox.Show("Error finding location\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
