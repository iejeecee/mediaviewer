using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.Model.Media.State.CollectionView
{
    public sealed class DefaultMediaStateCollectionView : MediaStateCollectionView
    {
        public DefaultMediaStateCollectionView(IMediaState mediaState)
            : base(mediaState)
        {
            filter = MediaStateFilterFunctions.getFilter(MediaStateFilterMode.All);
            sortFunc = MediaStateSortFunctions.getSortFunction(MediaStateSortMode.Name);

            FilterModes = new ListCollectionView(Enum.GetValues(typeof(MediaStateFilterMode)));         
            SortModes = new ListCollectionView(Enum.GetValues(typeof(MediaStateSortMode)));

            SortModes.CurrentChanged += (s, e) =>
            {
                MediaStateSortMode sortMode = (MediaStateSortMode)SortModes.CurrentItem;

                SortFunc = MediaStateSortFunctions.getSortFunction(sortMode);        
            };

            SortModes.Filter = mediaStateSortModeCollectionViewFilter;

            FilterModes.CurrentChanged += (s, e) =>
            {
                MediaStateFilterMode filterMode = (MediaStateFilterMode)FilterModes.CurrentItem;
                MediaStateSortMode sortMode = (MediaStateSortMode)SortModes.CurrentItem;

                Filter = MediaStateFilterFunctions.getFilter(filterMode);

                SortModes.Refresh();

                switch (filterMode)
                {
                    case MediaStateFilterMode.All:
                        if (!MediaStateSortFunctions.isAllSortMode(sortMode))
                        {
                            SortModes.MoveCurrentToFirst();
                        }
                        break;
                    case MediaStateFilterMode.Video:
                        if (!MediaStateSortFunctions.isVideoSortMode(sortMode))
                        {
                            SortModes.MoveCurrentToFirst();
                        }
                        break;
                    case MediaStateFilterMode.Images:
                        if (!MediaStateSortFunctions.isImageSortMode(sortMode))
                        {
                            SortModes.MoveCurrentToFirst();
                        }
                        break;
                    default:
                        break;
                }

       
            };
                                    
            FilterModes.MoveCurrentTo(MediaStateFilterMode.All);
            
        }

        override protected void MediaState_ItemPropertiesChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MediaFileItem item = sender as MediaFileItem;
            MediaStateSortMode sortMode = (MediaStateSortMode)SortModes.CurrentItem;

            if (e.PropertyName.Equals("Location"))
            {
                if (sortMode == MediaStateSortMode.Name)
                {
                    reSort(item);
                }
            }
            else if (e.PropertyName.Equals("Media"))
            {
                if (item.Media != null && sortMode != MediaStateSortMode.Name)
                {
                    reSort(item);
                }
            }
            
        }


        public ListCollectionView FilterModes { get; set; }    
        public ListCollectionView SortModes  {get;set;}
      
        private bool mediaStateSortModeCollectionViewFilter(object item)
        {
            MediaStateSortMode mode = (MediaStateSortMode)item;

            switch ((MediaStateFilterMode)FilterModes.CurrentItem)
            {
                case MediaStateFilterMode.All:
                    return (MediaStateSortFunctions.isAllSortMode(mode));
                case MediaStateFilterMode.Video:
                    return (MediaStateSortFunctions.isVideoSortMode(mode));
                case MediaStateFilterMode.Images:
                    return (MediaStateSortFunctions.isImageSortMode(mode));
                default:
                    break;
            }

            return (false);
        }
            
    }
}
