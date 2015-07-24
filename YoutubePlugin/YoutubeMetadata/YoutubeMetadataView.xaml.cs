using MediaViewer.UserControls.Layout;
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

namespace YoutubePlugin.YoutubeMetadata
{
    /// <summary>
    /// Interaction logic for YoutubeMetadataView.xaml
    /// </summary>
    [Export]
    public partial class YoutubeMetadataView : UserControl, ITabbedExpanderAware
    {
        YoutubeMetadataViewModel ViewModel { get; set; }
        IEventAggregator EventAggregator { get; set; }

        [ImportingConstructor]
        public YoutubeMetadataView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            EventAggregator = eventAggregator;

            DataContext = ViewModel = new YoutubeMetadataViewModel(EventAggregator);

            TabName = "Metadata";
            TabIsSelected = true;
            TabMargin = new Thickness(2);
            TabBorderThickness = new Thickness(2);
            TabBorderBrush = ClassicBorderDecorator.ClassicBorderBrush;
        }

        public string TabName { get; set; }
        public bool TabIsSelected { get; set; }
        public Thickness TabMargin { get; set; }
        public Thickness TabBorderThickness { get; set; }
        public Brush TabBorderBrush { get; set; }
        
    }
}
