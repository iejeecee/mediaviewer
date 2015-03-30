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
using MediaViewer.Model.Media.State;
using System.ComponentModel;

namespace MediaViewer.UserControls.MediaStateInfo
{
    /// <summary>
    /// Interaction logic for MediaStateInfo.xaml
    /// </summary>
    public partial class MediaStateInfoView : UserControl
    {
        const String dateFormat = "MMM d, yyyy";

        public MediaStateInfoView()
        {
            InitializeComponent();

            infoImage.Source = (ImageSource)Resources["folder"];
        }

        public MediaViewer.Model.Media.State.MediaState MediaState
        {
            get { return (MediaViewer.Model.Media.State.MediaState)GetValue(MediaStateProperty); }
            set { SetValue(MediaStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MediaState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaStateProperty =
            DependencyProperty.Register("MediaState", typeof(MediaViewer.Model.Media.State.MediaState), typeof(MediaStateInfoView), new PropertyMetadata(null, mediaStateChangedCallback));

        private static void mediaStateChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaStateInfoView view = (MediaStateInfoView)d;

            if (e.OldValue != null)
            {
                MediaViewer.Model.Media.State.MediaState oldState = (MediaViewer.Model.Media.State.MediaState)e.OldValue;

                oldState.PropertyChanged -= view.mediaState_PropertyChanged;
                ((INotifyPropertyChanged)oldState.UIMediaCollection).PropertyChanged -= view.uiMediaCollection_PropertyChanged;
            }

            if (e.NewValue != null)
            {
                MediaViewer.Model.Media.State.MediaState newState = (MediaViewer.Model.Media.State.MediaState)e.NewValue;

                newState.PropertyChanged += view.mediaState_PropertyChanged;
                ((INotifyPropertyChanged)newState.UIMediaCollection).PropertyChanged += view.uiMediaCollection_PropertyChanged;

                view.initialize(newState);
                
            }
        }

        void initialize(MediaViewer.Model.Media.State.MediaState state)
        {
            infoLabelTextBlock.Text = state.MediaStateInfo;
            dateTimeLabel.Content = state.MediaStateDateTime.ToString(dateFormat);
            setMediaStateType(state.MediaStateType);
            setNrItemsInStateLabel(state.UIMediaCollection.NrLoadedItems, state.UIMediaCollection.Count);
        }

        private void mediaState_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MediaViewer.Model.Media.State.MediaState state = (MediaViewer.Model.Media.State.MediaState)sender;

            switch (e.PropertyName)
            {
                case "MediaStateInfo":
                    {
                        Dispatcher.BeginInvoke(new Action(() => setInfoText(state.MediaStateInfo)));
                        break;
                    }
                case "MediaStateDateTime":
                    {
                        Dispatcher.BeginInvoke(new Action(() =>setDateTime(state.MediaStateDateTime)));
                        break;
                    }
                case "MediaStateType":
                    {
                        Dispatcher.BeginInvoke(new Action(() => setMediaStateType(state.MediaStateType)));                        
                        break;
                    }
                
            }
        }

        private void uiMediaCollection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MediaLockedCollection media = (MediaLockedCollection)sender;

            switch (e.PropertyName)
            {
                case "Count":
                    {
                        Dispatcher.BeginInvoke(new Action(() => setNrItemsInStateLabel(media.NrLoadedItems, media.Count)));
                        break;
                    }
                case "NrLoadedItems":
                    {
                        Dispatcher.BeginInvoke(new Action(() => setNrItemsInStateLabel(media.NrLoadedItems, media.Count)));
                        break;
                    }
            }

        }

        void setInfoText(string info)
        {
            infoLabelTextBlock.Text = info;
        }

        void setDateTime(DateTime dateTime)
        {
            dateTimeLabel.Content = dateTime.ToString(dateFormat);
        }

        void setNrItemsInStateLabel(int nrLoadedItemsInState, int nrItemsInState)
        {
            String prefix = "";

            if (nrLoadedItemsInState != nrItemsInState)
            {
                prefix = nrLoadedItemsInState.ToString() + "/";
            }

            nrItemsLabel.Content = prefix + nrItemsInState.ToString() + " item";
            if (nrItemsInState != 1)
            {
                nrItemsLabel.Content += "s";
            }
        }

        void setMediaStateType(MediaStateType type)
        {
            switch (type)
            {
                case MediaStateType.Directory:
                    infoImage.Source = (ImageSource)Resources["folder"];
                    break;
                case MediaStateType.SearchResult:
                    infoImage.Source = (ImageSource)Resources["search"];
                    break;
                case MediaStateType.Other:
                    infoImage.Source = (ImageSource)Resources["folder"];
                    break;
                default:
                    break;
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
