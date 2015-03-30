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

namespace MediaViewer.MediaFileGrid
{
    /// <summary>
    /// Interaction logic for MediaFileGridView.xaml
    /// </summary>
    [Export]
    public partial class MediaFileGridView : UserControl, IRegionMemberLifetime, INavigationAware
    {
        MediaFileGridViewModel ViewModel { get; set; }

        public MediaFileGridView()
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
            DataContext = ViewModel = (MediaFileGridViewModel)navigationContext.Parameters["viewModel"];

            ViewModel.OnNavigatedTo(navigationContext);
        }

        public bool KeepAlive
        {
            get { return (true); }
        }
    }
}
