using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using MediaViewer.ImageGrid;
using MediaViewer.MetaData;
using MediaViewer.MVMediaFile;

namespace MediaViewer.MediaPreview
{
    /// <summary>
    /// Interaction logic for MediaPreviewControl.xaml
    /// </summary>
    public partial class MediaPreviewControl : UserControl, INotifyPropertyChanged
    {

        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        MediaFileFactory mediaFileFactory;
        MediaFile media;

        enum InformImage
        {
            LOADING_IMAGE,
            ERROR_IMAGE
        }

        static List<BitmapImage> informImage;        

        static Color selectedColor = SystemColors.InfoColor;
        static Color defaultColor = SystemColors.ControlLightLightColor;

        public MediaPreviewAsyncState AsyncState
        {
            get { return (MediaPreviewAsyncState)this.GetValue(AsyncStateProperty); }
            set {              
                this.SetValue(AsyncStateProperty, value); 
            }
        }

        public static readonly DependencyProperty AsyncStateProperty = DependencyProperty.Register(
          "AsyncState", typeof(MediaPreviewAsyncState), typeof(MediaPreviewControl), new PropertyMetadata(OnAsyncStateChanged));


        private void OnAsyncStateChanged(MediaPreviewAsyncState state)
        {
            loadMedia(state);
        }

        static void OnAsyncStateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {

            MediaPreviewControl This = (MediaPreviewControl)obj;          
            MediaPreviewAsyncState state = (MediaPreviewAsyncState)args.NewValue;
            This.OnAsyncStateChanged(state);
        }

        bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                if (isSelected == true)
                {
                    BackgroundColor = selectedColor;
                }
                else
                {
                    BackgroundColor = defaultColor;
                }
            }
        }

        Color backgroundColor;

        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                backgroundColor = value;
                PropertyChanged(this, new PropertyChangedEventArgs("BackgroundColor"));
            }
        }

        public MediaPreviewControl()
        {
            InitializeComponent();

            mediaFileFactory = new MediaFileFactory();
            mediaFileFactory.OpenFinished += new EventHandler<MediaFile>(mediaFileFactory_OpenFinished);    

         
            IsSelected = false;
        }

     

       
        delegate void loadPreviewDelegate(MediaFile media, List<MetaDataThumb> thumbs);

        void mediaFileFactory_OpenFinished(System.Object sender, MediaFile media)
        {

            Object[] args = new Object[2];

            args[0] = media;
            args[1] = null;

            try
            {

                // grab or generate thumbnail images
                args[1] = media.getThumbnails();

            }
            catch (Exception e)
            {

                log.Error("Error generating thumbnails", e);
                media.OpenError = e;

            }
            finally
            {
               
                if (!Dispatcher.HasShutdownFinished)
                {

                    Dispatcher.Invoke(new loadPreviewDelegate(loadPreview), args);
                }
            }

        }

        void loadPreview(MediaFile media, List<MetaDataThumb> thumbs)
        {

            try
            {

                this.media = media;

                clearPictureBox();

                if (!media.OpenSuccess)
                {

                    if (media.OpenError.GetType() != typeof(MediaFileException))
                    {
                        setPictureBoxInformImage(InformImage.ERROR_IMAGE);
                    }

                }
                else if (string.IsNullOrEmpty(media.Location) || media.MediaFormat == MediaFile.MediaType.UNKNOWN)
                {

                    // unknown or empty file
                    return;

                }
                else if (thumbs.Count > 0)
                {

                    setPictureBoxImage(thumbs[0].ThumbImage);

                }

                setPictureBoxFeatures(media);

            }
            catch (Exception e)
            {

                log.Error("Error opening preview", e);

            }
            finally
            {

                media.close();

                // release the lock on opening of images
                mediaFileFactory.releaseNonBlockingOpenLock();
            }

        }

        void setPictureBoxFeatures(MediaFile media)
        {

            MediaPreviewAsyncState state = (MediaPreviewAsyncState)media.UserState;

            if (state == null) return;

            if (state.InfoIconMode == MediaPreviewAsyncState.InfoIconModes.DEFAULT_ICONS_ONLY ||
                state.InfoIconMode == MediaPreviewAsyncState.InfoIconModes.SHOW_ALL_ICONS)
            {
                InfoIcon icon = new InfoIcon(media.MimeType);
                icon.Caption = media.getDefaultFormatCaption();
                addInfoIcon(icon);

                if (media.MetaData == null)
                {

                    icon = new InfoIcon(InfoIcon.IconType.ERROR);
                    icon.Caption = "Cannot read metadata";

                    addInfoIcon(icon);

                }
                else
                {

                    if (media.MetaData.HasGeoTag)
                    {

                        icon = new InfoIcon(InfoIcon.IconType.GEOTAG);
                        icon.Caption = "Geo Tag";

                        addInfoIcon(icon);
                    }

                    int nrTags = media.MetaData.Tags.Count;

                    if (nrTags > 0)
                    {

                        icon = new InfoIcon(InfoIcon.IconType.TAGGED);

                        for (int i = 0; i < nrTags; i++)
                        {

                            icon.Caption += media.MetaData.Tags[i];
                            if (i != nrTags - 1)
                            {

                                icon.Caption += "\n";
                            }

                        }

                        addInfoIcon(icon);
                    }
                }

                // if media is a video and has no audio add a muted icon
                if (media.MediaFormat == MediaFile.MediaType.VIDEO)
                {

                    VideoFile video = (VideoFile)media;

                    if (video.HasAudio == false)
                    {

                        icon = new InfoIcon(InfoIcon.IconType.MUTE);
                        icon.Caption = "Video contains no audio";
                        addInfoIcon(icon);

                    }

                }
            }

            if (state.InfoIconMode == MediaPreviewAsyncState.InfoIconModes.CUSTOM_ICONS_ONLY ||
                state.InfoIconMode == MediaPreviewAsyncState.InfoIconModes.SHOW_ALL_ICONS)
            {

                for (int i = 0; i < state.InfoIcon.Count; i++)
                {

                    addInfoIcon(state.InfoIcon[i]);
                }

            }

            if (string.IsNullOrEmpty(state.Caption))
            {

                pictureBox.ToolTip = media.getDefaultCaption();

            }
            else
            {

                pictureBox.ToolTip = state.Caption;
            }

            //pictureBox.ContextMenuStrip = item.ContextMenu;

        }

        void addInfoIcon(InfoIcon icon)
        {

        }

        void setPictureBoxImage(BitmapSource image)
        {

            //pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            //pictureBox.TransparencyEnabled = false;
           
            pictureBox.Source = image;
           

        }

        void setPictureBoxInformImage(InformImage image)
        {

            clearPictureBox();

            if (informImage == null)
            {
                informImage = new List<BitmapImage>();
                informImage.Add(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/loading1.gif")));
                informImage.Add(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/error.png")));
            }

            // pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            //pictureBox.TransparencyEnabled = true;
            pictureBox.Source = informImage[(int)image];


        }

        void clearPictureBox()
        {
            pictureBox.Source = null;
            pictureBox.ToolTip = "";
            /*
                    if(pictureBox.Image != null) {

                        if(!informImage.Contains(pictureBox.Image)) {

                            delete pictureBox.Image;
                        }
                        pictureBox.Image = null;
                        pictureBox.clearInfoIcons();
                        pictureBox.Caption = "";
                        pictureBox.ContextMenuStrip = null;
			
                    }		
             */
        }

  
 


        public string Location
        {

            get
            {

                if (media == null) return ("");
                else return (media.Location);
            }

        }

        public bool IsEmpty
        {

            get
            {

                return (pictureBox.Source == null ? true : false);
            }

        }

        public void loadMedia(MediaPreviewAsyncState state)
        {
         

            if (string.IsNullOrEmpty(state.MediaLocation))
            {

                clearPictureBox();

            }
            else
            {

                log.Info("Opening media: " + state.MediaLocation);
                setPictureBoxInformImage(InformImage.LOADING_IMAGE);
            }
         

            mediaFileFactory.openNonBlockingAndCancelPending(state.MediaLocation, state,
                MediaFile.MetaDataMode.LOAD_FROM_DISK);

        }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

    
    }
}
