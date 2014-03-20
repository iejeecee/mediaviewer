using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.UserControls.AutoCompleteBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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

namespace MediaViewer.UserControls.TagPicker
{
    /// <summary>
    /// Interaction logic for TagPickerView.xaml
    /// </summary>
    public partial class TagPickerView : UserControl
    {
        public TagPickerView()
        {
         
            InitializeComponent();

            addTagAutoCompleteBox.CustomFindMatchesFunction = new UserControls.AutoCompleteBox.AutoCompleteBoxView.CustomFindMatchesDelegate((text) =>
            {
                List<Tag> results = new List<Tag>();

                using (TagDbCommands tc = new TagDbCommands())
                {
                    results = tc.getTagAutocompleteMatches(text);
                }

                return (results.Cast<Object>().ToList());
            });

            DependencyPropertyDescriptor.FromProperty(AutoCompleteBoxView.TextProperty, typeof(AutoCompleteBoxView)).AddValueChanged(addTagAutoCompleteBox,addTagAutoCompleteBox_TextChanged);
            DependencyPropertyDescriptor.FromProperty(AutoCompleteBoxView.SelectedItemProperty, typeof(AutoCompleteBoxView)).AddValueChanged(addTagAutoCompleteBox, addTagAutoCompleteBox_SelectedItemChanged);

            addButton.IsEnabled = false;
        }
      
        ~TagPickerView()
        {
            DependencyPropertyDescriptor.FromProperty(AutoCompleteBoxView.TextProperty, typeof(AutoCompleteBoxView)).RemoveValueChanged(addTagAutoCompleteBox, addTagAutoCompleteBox_TextChanged);
            DependencyPropertyDescriptor.FromProperty(AutoCompleteBoxView.TextProperty, typeof(AutoCompleteBoxView)).RemoveValueChanged(addTagAutoCompleteBox, addTagAutoCompleteBox_SelectedItemChanged);            
        }

        private void addTagAutoCompleteBox_SelectedItemChanged(object sender, EventArgs e)
        {
            if (addTagAutoCompleteBox.SelectedItem != null && AcceptOnlyExistingTags == true)
            {
                addButton.IsEnabled = true;
            }
            else if (addTagAutoCompleteBox.SelectedItem == null && AcceptOnlyExistingTags == true)
            {
                addButton.IsEnabled = false;
            }
        }

        void addTagAutoCompleteBox_TextChanged(Object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(addTagAutoCompleteBox.Text) || String.IsNullOrWhiteSpace(addTagAutoCompleteBox.Text))
            {
                addButton.IsEnabled = false;

            } else if(AcceptOnlyExistingTags == false) {

                addButton.IsEnabled = true;
            } else if(addTagAutoCompleteBox.SelectedItem != null && AcceptOnlyExistingTags == true)
            {
                if ((addTagAutoCompleteBox.SelectedItem as Tag).Name.Equals(addTagAutoCompleteBox.Text))
                {
                    addButton.IsEnabled = true;
                }
                else
                {
                    addButton.IsEnabled = false;
                }                
            }
                                   
        }

        public ObservableCollection<Tag> Tags
        {
            get { return (ObservableCollection<Tag>)GetValue(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        public static readonly DependencyProperty TagsProperty =
            DependencyProperty.Register("Tags", typeof(ObservableCollection<Tag>), typeof(TagPickerView), 
            new PropertyMetadata(null, new PropertyChangedCallback(tagsChangedCallback)));

        static void tagsChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            TagPickerView control = (TagPickerView)o;
                    
            ObservableCollection<Tag> tags = (ObservableCollection<Tag>)e.NewValue;
            control.tagListBox.ItemsSource = tags;
                 
        }

        public bool AddLinkedTags
        {
            get { return (bool)GetValue(AddLinkedTagsProperty); }
            set { SetValue(AddLinkedTagsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AddLinkedTags.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddLinkedTagsProperty =
            DependencyProperty.Register("AddLinkedTags", typeof(bool), typeof(TagPickerView), new PropertyMetadata(true));


        public bool AcceptOnlyExistingTags
        {
            get { return (bool)GetValue(AcceptOnlyExistingTagsProperty); }
            set { SetValue(AcceptOnlyExistingTagsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AcceptOnlyExistingTags.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AcceptOnlyExistingTagsProperty =
            DependencyProperty.Register("AcceptOnlyExistingTags", typeof(bool), typeof(TagPickerView), new PropertyMetadata(false));

        
        private void addTag(string tagName)
        {
            if (String.IsNullOrEmpty(tagName) || String.IsNullOrWhiteSpace(tagName)) return;
            if (AcceptOnlyExistingTags == true)
            {
                tagName = (addTagAutoCompleteBox.SelectedItem as Tag).Name;
            }

            addTagAutoCompleteBox.Text = "";

            // add linked tags

            using (TagDbCommands tc = new TagDbCommands(null))
            {
                Tag tag = tc.getTagByName(tagName);

                if (tag != null)
                {
                    if (AddLinkedTags)
                    {
                        foreach (Tag childTag in tag.ChildTags)
                        {
                            if (!Tags.Contains(childTag))
                            {
                                Tags.Add(childTag);
                            }
                        }
                    }
                }
                else
                {
                    tag = new Tag();
                    tag.Name = tagName;
                }
              
                if (!Tags.Contains(tag))
                {
                    Utils.Misc.insertIntoSortedCollection<Tag>(Tags, tag);
                }
            }           
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            addTag(addTagAutoCompleteBox.Text);
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            while (tagListBox.SelectedItems.Count > 0)
            {
                Tags.Remove((Tag)tagListBox.SelectedItem);
            }  
        }

        private void addTagAutoCompleteBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (addButton.IsEnabled == true)
                {
                    addTag(addTagAutoCompleteBox.Text);
                }
            }

        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            Tags.Clear();
        }

        private void tagButton_Click(object sender, RoutedEventArgs e)
        {
           Tag tag = (sender as Button).Tag as Tag;

           Tags.Remove(tag);
        }

        
        
    }
}
