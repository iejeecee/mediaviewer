using MediaViewer.Infrastructure.Utils;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Media.Base.Item;
using MediaViewer.Model.Utils;
using MediaViewer.UserControls.TabbedExpander;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Windows.Themes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace MediaViewer.UserControls.MediaPreview
{
    /// <summary>
    /// Interaction logic for MediaPreviewView.xaml
    /// </summary>
    [Export]
    public partial class MediaPreviewView : UserControl, INavigationAware, ITabbedExpanderAware
    {
        IEventAggregator EventAggregator { get; set; }

        MediaItem PreviewItem { get; set; }
        int CurrentPreviewImage { get; set; }

        CancellationTokenSource TokenSource { get; set; }
        Task<BitmapImage> LoadImageTask { get; set; }
      
        TimeAdorner TimeAdorner { get; set; }

        static BitmapImage audioImage;
        static BitmapImage errorImage;

        static MediaPreviewView()
        {
            audioImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/audio.ico", UriKind.Absolute));
            errorImage = new BitmapImage(new Uri("pack://application:,,,/MediaViewer;component/Resources/Images/error.png", UriKind.Absolute));
        }

        [ImportingConstructor]
        public MediaPreviewView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            EventAggregator = eventAggregator;

            TabName = "Preview";
            TabIsSelected = true;          
            TabMargin = new Thickness(2);
            TabBorderThickness = new Thickness(2);
            TabBorderBrush = ClassicBorderDecorator.ClassicBorderBrush;

            TokenSource = new CancellationTokenSource();
           
            TimeAdorner = new TimeAdorner(previewImage);
        }

        public string TabName { get; set; }
        public bool TabIsSelected { get; set; }
        public Thickness TabMargin { get; set; }
        public Thickness TabBorderThickness { get; set; }
        public Brush TabBorderBrush { get; set; }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return (true);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
           
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            EventAggregator.GetEvent<MediaSelectionEvent>().Subscribe(mediaSelectionEvent,ThreadOption.UIThread);
        }

        private void mediaSelectionEvent(MediaSelectionPayload selection)
        {
            if (selection.Items.Count == 0 || selection.Items.ElementAt(0).Metadata == null)
            {
                previewImageBorder.Visibility = Visibility.Collapsed;
                previewImage.Source = null;
                PreviewItem = null;
                CurrentPreviewImage = 0;
                return;
            }

            previewImageBorder.Visibility = Visibility.Visible;

            PreviewItem = selection.Items.ElementAt(0);

            if (PreviewItem.Metadata.Thumbnail != null)
            {
                previewImage.Stretch = Stretch.Uniform;
                previewImage.Source = PreviewItem.Metadata.Thumbnail.Image;
            }
            else
            {                
                if (PreviewItem.Metadata is AudioMetadata)
                {
                    previewImage.Stretch = Stretch.Uniform;
                    previewImage.Source = audioImage;
                }
                else
                {
                    previewImage.Stretch = Stretch.None;
                    previewImage.Source = errorImage;
                }
            }

            CurrentPreviewImage = 0;
            
        }
      
        private void previewImage_MouseLeave(object sender, MouseEventArgs e)
        {
            if (CurrentPreviewImage != 0)
            {
                previewImage.Source = PreviewItem.Metadata.Thumbnail.Image;
                CurrentPreviewImage = 0;
            }

            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
            adornerLayer.Remove(TimeAdorner);
        }

        private void previewImage_previewMouseMove(object sender, MouseEventArgs e)
        {
            VideoMetadata videoMetadata = PreviewItem.Metadata as VideoMetadata;

            if (videoMetadata == null || videoMetadata.VideoThumbnails.Count == 0)
            {
                return;
            }            
                   
            Point mousePos = Mouse.GetPosition(previewImage);

            double grid = previewImage.ActualWidth / videoMetadata.VideoThumbnails.Count;
            int previewImageNr = MiscUtils.clamp<int>((int)Math.Floor(mousePos.X / grid), 0, videoMetadata.VideoThumbnails.Count - 1);

            if (previewImageNr != CurrentPreviewImage)
            {
                previewImage.Source = videoMetadata.VideoThumbnails.ElementAt(previewImageNr).Image;
                CurrentPreviewImage = previewImageNr;
            }

            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);

            adornerLayer.Remove(TimeAdorner);

            Size size = TimeAdorner.Size;
            double xLeft = grid * previewImageNr + grid / 2 - size.Width / 2;
            double xRight = xLeft + size.Width;

            if (xLeft < 0) xLeft = 0;
            if (xRight > previewImage.ActualWidth) xLeft = previewImage.ActualWidth - size.Width;

            TimeAdorner.Location = new Point(xLeft, previewImage.ActualHeight - size.Height);
            TimeAdorner.TimeSeconds = (int)videoMetadata.VideoThumbnails.ElementAt(previewImageNr).TimeSeconds;

            adornerLayer.Add(TimeAdorner);
            
        }

        private void previewImage_MouseEnter(object sender, MouseEventArgs e)
        {
            VideoMetadata videoMetadata = PreviewItem.Metadata as VideoMetadata;

            if (videoMetadata == null || videoMetadata.IsImported == false)
            {
                return;
            }

            using (MetadataDbCommands metadataCommands = new MetadataDbCommands())
            {
                metadataCommands.loadVideoThumbnails(videoMetadata);
            }
        }

    }
}
