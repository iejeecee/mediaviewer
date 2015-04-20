using MediaViewer.Model.Media.File;
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

namespace MediaViewer.GeotagFileBrowser
{
    /// <summary>
    /// Interaction logic for GeotagBrowserView.xaml
    /// </summary>
    [Export]
    public partial class GeotagFileBrowserView : UserControl, INavigationAware
    {
        GeotagFileBrowserViewModel ViewModel { get; set; }

        public GeotagFileBrowserView()
        {
            InitializeComponent();
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
            ViewModel = (GeotagFileBrowserViewModel)navigationContext.Parameters["viewModel"];
            DataContext = ViewModel;

            ViewModel.OnNavigatedTo(navigationContext);   
        }
    }
}
