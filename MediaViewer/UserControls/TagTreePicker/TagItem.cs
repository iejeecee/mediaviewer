using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.TagTreePicker
{
    public class TagItem :  BindableBase, IComparable<TagItem>, IEquatable<TagItem>
    {
        Tag tag;

        public Tag Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        public TagItem(String name, int used, String category)
        {           
            Name = name;
            Count = used;
            Category = category;
            Tag = new Tag();
        }

        public TagItem(Tag tag)
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

        public int CompareTo(TagItem other)
        {
            if (other == null)
            {
                throw new ArgumentException();
            }

            return (other.Name.CompareTo(Name));
        }

        public bool Equals(TagItem other)
        {
        
            return (other.Tag.Id == Tag.Id);
        }

        public override string ToString()
        {
            return Name;
        }

    }
}
