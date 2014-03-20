using MediaViewer.MediaDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.TagTreePicker
{
    class TagItem : TagTreePickerItem
    {
        Tag tag;

        public Tag Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        public TagItem(Tag tag, TagTreePickerItem parent = null)
        {
            this.tag = tag;
            Name = tag.Name;
            Parent = parent;
            Used = tag.Used;

            ImageUrl = "pack://application:,,,/Resources/Icons/tag.ico";
        }

    }
}
