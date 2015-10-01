using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Settings;
using MediaViewer.UserControls.TabbedExpander;
using Microsoft.Practices.Prism.PubSubEvents;
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

namespace MediaViewer.MediaFileBrowser.DirectoryBrowser
{
    /// <summary>
    /// Interaction logic for MediaFileBrowserDirectoryBrowserView.xaml
    /// </summary>
    [Export]
    public partial class MediaFileBrowserDirectoryBrowserView : UserControl, INavigationAware, ITabbedExpanderAware
    {
        [ImportingConstructor]
        public MediaFileBrowserDirectoryBrowserView(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            DataContext = new MediaFileBrowserDirectoryBrowserViewModel(MediaFileWatcher.Instance, eventAggregator);

            TabName = "Browse";
            TabIsSelected = true;
            TabMargin = new Thickness(2);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return (true);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
           
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            
        }

        public string TabName { get; set; }
        public bool TabIsSelected { get; set; }
        public Thickness TabMargin { get; set; }
        public Thickness TabBorderThickness { get; set; }
        public Brush TabBorderBrush { get; set; }
    }
}
