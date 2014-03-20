using Aga.Controls.Tree;
using MediaViewer.MediaDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaViewer.UserControls.TagTreePicker
{
    /// <summary>
    /// Interaction logic for TagTreePickerView.xaml
    /// </summary>
    public partial class TagTreePickerView : UserControl
    {
        public TagTreePickerView()
        {
            InitializeComponent();
            tagTreeList.Model = new TagTreePickerViewModel();
        }

        public Tag SelectedTag
        {
            get { return (Tag)GetValue(SelectedTagProperty); }
            set { SetValue(SelectedTagProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedTag.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedTagProperty =
            DependencyProperty.Register("SelectedTag", typeof(Tag), typeof(TagTreePickerView), new PropertyMetadata(null));


        public TagCategory SelectedCategory
        {
            get { return (TagCategory)GetValue(SelectedCategoryProperty); }
            set { SetValue(SelectedCategoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedCategory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedCategoryProperty =
            DependencyProperty.Register("SelectedCategory", typeof(TagCategory), typeof(TagTreePickerView), new PropertyMetadata(null));
                
        private void tagTreeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tagTreeList.SelectedNode == null)
            {
                SelectedTag = null;
                SelectedCategory = null;
                return;
            }

            TagTreePickerItem item = (tagTreeList.SelectedNode as TreeNode).Tag as TagTreePickerItem;

            if (item is TagItem)
            {
                SelectedTag = (item as TagItem).Tag;
            }
            else if (item is CategoryItem)
            {
                SelectedCategory = (item as CategoryItem).Category;
            }
        }
    }
}
