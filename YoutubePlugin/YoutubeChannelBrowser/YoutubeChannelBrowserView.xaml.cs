using MediaViewer.UserControls.TabbedExpander;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Windows.Themes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using YoutubePlugin.Events;

namespace YoutubePlugin.YoutubeChannelBrowser
{
    /// <summary>
    /// Interaction logic for YoutubeChannelBrowserView.xaml
    /// </summary>
    [Export]
    public partial class YoutubeChannelBrowserView : UserControl, ITabbedExpanderAware
    {
        IEventAggregator EventAggregator { get; set; }

        [ImportingConstructor]
        public YoutubeChannelBrowserView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            EventAggregator = eventAggregator;

            treeView.SelectionChanged += treeView_SelectionChanged;

            TabName = "Browse";
            TabMargin = new Thickness(2);
            TabBorderThickness = new Thickness(0);
            TabBorderBrush = null;
        }

        void treeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (treeView.SelectedItems.Count == 0)
            {
                return;
            }

            YoutubeNodeBase item = treeView.SelectedItems[0] as YoutubeNodeBase;

            //EventAggregator.GetEvent<BrowseEvent>().Publish(item);
        }

        public string TabName { get; set; }
        public bool TabIsSelected { get; set; }
        public Thickness TabMargin { get; set; }
        public Thickness TabBorderThickness { get; set; }
        public Brush TabBorderBrush { get; set; }
    }
}
