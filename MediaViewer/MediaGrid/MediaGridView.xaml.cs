// intresting stuff:
// Lazy<T> for lazy initialization
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.UserControls.Layout;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Regions;
using System.ComponentModel.Composition;
using MediaViewer.MediaFileBrowser;
using MediaViewer.Model.Media.State.CollectionView;
using System.Windows.Controls.Primitives;
using Microsoft.Practices.Prism.PubSubEvents;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Media.Base;

namespace MediaViewer.MediaGrid
{
    /// <summary>
    /// Interaction logic for ImageGridControl.xaml
    /// </summary>
    [Export]
    public partial class MediaGridView : UserControl, IRegionMemberLifetime, INavigationAware
    {
        VirtualizingTilePanel panel;
              
        MediaGridViewModel ViewModel {get;set;}
 
        public MediaGridView()
        {           
            InitializeComponent();         

            panel = null;           
                       
        }

        void ImageGridView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is MediaGridViewModel)
            {
                MediaGridViewModel oldMediaGridViewModel = e.OldValue as MediaGridViewModel;

                oldMediaGridViewModel.MediaStateCollectionView.Cleared -= imageGridViewModel_Cleared;                                          
            }

            if (e.NewValue is MediaGridViewModel)
            {
                MediaGridViewModel mediaGridViewModel = e.NewValue as MediaGridViewModel;

                mediaGridViewModel.MediaStateCollectionView.Cleared += imageGridViewModel_Cleared;

                ViewModel = mediaGridViewModel;
            }
            else
            {
                ViewModel = null;
            }
        }

        public int NrColumns
        {
            get { return (int)GetValue(NrColumnsProperty); }
            set { SetValue(NrColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NrColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NrColumnsProperty =
            DependencyProperty.Register("NrColumns", typeof(int), typeof(MediaGridView), new PropertyMetadata(4,nrColumnsChangedCallback));

        private static void nrColumnsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
               
        private void imageGridViewModel_PrevPageCommand(object sender, EventArgs e)
        {
            if (panel != null)
            {
                panel.PageUp();              
            }
        }

        private void imageGridViewModel_NextPageCommand(object sender, EventArgs e)
        {
            if (panel != null)
            {
                panel.PageDown();
            }
        }

        private void imageGridViewModel_FirstPageCommand(object sender, EventArgs e)
        {
            if (panel != null)
            {
                panel.SetHorizontalOffset(double.NegativeInfinity);
            }
        }

        private void imageGridViewModel_LastPageCommand(object sender, EventArgs e)
        {
            if (panel != null)
            {
                panel.SetHorizontalOffset(double.PositiveInfinity);
            }
        }

        void imageGridViewModel_Cleared(object sender, EventArgs e)
        {
            if (panel != null)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() => panel.ScrollOwner.ScrollToVerticalOffset(0)));
            }
        }

        private void virtualizingTilePanel_Loaded(object sender, RoutedEventArgs e)
        {
            panel = sender as VirtualizingTilePanel;
                        
        }

        public bool KeepAlive
        {
            get { return(true); }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return (true);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            ViewModel.OnNavigatedFrom(navigationContext);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {            
            DataContext = navigationContext.Parameters["viewModel"];

            ViewModel.OnNavigatedTo(navigationContext);
                               
        }
    }
}
