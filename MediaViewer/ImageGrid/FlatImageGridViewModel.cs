using MediaViewer.MediaFileModel;
using MediaViewer.MediaFileModel.Watcher;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.ImageGrid
{
    class FlatImageGridViewModel : ImageGridViewModel
    {

       int sortedItemEnd;

       public FlatImageGridViewModel(IMediaState mediaState) :
            base(mediaState)
        {

            MediaState.NrItemsInStateChanged += new EventHandler<MediaStateChangedEventArgs>(flatImageGridViewModel_NrItemsInStateChanged);
            MediaState.ItemPropertiesChanged += MediaState_ItemPropertiesChanged;

            IsScrollBarEnabled = true;
            NrGridColumns = 4;
            NrGridRows = 0;
           
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
       }

       private void flatImageGridViewModel_NrItemsInStateChanged(object sender, MediaStateChangedEventArgs e)
       {
           lock (MediaLock)
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
                    
                       break;
                   default:
                       break;
               }
              
           }
       }

       public override FilterMode FilterMode
       {
           get
           {
               return (base.FilterMode);
           }
           set
           {
               base.FilterMode = value;              
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

           }
       }

       public override SortMode SortMode
       {
           get
           {
               return (base.SortMode);
           }
           set
           {
               base.SortMode = value;
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

           }
       }

       void reSort(MediaFileItem item)
       {
           lock (MediaLock)
           {
               remove(item);
               add(item);
           }
       }

       void remove(MediaFileItem item)
       {
           lock (MediaLock)
           {
               int index = Media.IndexOf(item);

               if (index < 0) return;

               Media.RemoveAt(index);
               if (index < sortedItemEnd)
               {
                   sortedItemEnd -= 1;
               }
           }
       }

       void add(MediaFileItem item)
       {
           lock (MediaLock)
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
                       (a, b) => {
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
               case SortMode.CreationDate:
                   sortFunc = new Func<MediaFileItem, MediaFileItem, int>(
                       (a, b) =>
                       {
                           int result = hasMediaTest(a, b);
                           if (result != 0) return result;

                           return (Nullable.Compare(a.Media.CreationDate, b.Media.CreationDate));
                       });
                   break;
               default:
                   break;
           }

           return (sortFunc);
       }


       void insertSorted(MediaFileItem item) {

           lock (MediaLock)
           {
               Utils.Misc.insertIntoSortedCollection<MediaFileItem>(Media, item, getSortFunction(), 0, sortedItemEnd);
               
               sortedItemEnd++;               
           }

       }

       void clearMedia()
       {
           lock (MediaLock)
           {
               Media.Clear();
               sortedItemEnd = 0;

               OnCleared();
           }
       }
    }
}
