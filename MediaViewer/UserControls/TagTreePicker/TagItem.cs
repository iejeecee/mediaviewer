using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.TagTreePicker
{
    class TagItem : TagTreePickerItem, IComparable<TagItem>, IEquatable<TagItem>
    {
        Tag tag;

        public Tag Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        public TagItem(Tag tag)
        {
            this.tag = tag;
            Name = tag.Name;       
            Used = tag.Used;

            LazyLoading = true;
         
        }

        public override long? Used
        {
            get { return Tag.Used; }
            set { }
        }

        public override object Text
        {
            get
            {               
                return Name;
            }
        }

        public override object Icon
        {
            get
            {
                return loadIcon("tag.ico");
            }
        }

        protected override void LoadChildren()
        {
            try
            {
                if (Parent is TagItem == false)
                {
                    using (TagDbCommands tagCommands = new TagDbCommands())
                    {
                        Tag temp = tagCommands.getTagById(Tag.Id);

                        foreach (Tag child in temp.ChildTags)
                        {
                            Children.Add(new TagItem(child));
                        }
                    }
                }

                IsLoaded = true;
            }
            catch
            {
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
            if (other == null)
            {
                throw new ArgumentException();
            }

            return (other.Tag.Id == Tag.Id);
        }

    }
}
