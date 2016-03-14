using MediaViewer.MediaDatabase;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VideoLib;

namespace MediaViewer.UserControls.MediaPreview
{
    public abstract class MediaPreviewViewModelBase : BindableBase
    {
        protected static BitmapImage AudioImage { get; set;}
        protected static BitmapImage ErrorImage { get; set;}

        static MediaPreviewViewModelBase()
        {
            AudioImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/audio.ico", UriKind.Absolute));            
            ErrorImage = new BitmapImage(new Uri("pack://application:,,,/MediaViewer;component/Resources/Images/error.png", UriKind.Absolute));         
        }

        protected MediaPreviewViewModelBase()
        {

        }

        ImageSource mediaPreviewImage;

        public ImageSource MediaPreviewImage
        {
            get
            {
                return mediaPreviewImage;
            }
            set
            {
                SetProperty(ref mediaPreviewImage, value);
            }
        }

        public abstract void startVideoPreview(CancellationToken token);         
        public abstract void endVideoPreview();

        public abstract MediaThumb getVideoPreviewThumbnail(double pos, CancellationToken token);
        
                     
    }
}
