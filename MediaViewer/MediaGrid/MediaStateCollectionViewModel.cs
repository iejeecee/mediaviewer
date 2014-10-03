using MediaViewer.Model.Media.State;
using MediaViewer.Model.Media.State.CollectionView;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaGrid
{
    public class MediaStateCollectionViewModel : ObservableObject
    {
        IMediaState mediaState;

        public IMediaState MediaState
        {
            get
            {
                return (mediaState);
            }
        }

        DefaultMediaStateCollectionView mediaStateCollectionView;

        public DefaultMediaStateCollectionView MediaStateCollectionView
        {
            get { return mediaStateCollectionView; }
            protected set
            {
                mediaStateCollectionView = value;
                NotifyPropertyChanged();
            }
        }


        public MediaStateCollectionViewModel(IMediaState mediaState)
        {
            if (mediaState == null)
            {
                throw new ArgumentNullException("mediaState cannot be null");
            }

            this.mediaState = mediaState;

            MediaStateCollectionView = new DefaultMediaStateCollectionView(mediaState);
            
        }

        String imageGridInfo;

        public String ImageGridInfo
        {
            get { return imageGridInfo; }
            set
            {
                imageGridInfo = value;
                NotifyPropertyChanged();
            }
        }

        DateTime imageGridInfoDateTime;

        public DateTime ImageGridInfoDateTime
        {
            get { return imageGridInfoDateTime; }
            set { imageGridInfoDateTime = value; }
        }


        
    }
}
