using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Progress;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.MediaFileModel
{
    interface IMediaState
    {
        event NotifyCollectionChangedEventHandler NrItemsInStateChanged;
        event NotifyCollectionChangedEventHandler NrImportedItemsChanged;
        event EventHandler ItemIsSelectedChanged;
        event EventHandler ItemPropertiesChanged;

        MediaLockedCollection MediaCollection
        {
            get;
        }

        void clear();

        void selectAll();
        void deselectAll();
        List<MediaFileItem> getSelectedItems();
      
        void add(IEnumerable<MediaFileItem> items);
        void remove(IEnumerable<MediaFileItem> items);

        void delete(IEnumerable<MediaFileItem> items, CancellationToken token);
        void move(MediaFileItem item, String location, IProgress progress);
        void move(IEnumerable<MediaFileItem> items, IEnumerable<String> locations, IProgress progress);

        void import(MediaFileItem item, CancellationToken token);
        void import(IEnumerable<MediaFileItem> items, CancellationToken token);
        void export(MediaFileItem item, CancellationToken token);
        void export(IEnumerable<MediaFileItem> items, CancellationToken token);

        void readMetadataRangeAsync(int start, int nrItems, CancellationToken token);
        void readMetadata(MediaFileItem item, MediaFactory.ReadOptions options, CancellationToken token);
        void readMetadata(IEnumerable<MediaFileItem> items, MediaFactory.ReadOptions options, CancellationToken token);
        void writeMetadata(MediaFileItem item, MediaFactory.WriteOptions options, IProgress progress);
        void writeMetadata(IEnumerable<MediaFileItem> items, MediaFactory.WriteOptions options, IProgress progress);

        bool isInUse(MediaFileItem item);
    }
}
