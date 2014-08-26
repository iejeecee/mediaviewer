using MediaViewer;
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

namespace PluginTest
{
    /// <summary>
    /// Interaction logic for GoogleEarthGeoTagNavigationItem.xaml
    /// </summary>
    [Export]
    public partial class GoogleEarthGeoTagNavigationItemView : UserControl
    {
        [Import]
        public IRegionManager RegionManager;

        public GoogleEarthGeoTagNavigationItemView()
        {
            InitializeComponent();
        }

        private void navigationButton_Click(object sender, RoutedEventArgs e)
        {
            Uri googleEarthGeoTagViewUri = new Uri(typeof(GoogleEarthGeoTagView).FullName, UriKind.Relative);

            RegionManager.RequestNavigate(RegionNames.MediaFileBrowserContentRegion, googleEarthGeoTagViewUri);
        }
    }
}
