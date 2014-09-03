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
using MediaViewer.Utils.WPF;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.UserControls.Layout;
using MvvmFoundation.Wpf;
using Microsoft.Practices.Prism.Regions;
using System.ComponentModel.Composition;
using MediaViewer.MediaFileBrowser;

namespace MediaViewer.ImageGrid
{
    /// <summary>
    /// Interaction logic for ImageGridControl.xaml
    /// </summary>
    [Export]
    public partial class ImageGridView : UserControl, IRegionMemberLifetime, INavigationAware
    {
        VirtualizingTilePanel panel;

        ImageGridViewModel ViewModel
        {
            get;
            set;
        }

        public ImageGridView()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public ImageGridView(IRegionManager regionManager)
        {           
            InitializeComponent();

            panel = null;

            //ViewModel = new ImageGridViewModel(MediaFileWatcher.Instance.MediaState, regionManager);
            //DataContext = ViewModel;

        }

        void ImageGridView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ImageGridViewModel)
            {
                ImageGridViewModel imageGridViewModel = e.OldValue as ImageGridViewModel;

                WeakEventManager<ImageGridViewModel, EventArgs>.RemoveHandler(imageGridViewModel, "Cleared", imageGridViewModel_Cleared);

                WeakEventManager<Command, EventArgs>.RemoveHandler(imageGridViewModel.LastPageCommand, "Executed", imageGridViewModel_LastPageCommand);
                WeakEventManager<Command, EventArgs>.RemoveHandler(imageGridViewModel.FirstPageCommand, "Executed", imageGridViewModel_FirstPageCommand);
                WeakEventManager<Command, EventArgs>.RemoveHandler(imageGridViewModel.NextPageCommand, "Executed", imageGridViewModel_NextPageCommand);
                WeakEventManager<Command, EventArgs>.RemoveHandler(imageGridViewModel.PrevPageCommand, "Executed", imageGridViewModel_PrevPageCommand);
            }

            if (e.NewValue is ImageGridViewModel)
            {
                ImageGridViewModel imageGridViewModel = e.NewValue as ImageGridViewModel;

                WeakEventManager<ImageGridViewModel,EventArgs>.AddHandler(imageGridViewModel,"Cleared",imageGridViewModel_Cleared);

                WeakEventManager<Command, EventArgs>.AddHandler(imageGridViewModel.LastPageCommand, "Executed", imageGridViewModel_LastPageCommand);
                WeakEventManager<Command, EventArgs>.AddHandler(imageGridViewModel.FirstPageCommand, "Executed", imageGridViewModel_FirstPageCommand);
                WeakEventManager<Command, EventArgs>.AddHandler(imageGridViewModel.NextPageCommand, "Executed", imageGridViewModel_NextPageCommand);
                WeakEventManager<Command, EventArgs>.AddHandler(imageGridViewModel.PrevPageCommand, "Executed", imageGridViewModel_PrevPageCommand);
            }
            
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
           
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            ViewModel = (ImageGridViewModel)navigationContext.Parameters["viewModel"];

            DataContext = ViewModel;
        }
    }
}
