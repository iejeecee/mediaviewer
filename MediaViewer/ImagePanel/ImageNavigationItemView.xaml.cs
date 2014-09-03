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
        [Import]
        public IRegionManager RegionManager {get;set;}

        public ImageNavigationItemView()
        {
            InitializeComponent();
    
        }

        private void navigationButton_Click(object sender, RoutedEventArgs e)
        {
            ShellViewModel vm = (ShellViewModel)DataContext;
            vm.navigateToImageView();   
        }
    }
}
