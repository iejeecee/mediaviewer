using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.Base.State.CollectionView;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Filter
{
    public class TagItem :  BindableBase
    {
        public event EventHandler IsFilterChanged;

        Tag tag;

        public Tag Tag
        {
            get { return tag; }
            set { tag = value; }
        }
     
        public TagItem(Tag tag, MediaStateCollectionView mediaCollectionView)
        {
            this.tag = tag;
            Name = tag.Name;       
            Count = 1;

            if (tag.TagCategory != null)
            {
                Category = tag.TagCategory.Name;
            }
            else
            {
                Category = "None";
            }

            TagItem item = mediaCollectionView.TagFilter.Find((i) => i.Name.Equals(Name));

            if (item != null)
            {
                if(item.IsIncluded) IsIncluded = true;
                if(item.IsExcluded) IsExcluded = true;
            }
        }

        string name;

        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        string category;

        public string Category
        {
            get { return category; }
            set { SetProperty(ref category, value); }
        }

        int count;

        public int Count
        {
            get
            {
                return count;
            }

            set
            {
                SetProperty(ref count, value);
            }
        }

        bool isIncluded;

        public bool IsIncluded
        {
            get { return isIncluded; }
            set
            {
                if (value == isIncluded) return;

                if (value == true)
                {
                    isExcluded = false;
                    OnPropertyChanged("IsExcluded");
                }

                isIncluded = value;
                OnPropertyChanged("IsIncluded");

                if (IsFilterChanged != null)
                {
                    IsFilterChanged(this, EventArgs.Empty);
                }
            }
        }

        bool isExcluded;

        public bool IsExcluded
        {
            get { return isExcluded; }
            set
            {
                if (value == isExcluded) return;

                if (value == true)
                {
                    isIncluded = false;
                    OnPropertyChanged("IsIncluded");
                }

                isExcluded = value;
                OnPropertyChanged("IsExcluded");

                if (IsFilterChanged != null)
                {
                    IsFilterChanged(this, EventArgs.Empty);
                }
            }
        }        

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return((obj as TagItem).Name.Equals(Name));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
