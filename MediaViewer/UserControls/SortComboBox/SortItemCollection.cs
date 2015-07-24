using MediaViewer.Model.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.SortComboBox
{
    public class SortItemCollection<T,X> : LockedObservableCollection<T> where T : SortItemBase<X>
    {
        public SortItemCollection()
        {

        }

        public event EventHandler ItemSortDirectionChanged;

    
        protected override void afterItemAdded(T item)
        {
            item.SortDirectionChanged += item_SortDirectionChanged; 
        }

        void item_SortDirectionChanged(object sender, EventArgs e)
        {
            if (ItemSortDirectionChanged != null)
            {
                ItemSortDirectionChanged(sender, EventArgs.Empty);
            }
        }

        protected override void beforeItemRemoved(T item)
        {
            item.SortDirectionChanged -= item_SortDirectionChanged;
        }
    }
}
