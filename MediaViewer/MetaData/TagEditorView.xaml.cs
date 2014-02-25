using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

namespace MediaViewer.MetaData
{
    /// <summary>
    /// Interaction logic for TagEditorView.xaml
    /// </summary>
    public partial class TagEditorView : UserControl
    {
        public TagEditorView()
        {
         
            InitializeComponent();         
                      
        }

        public ObservableCollection<Tag> Tags
        {
            get { return (ObservableCollection<Tag>)GetValue(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        public static readonly DependencyProperty TagsProperty =
            DependencyProperty.Register("Tags", typeof(ObservableCollection<Tag>), typeof(TagEditorView), 
            new PropertyMetadata(null, new PropertyChangedCallback(tagsChangedCallback)));

        static void tagsChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            TagEditorView control = (TagEditorView)o;
                    
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
            DependencyProperty.Register("AddLinkedTags", typeof(bool), typeof(TagEditorView), new PropertyMetadata(true));
        
        private void addTag(string tagName)
        {
            if (String.IsNullOrEmpty(tagName) || String.IsNullOrWhiteSpace(tagName)) return;

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
                addTag(addTagAutoCompleteBox.Text);              
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

        private void addTagAutoCompleteBox_GotFocus(object sender, RoutedEventArgs e)
        {
            using (TagDbCommands tc = new TagDbCommands())
            {               
                addTagAutoCompleteBox.Items = tc.getAllTags();
            }
        }
        
    }
}
