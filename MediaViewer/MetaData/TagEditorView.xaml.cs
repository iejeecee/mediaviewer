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
                    
            control.tagListBox.ItemsSource = (ObservableCollection<Tag>)e.NewValue;
                     
        }

        private void addTag(string tagName)
        {
            if (String.IsNullOrEmpty(tagName) || String.IsNullOrWhiteSpace(tagName)) return;
         
            addTagTextbox.Text = "";

            // add linked tags

            using (TagDbCommands tc = new TagDbCommands(null))
            {
                Tag tag = tc.getTagByName(tagName);

                if (tag != null)
                {
                    foreach (Tag childTag in tag.ChildTags)
                    {
                        if (!Tags.Contains(childTag))
                        {
                            Tags.Add(childTag);
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
                    Tags.Add(tag);
                }
            }           
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            addTag(addTagTextbox.Text);
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            while (tagListBox.SelectedItems.Count > 0)
            {
                Tags.Remove((Tag)tagListBox.SelectedItem);
            }  
        }

        private void addTagTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                addTag(addTagTextbox.Text);
            }

        }
        
    }
}
