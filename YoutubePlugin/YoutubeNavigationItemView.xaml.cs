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

namespace YoutubePlugin
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    [Export]
    [ViewSortHint("05")]
    public partial class YoutubeNavigationItemView : UserControl
    {
        IRegionManager RegionManager { get; set; }

        [ImportingConstructor]
        public YoutubeNavigationItemView(IRegionManager regionManager)
        {
            InitializeComponent();
            RegionManager = regionManager;                       
        }
               
        private void navigationButton_Click(object sender, RoutedEventArgs e)
        {
            RegionManager.RequestNavigate(RegionNames.MainContentRegion, typeof(YoutubeView).FullName);                    
        }

        
    }
}
