using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
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
using System.ComponentModel.Composition;
using Microsoft.Practices.ServiceLocation;
using MediaViewer.Infrastructure;

namespace MediaViewer.VideoPanel
{
    /// <summary>
    /// Interaction logic for VideoNavigationItemView.xaml
    /// </summary>
    [Export]
    [ViewSortHint("02")]
    public partial class VideoNavigationItemView : UserControl
    {
   
        [ImportingConstructor]
        public VideoNavigationItemView(IRegionManager regionManager)
        {
            InitializeComponent();

            // initialize a instance of videosettings
            ServiceLocator.Current.GetInstance(typeof(VideoSettingsViewModel));

            regionManager.Regions[RegionNames.MainContentRegion].ActiveViews.CollectionChanged += Views_CollectionChanged;
        }

        private void Views_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems[0] is VideoView)
            {
                navigationButton.IsChecked = true;
            }
        }

        private void navigationButton_Click(object sender, RoutedEventArgs e)
        {
            ShellViewModel vm = (ShellViewModel)DataContext;
            vm.navigateToVideoView();          
        }
    }
}
