using MediaViewer.Infrastructure;
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

namespace MediaViewer.ImagePanel
{
    /// <summary>
    /// Interaction logic for ImageNavigationItemView.xaml
    /// </summary>
    [Export]
    [ViewSortHint("01")]
    public partial class ImageNavigationItemView : UserControl
    {
      
        [ImportingConstructor]
        public ImageNavigationItemView(IRegionManager regionManager)
        {
            InitializeComponent();

            regionManager.Regions[RegionNames.MainContentRegion].ActiveViews.CollectionChanged += Views_CollectionChanged;
        }

        private void Views_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems[0] is ImageView)
            {
                navigationButton.IsChecked = true;
            }
        }

        private void navigationButton_Click(object sender, RoutedEventArgs e)
        {
            ShellViewModel vm = (ShellViewModel)DataContext;
            vm.navigateToImageView();   
        }
    }
}
