using MediaViewer.MediaDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.TagTreePicker
{
    class CategoryItem : TagTreePickerItem
    {
        TagCategory category;

        public TagCategory Category
        {
            get { return category; }
            set { category = value; }
        }

        bool isAllTagsCategory;

        public bool IsAllTagsCategory
        {
            get { return isAllTagsCategory; }
            set { isAllTagsCategory = value;
            NotifyPropertyChanged();
            }
        }

        public CategoryItem(TagCategory category)
        {

            this.category = category;
            if (category != null)
            {
                Name = category.Name;
            }

            isAllTagsCategory = false;

            ImageUrl = "pack://application:,,,/Resources/Icons/folder_back.ico";
        }
    }
}
