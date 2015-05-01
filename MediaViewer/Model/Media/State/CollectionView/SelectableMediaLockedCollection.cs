using MediaViewer.Model.Collections;
using MediaViewer.Model.Media.State.CollectionView;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.State
{
    public class SelectableMediaLockedCollection : LockedObservableCollection<SelectableMediaItem>
    {
        override protected void afterItemAdded(SelectableMediaItem item)
        {
            item.MediaItemPropertyChanged += item_PropertyChanged;
        }

        override protected void beforeItemRemoved(SelectableMediaItem item)
        {
            item.MediaItemPropertyChanged -= item_PropertyChanged;
        }
    }
}
