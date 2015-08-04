using MediaViewer.Model.Collections.Sort;
using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace MediaViewer.UserControls.LocationBox
{
    /// <summary>
    /// Interaction logic for LocationBoxView.xaml
    /// </summary>
    public partial class LocationBoxView : UserControl
    {
        PopupViewModel popupViewModel;
        LocationBoxViewModel locationBoxViewModel;
 
        public LocationBoxView()
        {
            InitializeComponent();

            popupViewModel = new PopupViewModel();
            popupViewModel.LocationSelected += popupViewModel_LocationSelected;
            popupViewModel.LocationRemoved += popupViewModel_LocationRemoved;

            popup.DataContext = popupViewModel;

            locationBoxViewModel = new LocationBoxViewModel();
            locationBoxViewModel.LocationSelected += locationBoxViewModel_LocationSelected;

            locationListBox.DataContext = locationBoxViewModel;
       
            Index =  0;
        
        }
                              
        int index;

        int Index
        {
            get
            {
                return (index);
            }
            set
            {
                index = value;

                if (index == 0)
                {
                    forwardButton.IsEnabled = false;
                }
                else
                {
                    forwardButton.IsEnabled = true;
                }

                if (LocationHistory == null || index == LocationHistory.Count() - 1)
                {
                    backButton.IsEnabled = false;
                }
                else
                {
                    backButton.IsEnabled = true;
                }
            }
        }

        public String Location
        {
            get { return (String)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Location.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register("Location", typeof(String), typeof(LocationBoxView), new PropertyMetadata("",locationChanged));

        private static void locationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LocationBoxView view = (LocationBoxView)d;
            
            if (view.LocationHistory == null)
            {
                view.locationBoxViewModel.setPath((String)e.NewValue);                
                return;
            }
            else if (view.IsUpdateHistory && view.LocationHistory.Count() == 0)
            {
                MiscUtils.insertIntoHistoryCollection(view.LocationHistory, (String)e.NewValue);
            }
            else if (view.IsUpdateHistory && !view.LocationHistory[view.index].Equals(e.NewValue))
            {
                for (int i = 0; i < view.index; i++)
                {
                    view.LocationHistory.RemoveAt(0);
                }

                MiscUtils.insertIntoHistoryCollection(view.LocationHistory, (String)e.NewValue);
                view.Index = 0;
            }

            view.locationBoxViewModel.setPath((String)e.NewValue);            
        }

        public bool IsUpdateHistory
        {
            get { return (bool)GetValue(IsUpdateHistoryProperty); }
            set { SetValue(IsUpdateHistoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsUpdateHistory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsUpdateHistoryProperty =
            DependencyProperty.Register("IsUpdateHistory", typeof(bool), typeof(LocationBoxView), new PropertyMetadata(true));

        
        public ObservableCollection<String> LocationHistory
        {
            get { return (ObservableCollection<String>)GetValue(LocationHistoryProperty); }
            set { SetValue(LocationHistoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LocationHistory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LocationHistoryProperty =
            DependencyProperty.Register("LocationHistory", typeof(ObservableCollection<String>), typeof(LocationBoxView), new PropertyMetadata(null, locationHistoryChanged));

        private static void locationHistoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LocationBoxView view = (LocationBoxView)d;
            if (e.NewValue != null && view.IsUpdateHistory)
            {
                MiscUtils.insertIntoHistoryCollection((ObservableCollection<String>)e.NewValue, view.Location);
            }
        }

        public ObservableCollection<String> FavoriteLocations
        {
            get { return (ObservableCollection<String>)GetValue(FavoriteLocationsProperty); }
            set { SetValue(FavoriteLocationsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FavoriteLocations.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FavoriteLocationsProperty =
            DependencyProperty.Register("FavoriteLocations", typeof(ObservableCollection<String>), typeof(LocationBoxView), new PropertyMetadata(null));
        
      
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (Index >= LocationHistory.Count - 1) return;

            Location = LocationHistory[++Index];

        }

        private void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (Index <= 0) return;

            Location = LocationHistory[--Index];
        }
      
        private void historyButton_Click(object sender, RoutedEventArgs e)
        {
            popup.Tag = "historyPopup";
            showPopup(LocationHistory, false);
        }

        private void favoritesButton_Click(object sender, RoutedEventArgs e)
        {
            popup.Tag = "favoritesPopup";
            showPopup(FavoriteLocations, true);
        }

        void showPopup(ObservableCollection<String> locations, bool isRemovable)
        {
            popupViewModel.setLocations(locations, isRemovable);

            popup.DataContext = popupViewModel;

            popup.PlacementTarget = locationListBox;
            popup.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            popup.MinWidth = locationListBox.ActualWidth;
            popup.IsEnabled = true;
            popup.IsOpen = true;
            scrollViewer.ScrollToVerticalOffset(0);     
            
        }
   
        void hidePopup()
        {          
            popup.IsOpen = false;         
        }

        private void addFavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(Location) || String.IsNullOrWhiteSpace(Location))
            {
                return;
            }

            if (!FavoriteLocations.Contains(Location))
            {
                CollectionsSort.insertIntoSortedCollection(FavoriteLocations, Location);          
            }
        }

        private void popupViewModel_LocationSelected(object sender, PopupLocationItem location)
        {
            if(((String)popup.Tag).Equals("historyPopup")) {

                Index =  popupViewModel.getIndexOfSelectedLocation();
            }

            Location = location.Name;
            hidePopup();
        }
        
        private void directoryButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DirectoryItem item = (DirectoryItem)button.Tag;
            Location = item.FullPath;
        }

        private void locationBoxViewModel_LocationSelected(object sender, DirectoryItem e)
        {
            Location = e.FullPath;
        }

        private void popupViewModel_LocationRemoved(object sender, PopupLocationItem e)
        {
            FavoriteLocations.Remove(e.Name);
        }

        private void location_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;

            DirectoryItem item = textBlock.Tag as DirectoryItem;

            if (item.SubDirectories.Count > 0)
            {
                Location = item.FullPath;
                SystemSounds.Question.Play();
            }
        }
             
    }
}
