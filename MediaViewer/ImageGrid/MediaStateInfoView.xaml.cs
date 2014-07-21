using MediaViewer.MediaFileModel.Watcher;
using System;
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
using MediaViewer.MediaFileModel;

namespace MediaViewer.ImageGrid
{
    /// <summary>
    /// Interaction logic for MediaStateInfo.xaml
    /// </summary>
    public partial class MediaStateInfoView : UserControl
    {
        public MediaStateInfoView()
        {
            InitializeComponent();

            infoImage.Source = (ImageSource)Resources["folder"];
        }

        public String MediaStateInfo
        {
            get { return (String)GetValue(MediaStateInfoProperty); }
            set { SetValue(MediaStateInfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MediaStateInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaStateInfoProperty =
            DependencyProperty.Register("MediaStateInfo", typeof(String), typeof(MediaStateInfoView), new PropertyMetadata(null, collectionInfoView_MediaStateInfoChangedCallback));

        private static void collectionInfoView_MediaStateInfoChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaStateInfoView infoView = d as MediaStateInfoView;
            infoView.infoLabel.Content = (String)e.NewValue;
        }

        public DateTime MediaStateDateTime
        {
            get { return (DateTime)GetValue(MediaStateDateTimeProperty); }
            set { SetValue(MediaStateDateTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MediaStateDateTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaStateDateTimeProperty =
            DependencyProperty.Register("MediaStateDateTime", typeof(DateTime), typeof(MediaStateInfoView), new PropertyMetadata(DateTime.Now, collectionInfoView_MediaStateDateTimeChangedCallback));

        private static void collectionInfoView_MediaStateDateTimeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaStateInfoView infoView = d as MediaStateInfoView;
            infoView.dateTimeLabel.Content = ((DateTime)e.NewValue).ToString("MMM d, yyyy");
        }

        public MediaStateType MediaStateType
        {
            get { return (MediaStateType)GetValue(MediaStateTypeProperty); }
            set { SetValue(MediaStateTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MediaStateType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaStateTypeProperty =
            DependencyProperty.Register("MediaStateType", typeof(MediaStateType), typeof(MediaStateInfoView), new PropertyMetadata(MediaStateType.Directory, collectionInfoView_MediaStateTypeChangedCallback));

        private static void collectionInfoView_MediaStateTypeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaStateInfoView infoView = d as MediaStateInfoView;

            MediaStateType collectionType = (MediaStateType)e.NewValue;

            switch (collectionType)
            {
                case MediaStateType.Directory:
                    infoView.infoImage.Source = (ImageSource)infoView.Resources["folder"];
                    break;
                case MediaStateType.SearchResult:
                    infoView.infoImage.Source = (ImageSource)infoView.Resources["search"];
                    break;
                case MediaStateType.Other:
                    infoView.infoImage.Source = (ImageSource)infoView.Resources["folder"];
                    break;
                default:
                    break;
            }
        }


        
        
        
    }
}
