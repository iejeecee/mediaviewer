using MediaViewer.Model.Media.Base.Item;
using MediaViewer.Model.Media.File;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.Base.State.CollectionView
{
    public class SelectableMediaItem : BindableBase, IComparable<SelectableMediaItem>, IEquatable<SelectableMediaItem>
    {
        public event EventHandler SelectionChanged;
        public event PropertyChangedEventHandler MediaItemPropertyChanged;

        public SelectableMediaItem(MediaItem item)
        {
            Item = item;
            IsSelected = false;

            item.PropertyChanged += item_PropertyChanged;
        }

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (MediaItemPropertyChanged != null)
            {
                MediaItemPropertyChanged(this, e);
            }
        }

        MediaItem item;

        public MediaItem Item
        {
            get { return item; }
            set {
                SetProperty(ref item, value);               
            }
        }

        bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }            
            set {

                if (value != isSelected)
                {
                    SetProperty(ref isSelected, value);                 
                    OnSelectionChanged();
                }
            }
        }     

        public bool Equals(SelectableMediaItem other)
        {       
            return (Item.Equals(other.Item));
        }

        public int CompareTo(SelectableMediaItem other)
        {
            if (other == null)
            {
                throw new ArgumentException();
            }

            return (Item.CompareTo(other.Item));
        }

        protected void OnSelectionChanged()
        {

            if (SelectionChanged != null)
            {
                SelectionChanged(this, EventArgs.Empty);
            }
        }
    }
}
