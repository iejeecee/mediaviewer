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
    /// Interaction logic for GoogleEarthGeoTagView.xaml
    /// </summary>
    [Export]
    public partial class GoogleEarthGeoTagView : UserControl, IRegionMemberLifetime
    {
        GoogleEarthGeoTagViewModel ViewModel { get; set; }

        [ImportingConstructor]
        public GoogleEarthGeoTagView(GoogleEarthGeoTagViewModel viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
            DataContext = ViewModel;
            
        }

        public bool KeepAlive
        {
            get { return (true); }
        }
    }
}
