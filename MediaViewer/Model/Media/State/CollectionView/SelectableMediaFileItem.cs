using MediaViewer.Model.Media.File;
using MvvmFoundation.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.State.CollectionView
{
    public class SelectableMediaFileItem : ObservableObject, IComparable<SelectableMediaFileItem>, IEquatable<SelectableMediaFileItem>
    {
        public event EventHandler SelectionChanged;

        public SelectableMediaFileItem(MediaFileItem item)
        {
            Item = item;
            IsSelected = false;         
        }

        MediaFileItem item;

        public MediaFileItem Item
        {
            get { return item; }
            set { item = value;
            NotifyPropertyChanged();
            }
        }

        bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }            
            set {

                if (value != isSelected)
                {
                    isSelected = value;
                    NotifyPropertyChanged();
                    OnSelectionChanged();
                }
            }
        }     

        public bool Equals(SelectableMediaFileItem other)
        {
            if (other == null)
            {
                throw new ArgumentException();
            }

            return (Item.Equals(other.Item));
        }

        public int CompareTo(SelectableMediaFileItem other)
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
