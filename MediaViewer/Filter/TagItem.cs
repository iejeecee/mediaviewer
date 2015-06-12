using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Media.File;
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
     
        public TagItem(Tag tag, MediaFileStateCollectionView mediaCollectionView)
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

            if (mediaCollectionView.TagFilter.Contains(this))
            {
                isFilter = true;
            }
            else
            {
                isFilter = false;
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

        bool isFilter;

        public bool IsFilter
        {
            get { return isFilter; }
            set { SetProperty(ref isFilter, value);

                if (IsFilterChanged != null)
                {
                    IsFilterChanged(this,EventArgs.Empty);
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
