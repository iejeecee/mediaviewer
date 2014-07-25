using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.MediaFileModel;
using MvvmFoundation.Wpf;
using System.Windows.Data;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using System.Collections.Specialized;
using MediaViewer.MediaFileModel.Watcher;

namespace MediaViewer.ImageGrid
{
    public enum FilterMode
    {
        All,
        Video,
        Images
    }

    public enum SortMode
    {
        Name,
        Size,
        Rating,
        Imported,
        Tags,
        CreationDate
    }
    
    class ImageGridViewModel : ObservableObject
    {

        IMediaState mediaState;

        public IMediaState MediaState
        {
            get
            {
                return (mediaState);
            }
        }

        CancellationTokenSource loadItemsCTS;

                  
        protected ImageGridViewModel(IMediaState mediaState)
        {

            if (mediaState == null)
            {
                throw new ArgumentException("mediaState cannot be null");
            }

            this.mediaState = mediaState;
          
            loadItemsCTS = new CancellationTokenSource();

            IsScrollBarEnabled = true;
            NrGridRows = 0;
            NrGridColumns = 4;

            Media = new ObservableCollection<MediaFileItem>();
            MediaLock = new Object();
            BindingOperations.EnableCollectionSynchronization(Media, MediaLock);

            filterMode = ImageGrid.FilterMode.All;
            sortMode = ImageGrid.SortMode.Name;
        }

        String imageGridInfo;

        public String ImageGridInfo
        {
            get { return imageGridInfo; }
            set { imageGridInfo = value;
            NotifyPropertyChanged();
            }
        }

        DateTime imageGridInfoDateTime;

        public DateTime ImageGridInfoDateTime
        {
            get { return imageGridInfoDateTime; }
            set { imageGridInfoDateTime = value; }
        }
             
        public void loadItemRangeAsync(int start, int nrItems)
        {

            // cancel any previously loading items           
            loadItemsCTS.Cancel();          
            // create new cts for the items that need to be loaded
            loadItemsCTS = new CancellationTokenSource();

            MediaState.readMetadataRangeAsync(start, nrItems, loadItemsCTS.Token);
                    
        }
       
        public MediaLockedCollection SelectedMedia
        {
            get { return mediaState.UISelectedMedia; }            
        }

        ObservableCollection<MediaFileItem> media;

        public ObservableCollection<MediaFileItem> Media
        {
            get { return media; }
            set { media = value; }
        }

        object mediaLock;

        protected object MediaLock
        {
            get { return mediaLock; }
            set { mediaLock = value; }
        }

        int nrGridRows;

        public int NrGridRows
        {
            get { return nrGridRows; }
            set { nrGridRows = value;
            NotifyPropertyChanged();
            }
        }
        int nrGridColumns;

        public int NrGridColumns
        {
            get { return nrGridColumns; }
            set { nrGridColumns = value;
            NotifyPropertyChanged();
            }
        }

        bool isScrollBarEnabled;

        public bool IsScrollBarEnabled
        {
            get { return isScrollBarEnabled; }
            set { isScrollBarEnabled = value;
            NotifyPropertyChanged();
            }
        }

        FilterMode filterMode;

        public virtual FilterMode FilterMode
        {
            get { return filterMode; }
            set { filterMode = value;
            deselectAll();
            NotifyPropertyChanged();
            }
        }

        SortMode sortMode;

        public virtual SortMode SortMode
        {
            get { return sortMode; }
            set { sortMode = value;
            NotifyPropertyChanged();
            }
        }

        public void selectAll()
        {
            lock (mediaLock)
            {
                foreach (MediaFileItem item in Media)
                {
                    item.IsSelected = true;
                }
            }
        }

        public void deselectAll()
        {
            lock (mediaLock)
            {
                foreach (MediaFileItem item in Media)
                {
                    item.IsSelected = false;
                }
            }
        }

    }
}
