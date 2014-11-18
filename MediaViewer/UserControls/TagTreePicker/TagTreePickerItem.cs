using ICSharpCode.TreeView;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MediaViewer.UserControls.TagTreePicker
{
    public class TagTreePickerItem : SharpTreeNode, IComparable<TagTreePickerItem>, IEquatable<TagTreePickerItem>
    {
        protected TagTreePickerItem()
        {
            IsLoaded = false;
            
        }

        String name;

        public String Name
        {
            get { return name; }
            set { name = value;        
            }
        }
                  
        virtual public long? Used
        {
            get { return null; }
            set {}
        }

        bool isLoaded;

        public bool IsLoaded
        {
            get { return isLoaded; }
            set { isLoaded = value; }
        }


        public int CompareTo(TagTreePickerItem other)
        {
            if (other == null)
            {
                throw new NotImplementedException();
            }

            return (other.Name.CompareTo(Name));
        }

        public bool Equals(TagTreePickerItem other)
        {
         
            if (this is TagItem && other is TagItem)
            {
                return ((this as TagItem).Tag.Id == (other as TagItem).Tag.Id);
            }

            if (this is CategoryItem && other is CategoryItem)
            {
                return ((this as CategoryItem).Category.Id == (other as CategoryItem).Category.Id);
            }

            return (false);
        }

        protected static ImageSource loadIcon(string name)
        {
            var frame = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Icons/" + name, UriKind.Absolute));
            return frame;
        }

        public override void DeleteWithoutConfirmation(SharpTreeNode[] nodes)
        {

            foreach (var node in nodes)
            {
                if (node.Parent == null) continue;

                foreach (TagTreePickerItem item in node.Parent.Children)
                {
                    if (item.Equals(node))
                    {
                        node.Parent.Children.Remove(item);
                        return;
                    }
                }
                                                        
            }
        }

        public CategoryItem findCategory(int id)
        {
            foreach (TagTreePickerItem child in Children)
            {
                if (child is CategoryItem && (child as CategoryItem).Category.Id == id) 
                {
                    return (child as CategoryItem);
                }
            }

            return (null);
        }

        public TagItem findTag(int id)
        {
            foreach (TagTreePickerItem child in Children)
            {
                if (child is CategoryItem)
                {
                    TagItem result = child.findTag(id);

                    if (result != null) return (result);
                }
                else if (child is TagItem && (child as TagItem).Tag.Id == id)
                {
                    return (child as TagItem);
                } 
            }

            return (null);
        }
    }
}
