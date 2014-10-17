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
    public class SelectableMediaLockedCollection : LockedObservableCollection<SelectableMediaFileItem>
    {
        override protected void addItemPropertyChangedListener(SelectableMediaFileItem item)
        {
            item.Item.PropertyChanged += item_PropertyChanged;
        }

        override protected void removeItemPropertyChangedListener(SelectableMediaFileItem item)
        {
            item.Item.PropertyChanged -= item_PropertyChanged;
        }
    }
}
