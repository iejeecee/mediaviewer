using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
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

namespace ImageSearchPlugin
{
    /// <summary>
    /// Interaction logic for GoogleImageSearchNavigationItem.xaml
    /// </summary>
    [Export]
    [ViewSortHint("04")]
    public partial class ImageSearchNavigationItemView : UserControl
    {
      
        public ImageSearchNavigationItemView()
        {
            InitializeComponent();
           
        }

        private void navigationButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;

            GoogleImageSearchView imageSearch = new GoogleImageSearchView();            
            imageSearch.Closed += imageSearch_Closed;

            imageSearch.Show();
        }

        void imageSearch_Closed(object sender, EventArgs e)
        {
            this.IsEnabled = true;
            navigationButton.IsChecked = false;
        }
    }
}
