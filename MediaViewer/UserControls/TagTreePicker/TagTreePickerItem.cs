using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.TagTreePicker
{
    class TagTreePickerItem : ObservableObject
    {
        protected TagTreePickerItem()
        {
            Children = new ObservableCollection<TagTreePickerItem>();
            Parent = null;
            Used = 0;
        }

        String name;

        public String Name
        {
            get { return name; }
            set { name = value;
            NotifyPropertyChanged();
            }
        }

        TagTreePickerItem parent;

        public TagTreePickerItem Parent
        {
            get { return parent; }
            set { parent = value;
            NotifyPropertyChanged();
            }
        }

        ObservableCollection<TagTreePickerItem> children;

        public ObservableCollection<TagTreePickerItem> Children
        {
            get { return children; }
            set { children = value; }
        }

        long used;

        public long Used
        {
            get { return used; }
            set { used = value;
            NotifyPropertyChanged();
            }
        }

        private string imageUrl;

        public string ImageUrl
        {
            get { return imageUrl; }
            set
            {
                imageUrl = value;
                NotifyPropertyChanged();
            }
        }
    }
}
