using MediaViewer.MediaDatabase;
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

        public ObservableCollection<String> Tags
        {
            get { return (ObservableCollection<String>)GetValue(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        public static readonly DependencyProperty TagsProperty =
            DependencyProperty.Register("Tags", typeof(ObservableCollection<String>), typeof(TagEditorView), 
            new PropertyMetadata(null, new PropertyChangedCallback(tagsChangedCallback)));

        static void tagsChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            TagEditorView control = (TagEditorView)o;
                    
            control.tagListBox.ItemsSource = (ObservableCollection<String>)e.NewValue;
                     
        }

        private void addTag(string tagName)
        {
            if (String.IsNullOrEmpty(tagName) || String.IsNullOrWhiteSpace(tagName)) return;

            if (!Tags.Contains(tagName))
            {
                Tags.Add(tagName);
            }
            
            using (TagDbCommands tc = new TagDbCommands(null))
            {
                Tag tag = tc.getTagByName(tagName);
             
                foreach (Tag linkedTag in tag.LinkedTags)
                {
                    if (!Tags.Contains(linkedTag.Name))
                    {
                        Tags.Add(linkedTag.Name);
                    }
                }                

            }

            addTagTextbox.Text = "";
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            addTag(addTagTextbox.Text);
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            while (tagListBox.SelectedItems.Count > 0)
            {
                Tags.Remove((String)tagListBox.SelectedItem);
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
