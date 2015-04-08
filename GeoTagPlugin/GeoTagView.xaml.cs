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

namespace GeoTagPlugin
{
    /// <summary>
    /// Interaction logic for GoogleEarthGeoTagView.xaml
    /// </summary>
    [Export]
    public partial class GeoTagView : UserControl, IRegionMemberLifetime
    {
        GeoTagViewModel ViewModel { get; set; }

        [ImportingConstructor]
        public GeoTagView(GeoTagViewModel viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
            DataContext = ViewModel;
            
        }

        public bool KeepAlive
        {
            get { return (true); }
        }

        private void searchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.SearchCommand.Execute();
            }
        }
    }
}
