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
using System.ComponentModel;
using MediaViewer.MediaDatabase;
using Microsoft.Practices.Prism.Regions;
using System.Windows.Input;
using MediaViewer.Pager;
using MediaViewer.MetaData;

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
        MimeType,
        CreationDate,
        FileDate,
        LastModified,
        SoftWare,
        Width,
        Height,
        // video
        Duration,
        FramesPerSecond,
        VideoCodec,
        AudioCodec,
        PixelFormat,
        BitsPerSample,
        SamplesPerSecond,
        NrChannels,
        // image
        CameraMake,
        CameraModel,
        Lens,
        FocalLength,
        ExposureTime,
        FNumber,
        ISOSpeedRating
    }
    
    public class ImageGridViewModel : ObservableObject, IPageable, ISelectedMedia
    {

        IMediaState mediaState;

        public IMediaState MediaState
        {
            get
            {
                return (mediaState);
            }
        }

        IRegionManager regionManager;

        public IRegionManager RegionManager
        {
            get { return regionManager; }            
        }
    
        int sortedItemEnd;
                  
        public ImageGridViewModel(IMediaState mediaState, IRegionManager regionManager)
        {
            if (mediaState == null)
            {
                throw new ArgumentException("mediaState cannot be null");
            }

            this.mediaState = mediaState;
            this.regionManager = regionManager;
                      
            NrGridColumns = 4;

            Media = new ObservableCollection<MediaFileItem>();      
            BindingOperations.EnableCollectionSynchronization(Media, Media);

            filterMode = ImageGrid.FilterMode.All;
            sortMode = ImageGrid.SortMode.Name;

            SortModes = CollectionViewSource.GetDefaultView(Enum.GetValues(typeof(SortMode)));
            SortModes.Filter = sortModeCollectionViewFilter;

            SelectedMedia = new ObservableCollection<MediaFileItem>();
            BindingOperations.EnableCollectionSynchronization(SelectedMedia, SelectedMedia);
          
            MediaState.NrItemsInStateChanged += new EventHandler<MediaStateChangedEventArgs>(ImageGridViewModel_NrItemsInStateChanged);
            MediaState.ItemPropertiesChanged += MediaState_ItemPropertiesChanged;
     
            NextPageCommand = new Command(() => { });
            PrevPageCommand = new Command(() => { });
            FirstPageCommand = new Command(() => { });
            LastPageCommand = new Command(() => { });

            IsPagingEnabled = false;
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
                 
        ObservableCollection<MediaFileItem> media;

        public ObservableCollection<MediaFileItem> Media
        {
            get { return media; }
            set { media = value; }
        }

        ObservableCollection<MediaFileItem> selectedMedia;

        public ObservableCollection<MediaFileItem> SelectedMedia
        {
            set { selectedMedia = value; }
            get { return selectedMedia; }
        }
          
        int nrGridColumns;

        public int NrGridColumns
        {
            get { return nrGridColumns; }
            set { nrGridColumns = value;
            NotifyPropertyChanged();
            }
        }

        FilterMode filterMode;

        public virtual FilterMode FilterMode
        {
            get { return filterMode; }
            set
            {
                filterMode = value;

                deselectAll();                
                SortModes.Refresh();

                switch (filterMode)
                {
                    case FilterMode.All:
                        if (!isAllSortMode(SortMode))
                        {
                            SortMode = ImageGrid.SortMode.Name;
                        }
                        break;
                    case FilterMode.Video:
                        if (!isVideoSortMode(SortMode))
                        {
                            SortMode = ImageGrid.SortMode.Name;
                        }
                        break;
                    case FilterMode.Images:
                        if (!isImageSortMode(SortMode))
                        {
                            SortMode = ImageGrid.SortMode.Name;
                        }
                        break;
                    default:
                        break;
                }

                MediaState.UIMediaCollection.EnterReaderLock();
                try
                {
                    clearMedia();

                    OnCleared();

                    foreach (MediaFileItem item in MediaState.UIMediaCollection.Items)
                    {
                        add(item);
                    }
                }
                finally
                {
                    MediaState.UIMediaCollection.ExitReaderLock();
                }

                NotifyPropertyChanged();
            }
        }

        SortMode sortMode;

        public virtual SortMode SortMode
        {
            get { return sortMode; }
            set
            {
                sortMode = value;

                MediaState.UIMediaCollection.EnterReaderLock();
                try
                {
                    clearMedia();

                    foreach (MediaFileItem item in MediaState.UIMediaCollection.Items)
                    {
                        add(item);
                    }
                }
                finally
                {
                    MediaState.UIMediaCollection.ExitReaderLock();
                }

                NotifyPropertyChanged();
            }
        }

        public void selectAll()
        {
            lock (Media)
            {
                foreach (MediaFileItem item in Media)
                {
                    if (item.ItemState == MediaFileItemState.LOADED)
                    {
                        item.IsSelected = true;
                    }
                }
            }
        }

        public void deselectAll()
        {
            lock (Media)
            {
                foreach (MediaFileItem item in Media)
                {
                    item.IsSelected = false;
                }
            }
        }

        public event EventHandler Cleared;

        protected void OnCleared() {

            if (Cleared != null)
            {
                Cleared(this, EventArgs.Empty);
            }
        }

        ICollectionView sortModes;

        public ICollectionView SortModes
        {
            get { return sortModes; }
            set { sortModes = value; }
        }

        static bool isAllSortMode(SortMode mode)
        {
            if ((int)mode <= (int)SortMode.SoftWare) return (true);
            else return (false); 
        }

        static bool isVideoSortMode(SortMode mode)
        {
            if ((int)mode <= (int)SortMode.NrChannels) return (true);
            else return (false); 
        }

        static bool isImageSortMode(SortMode mode)
        {
            if ((int)mode <= (int)SortMode.Height || (int)mode >= (int)SortMode.CameraMake) return (true);
            else return (false);      
        }

        private bool sortModeCollectionViewFilter(object item)
        {
            SortMode mode = (SortMode)item; 

            switch (FilterMode)
            {
                case FilterMode.All:
                    return (isAllSortMode(mode));             
                case FilterMode.Video:
                    return (isVideoSortMode(mode));                
                case FilterMode.Images:
                    return (isImageSortMode(mode));                  
                default:
                    break;
            }

            return (false);
        }

        static int hasMediaTest(MediaFileItem a, MediaFileItem b)
        {
            if (a.Media != null && b.Media != null) return 0;
            if (a.Media == null) return 1;
            else return -1;
        }

        Func<MediaFileItem, MediaFileItem, int> getSortFunction()
        {

            Func<MediaFileItem, MediaFileItem, int> sortFunc = null;

            switch (SortMode)
            {
                case SortMode.Name:

                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = System.IO.Path.GetFileName(a.Location).CompareTo(System.IO.Path.GetFileName(b.Location));
                            return (result);
                        });
                    break;
                case SortMode.Size:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (a.Media.SizeBytes.CompareTo(b.Media.SizeBytes));
                        });
                    break;
                case SortMode.Rating:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (Nullable.Compare(a.Media.Rating, b.Media.Rating));
                        });
                    break;
                case SortMode.Imported:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (a.Media.IsImported.CompareTo(b.Media.IsImported));
                        });
                    break;
                case SortMode.Tags:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (a.Media.Tags.Count.CompareTo(b.Media.Tags.Count));
                        });
                    break;
                case SortMode.FileDate:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                       (a, b) =>
                       {
                           int result = hasMediaTest(a, b);
                           if (result != 0) return result;

                           return (a.Media.FileDate.CompareTo(b.Media.FileDate));
                       });
                    break;
                case SortMode.MimeType:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (a.Media.MimeType.CompareTo(b.Media.MimeType));
                        });
                    break;
                case SortMode.LastModified:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (a.Media.LastModifiedDate.CompareTo(b.Media.LastModifiedDate));
                        });
                    break;
                case SortMode.CreationDate:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (Nullable.Compare(a.Media.CreationDate, b.Media.CreationDate));
                        });
                    break;
                case SortMode.SoftWare:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            return (Compare(a.Media.Software, b.Media.Software));
                        });
                    break;
                case SortMode.Width:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            int widthA;
                            int widthB;

                            if (a.Media is ImageMedia)
                            {
                                widthA = (a.Media as ImageMedia).Width;
                            }
                            else
                            {
                                widthA = (a.Media as VideoMedia).Width;
                            }

                            if (b.Media is ImageMedia)
                            {
                                widthB = (b.Media as ImageMedia).Width;
                            }
                            else
                            {
                                widthB = (b.Media as VideoMedia).Width;
                            }

                            return (widthA.CompareTo(widthB));
                        });
                    break;
                case SortMode.Height:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            int heightA;
                            int heightB;

                            if (a.Media is ImageMedia)
                            {
                                heightA = (a.Media as ImageMedia).Height;
                            }
                            else
                            {
                                heightA = (a.Media as VideoMedia).Height;
                            }

                            if (b.Media is ImageMedia)
                            {
                                heightB = (b.Media as ImageMedia).Height;
                            }
                            else
                            {
                                heightB = (b.Media as VideoMedia).Height;
                            }

                            return (heightA.CompareTo(heightB));
                        });
                    break;
                case SortMode.Duration:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<int> aDuration = a.Media is VideoMedia ? new Nullable<int>((a.Media as VideoMedia).DurationSeconds) : null;
                            Nullable<int> bDuration = b.Media is VideoMedia ? new Nullable<int>((b.Media as VideoMedia).DurationSeconds) : null;

                            return (Nullable.Compare<int>(aDuration, bDuration));
                        });
                    break;
                case SortMode.FramesPerSecond:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<double> aFPS = a.Media is VideoMedia ? new Nullable<double>((a.Media as VideoMedia).FramesPerSecond) : null;
                            Nullable<double> bFPS = b.Media is VideoMedia ? new Nullable<double>((b.Media as VideoMedia).FramesPerSecond) : null;

                            return (Nullable.Compare<double>(aFPS, bFPS));
                        });
                    break;
                case SortMode.VideoCodec:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aVideoCodec = a.Media is VideoMedia ? (a.Media as VideoMedia).VideoCodec : "";
                            String bVideoCodec = b.Media is VideoMedia ? (b.Media as VideoMedia).VideoCodec : "";

                            return (aVideoCodec.CompareTo(bVideoCodec));
                        });
                    break;
                case SortMode.AudioCodec:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aAudioCodec = a.Media is VideoMedia ? (a.Media as VideoMedia).AudioCodec : null;
                            String bAudioCodec = b.Media is VideoMedia ? (b.Media as VideoMedia).AudioCodec : null;

                            return (Compare(aAudioCodec, bAudioCodec));
                        });
                    break;
                case SortMode.PixelFormat:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aPixelFormat = a.Media is VideoMedia ? (a.Media as VideoMedia).PixelFormat : null;
                            String bPixelFormat = b.Media is VideoMedia ? (b.Media as VideoMedia).PixelFormat : null;

                            return (Compare(aPixelFormat, bPixelFormat));
                        });
                    break;
                case SortMode.BitsPerSample:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<short> aBPS = a.Media is VideoMedia ? (a.Media as VideoMedia).BitsPerSample : null;
                            Nullable<short> bBPS = b.Media is VideoMedia ? (b.Media as VideoMedia).BitsPerSample : null;

                            return (Nullable.Compare(aBPS, bBPS));
                        });
                    break;
                case SortMode.SamplesPerSecond:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<int> aSPS = a.Media is VideoMedia ? (a.Media as VideoMedia).SamplesPerSecond : null;
                            Nullable<int> bSPS = b.Media is VideoMedia ? (b.Media as VideoMedia).SamplesPerSecond : null;

                            return (Nullable.Compare(aSPS, bSPS));
                        });
                    break;
                case SortMode.NrChannels:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<int> aNrChannels = a.Media is VideoMedia ? (a.Media as VideoMedia).NrChannels : null;
                            Nullable<int> bNrChannels = b.Media is VideoMedia ? (b.Media as VideoMedia).NrChannels : null;

                            return (Nullable.Compare(aNrChannels, bNrChannels));
                        });
                    break;
                case SortMode.CameraMake:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aVal = a.Media is ImageMedia ? (a.Media as ImageMedia).CameraMake : null;
                            String bVal = b.Media is ImageMedia ? (b.Media as ImageMedia).CameraMake : null;

                            return (Compare(aVal, bVal));
                        });
                    break;
                case SortMode.CameraModel:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aVal = a.Media is ImageMedia ? (a.Media as ImageMedia).CameraModel : null;
                            String bVal = b.Media is ImageMedia ? (b.Media as ImageMedia).CameraModel : null;

                            return (Compare(aVal, bVal));
                        });
                    break;
                case SortMode.Lens:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            String aVal = a.Media is ImageMedia ? (a.Media as ImageMedia).Lens : null;
                            String bVal = b.Media is ImageMedia ? (b.Media as ImageMedia).Lens : null;

                            return (Compare(aVal, bVal));
                        });
                    break;
                case SortMode.FocalLength:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<double> aVal = a.Media is ImageMedia ? (a.Media as ImageMedia).FocalLength : null;
                            Nullable<double> bVal = b.Media is ImageMedia ? (b.Media as ImageMedia).FocalLength : null;

                            return (Nullable.Compare(aVal, bVal));
                        });
                    break;
                case SortMode.FNumber:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<double> aVal = a.Media is ImageMedia ? (a.Media as ImageMedia).FNumber : null;
                            Nullable<double> bVal = b.Media is ImageMedia ? (b.Media as ImageMedia).FNumber : null;

                            return (Nullable.Compare(aVal, bVal));
                        });
                    break;
                case SortMode.ExposureTime:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<double> aVal = a.Media is ImageMedia ? (a.Media as ImageMedia).ExposureTime : null;
                            Nullable<double> bVal = b.Media is ImageMedia ? (b.Media as ImageMedia).ExposureTime : null;

                            return (Nullable.Compare(aVal, bVal));
                        });
                    break;
                case SortMode.ISOSpeedRating:
                    sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                        (a, b) =>
                        {
                            int result = hasMediaTest(a, b);
                            if (result != 0) return result;

                            Nullable<int> aVal = a.Media is ImageMedia ? (a.Media as ImageMedia).ISOSpeedRating : null;
                            Nullable<int> bVal = b.Media is ImageMedia ? (b.Media as ImageMedia).ISOSpeedRating : null;

                            return (Nullable.Compare(aVal, bVal));
                        });
                    break;
                default:
                    break;
            }

            return (sortFunc);
        }


        public static int Compare<T>(T a, T b) where T : IComparable
        {
            if (a == null) return -1;
            if (b == null) return 1;
            return (a.CompareTo(b));
        }

       void MediaState_ItemPropertiesChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
       {
           MediaFileItem item = sender as MediaFileItem;

           if (e.PropertyName.Equals("Location"))
           {
               if (SortMode == ImageGrid.SortMode.Name)
               {
                   reSort(item);
               }
           }
           else if (e.PropertyName.Equals("Media"))
           {
               if (item.Media != null && SortMode != ImageGrid.SortMode.Name)
               {
                   reSort(item);
               }
           }
           else if (e.PropertyName.Equals("IsSelected"))
           {
               if (item.IsSelected)
               {
                   insertSorted(item, true);
               }
               else
               {
                   remove(item, true);
               }

           }
       }

       private void ImageGridViewModel_NrItemsInStateChanged(object sender, MediaStateChangedEventArgs e)
       {
           lock (Media)
           {
               switch (e.Action)
               {
                   case MediaStateChangedAction.Add:                   
                       foreach (MediaFileItem item in e.NewItems)
                       {
                           add(item);                           
                       }
                       break;               
                   case MediaStateChangedAction.Remove:
                       foreach (MediaFileItem item in e.OldItems)
                       {
                           remove(item);
                       }
                       break;
                   case MediaStateChangedAction.Modified:
                       break;
                   case MediaStateChangedAction.Clear:
                       clearMedia();
                       OnCleared();                      
                       break;
                   default:
                       break;
               }

               if (Media.Count > 0)
               {
                   IsPagingEnabled = true;
               }
               else
               {
                   IsPagingEnabled = false;
               }
           }
       }

         
       void reSort(MediaFileItem item)
       {
           lock (Media)
           {
               remove(item);
               add(item);
           }
       }

       void remove(MediaFileItem item, bool selectedMediaOnly = false)
       {
           lock (Media)
           {
               if (selectedMediaOnly == false)
               {
                   int index = Media.IndexOf(item);

                   if (index < 0) return;

                   Media.RemoveAt(index);
                   if (index < sortedItemEnd)
                   {
                       sortedItemEnd -= 1;
                   }
               }

               lock (SelectedMedia)
               {
                   SelectedMedia.Remove(item);
               }
           }

       }

       void add(MediaFileItem item)
       {
           lock (Media)
           {
               switch (FilterMode)
               {
                   case FilterMode.All:
                       insertSorted(item);
                       break;
                   case FilterMode.Video:
                       if (Utils.MediaFormatConvert.isVideoFile(item.Location))
                       {
                           insertSorted(item);
                       }
                       break;
                   case FilterMode.Images:
                       if (Utils.MediaFormatConvert.isImageFile(item.Location))
                       {
                           insertSorted(item);
                       }
                       break;
                   default:
                       break;
               }
           }
       }
       
       void insertSorted(MediaFileItem item, bool insertInSelectedMediaOnly = false) {

           lock (Media)
           {
               if (insertInSelectedMediaOnly == false)
               {
                   Utils.Misc.insertIntoSortedCollection<MediaFileItem>(Media, item, getSortFunction(), 0, sortedItemEnd);

                   sortedItemEnd++;
               }

               if (item.IsSelected)
               {
                   lock (SelectedMedia)
                   {
                       if (!SelectedMedia.Contains(item))
                       {

                           Utils.Misc.insertIntoSortedCollection<MediaFileItem>(SelectedMedia, item, getSortFunction());
                       }
                   }
               }
           }
                     
       }

       void clearMedia()
       {
           lock (Media)
           {
               Media.Clear();
               sortedItemEnd = 0;

               lock (SelectedMedia)
               {
                   SelectedMedia.Clear();
               }
           }

           
       }

       public int NrPages
       {
           get
           {
               return (0);
           }
           set
           {              
           }
       }

       public int CurrentPage
       {
           get
           {
               return (0);
           }
           set
           {
      
           }
       }

       bool isPagingEnabled;

       public bool IsPagingEnabled
       {
           get
           {
               return (isPagingEnabled);
           }
           set
           {
               isPagingEnabled = value;
               NotifyPropertyChanged();
           }
       }

       Command nextPageCommand;

       public Command NextPageCommand
       {
           get
           {
               return (nextPageCommand);
           }
           set
           {
               nextPageCommand = value;
           }
       }

       Command prevPageCommand;

       public Command PrevPageCommand
       {
           get
           {
               return (prevPageCommand);
           }
           set
           {
               prevPageCommand = value;
           }
       }

       Command firstPageCommand;

       public Command FirstPageCommand
       {
           get
           {
               return (firstPageCommand);
           }
           set
           {
               firstPageCommand = value;
           }
       }

       Command lastPageCommand;

       public Command LastPageCommand
       {
           get
           {
               return (lastPageCommand);
           }
           set
           {
               lastPageCommand = value;
           }
       }
    
    }
}
