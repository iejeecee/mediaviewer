using MediaViewer.Model.Media.File.Watcher;
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
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.State;

namespace MediaViewer.MediaGrid
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
            infoView.infoLabelTextBlock.Text = (String)e.NewValue;
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

        public int NrItemsInState
        {
            get { return (int)GetValue(NrItemsInStateProperty); }
            set { SetValue(NrItemsInStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NrItemsInState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NrItemsInStateProperty =
            DependencyProperty.Register("NrItemsInState", typeof(int), typeof(MediaStateInfoView), new PropertyMetadata(0, mediaStateInfoView_NrItemsInStateChangedCallback));

        private static void mediaStateInfoView_NrItemsInStateChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaStateInfoView infoView = d as MediaStateInfoView;
            int nrItemsInState = (int)e.NewValue;
            infoView.setNrItemsInStateLabel(infoView.NrLoadedItemsInState, nrItemsInState);
        }

        public int NrLoadedItemsInState
        {
            get { return (int)GetValue(NrLoadedItemsInStateProperty); }
            set { SetValue(NrLoadedItemsInStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NrItemsInState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NrLoadedItemsInStateProperty =
            DependencyProperty.Register("NrLoadedItemsInState", typeof(int), typeof(MediaStateInfoView), new PropertyMetadata(0, mediaStateInfoView_NrLoadedItemsInStateChangedCallback));

        private static void mediaStateInfoView_NrLoadedItemsInStateChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaStateInfoView infoView = d as MediaStateInfoView;
            int nrLoadedItemsInState = (int)e.NewValue;
            infoView.setNrItemsInStateLabel(nrLoadedItemsInState, infoView.NrItemsInState);
            
        }

        void setNrItemsInStateLabel(int nrLoadedItemsInState, int nrItemsInState)
        {
            String prefix = "";

            if (NrLoadedItemsInState != nrItemsInState)
            {
                prefix = nrLoadedItemsInState.ToString() + "/";
            }

            nrItemsLabel.Content = prefix + nrItemsInState.ToString() + " item";
            if (nrItemsInState != 1)
            {
                nrItemsLabel.Content += "s";
            }
        }

        public bool IsFlatMode
        {
            get { return (bool)GetValue(IsFlatModeProperty); }
            set { SetValue(IsFlatModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFlatMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFlatModeProperty =
            DependencyProperty.Register("IsFlatMode", typeof(bool), typeof(MediaStateInfoView), new PropertyMetadata(false,isFlatModeChangedCallback));

        private static void isFlatModeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaStateInfoView infoView = d as MediaStateInfoView;

            if ((bool)e.NewValue == true)
            {
                infoView.dateTimeLabel.SetValue(Grid.RowProperty, 0);
                infoView.dateTimeLabel.SetValue(Grid.ColumnProperty, 2);
                infoView.dateTimeLabel.FontWeight = FontWeights.Normal;
                infoView.dateTimeLabel.FontSize = infoView.infoLabel.FontSize;
            }
            else
            {
                infoView.dateTimeLabel.SetValue(Grid.RowProperty, 1);
                infoView.dateTimeLabel.SetValue(Grid.ColumnProperty, 0);
                infoView.dateTimeLabel.FontWeight = FontWeights.Bold;
                infoView.dateTimeLabel.FontSize = 8;
            }

        }        
        
    }
}
