using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Progress;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.MediaFileModel
{
    public enum MediaStateType
    {
        Directory,
        SearchResult,
        Other
    }

    interface IMediaState : INotifyPropertyChanged
    {
        event EventHandler<MediaStateChangedEventArgs> NrItemsInStateChanged;
        event EventHandler<MediaStateChangedEventArgs> NrImportedItemsChanged;
        event EventHandler ItemIsSelectedChanged;
        event EventHandler<PropertyChangedEventArgs> ItemPropertiesChanged;

        MediaStateType MediaStateType
        {
            get;
        }

        String MediaStateInfo
        {
            get;
        }

        DateTime MediaStateDateTime
        {
            get;
        }

        MediaLockedCollection UIMediaCollection
        {
            get;
        }

        MediaLockedCollection UISelectedMedia
        {
            get;
        }

        int NrLoadedItemsInState
        {
            get;
        }

        int NrItemsInState
        {
            get;
        }
       
        void selectAllUIState();
        void deselectAllUIState();
        List<MediaFileItem> getSelectedItemsUIState();
      
        void addUIState(IEnumerable<MediaFileItem> items);
        void removeUIState(IEnumerable<MediaFileItem> items);
        void renameUIState(IEnumerable<MediaFileItem> oldItems, IEnumerable<String> newLocations);
        void clearUIState(String stateInfo, DateTime stateDateTime, MediaStateType stateType);

        void delete(IEnumerable<MediaFileItem> items, CancellationToken token);
        void move(MediaFileItem item, String location, ICancellableOperationProgress progress);
        void move(IEnumerable<MediaFileItem> items, IEnumerable<String> locations, ICancellableOperationProgress progress);

        void import(MediaFileItem item, CancellationToken token);
        void import(IEnumerable<MediaFileItem> items, CancellationToken token);
        void export(MediaFileItem item, CancellationToken token);
        void export(IEnumerable<MediaFileItem> items, CancellationToken token);

        void readMetadataRangeAsync(int start, int nrItems, CancellationToken token);
        void readMetadata(MediaFileItem item, MediaFactory.ReadOptions options, CancellationToken token);
        void readMetadata(IEnumerable<MediaFileItem> items, MediaFactory.ReadOptions options, CancellationToken token);
        void writeMetadata(MediaFileItem item, MediaFactory.WriteOptions options, ICancellableOperationProgress progress);
        void writeMetadata(IEnumerable<MediaFileItem> items, MediaFactory.WriteOptions options, ICancellableOperationProgress progress);    
    }
}
