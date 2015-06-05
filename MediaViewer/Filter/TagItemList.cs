using MediaViewer.MediaDatabase;
using MediaViewer.Model.Collections;
using MediaViewer.Model.Collections.Sort;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Filter
{
    public class TagItemList : LockedObservableCollection<TagItem>
    {
        public event EventHandler IsFilterChanged;

        public List<TagItem> tagItems;
        
        void tagItem_IsFilterChanged(object sender, EventArgs e)
        {
            if (IsFilterChanged != null)
            {
                IsFilterChanged(sender, EventArgs.Empty);
            }
        }

        protected override void afterItemAdded(TagItem item)
        {            
            base.afterItemAdded(item);
            item.IsFilterChanged += tagItem_IsFilterChanged;
        }

        protected override void beforeItemRemoved(TagItem item)
        {
            base.beforeItemRemoved(item);
            item.IsFilterChanged -= tagItem_IsFilterChanged;
        }
       
    }
}
