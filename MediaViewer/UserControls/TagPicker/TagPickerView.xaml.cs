using MediaViewer.Infrastructure.Logging;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Collections.Sort;
using MediaViewer.Model.Utils;
using MediaViewer.UserControls.AutoCompleteBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        protected 

        static SolidColorBrush unselectedTagColor = new SolidColorBrush(Colors.White);
        static SolidColorBrush parentSelectedTagColor = new SolidColorBrush(Colors.Red);
        static SolidColorBrush childSelectedTagColor = new SolidColorBrush(Colors.Orange);

        List<ToggleButton> selectedTags;
        const string tagClipboardDataFormat = "TagPickerView_TagNames";
        //TagPickerTagCollection tagPickerTags;
      
        public TagPickerView()
        {

            InitializeComponent();

            addTagAutoCompleteBox.CustomFindMatchesFunction = new UserControls.AutoCompleteBox.AutoCompleteBoxView.CustomFindMatchesDelegate((text) =>
            {
                List<Tag> results = new List<Tag>();

                using (TagDbCommands tc = new TagDbCommands())
                {
                    results = tc.getTagAutocompleteMatches(text, 50, IsStartsWithAutoCompleteMode);
                }

                return (results.Cast<Object>().ToList());
            });

            DependencyPropertyDescriptor.FromProperty(AutoCompleteBoxView.TextProperty, typeof(AutoCompleteBoxView)).AddValueChanged(addTagAutoCompleteBox, addTagAutoCompleteBox_TextChanged);
            DependencyPropertyDescriptor.FromProperty(AutoCompleteBoxView.SelectedItemProperty, typeof(AutoCompleteBoxView)).AddValueChanged(addTagAutoCompleteBox, addTagAutoCompleteBox_SelectedItemChanged);

            addButton.IsEnabled = false;
            clearButton.IsEnabled = false;
            selectedTags = new List<ToggleButton>();
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
            } 
            else if(AcceptOnlyExistingTags == false) 
            {
                addButton.IsEnabled = true;
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
            control.selectedTags.Clear();
            
            // WeakEventManager info:
            // http://www.kolls.net/blog/?p=17
            if (e.OldValue != null)
            {
                WeakEventManager<ObservableCollection<Tag>, NotifyCollectionChangedEventArgs>.RemoveHandler(
                   control.tagListBox.ItemsSource as ObservableCollection<Tag>, "CollectionChanged", control.tagPickerTags_CollectionChanged);
            }

            control.tagListBox.ItemsSource = tags;

            if (e.NewValue != null)
            {
                WeakEventManager<ObservableCollection<Tag>, NotifyCollectionChangedEventArgs>.AddHandler(
                   control.tagListBox.ItemsSource as ObservableCollection<Tag>, "CollectionChanged", control.tagPickerTags_CollectionChanged);
            }

            
        }

        private void tagPickerTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ObservableCollection<Tag> tags = (ObservableCollection<Tag>)sender;

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                selectedTags.Clear();
            }

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (Tags.Count == 0)
                {
                    clearButton.IsEnabled = false;                   
                }
                else
                {
                    if (IsReadOnly)
                    {
                        clearButton.IsEnabled = false;
                    }
                    else
                    {
                        clearButton.IsEnabled = true;
                    }
                }
            }));
        }



        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(TagPickerView), new PropertyMetadata(false,isReadOnlyChangedCallback));

        private static void isReadOnlyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TagPickerView view = (TagPickerView)d;

            bool isReadOnly = (bool)e.NewValue;

            view.addButton.IsEnabled = !isReadOnly;
            view.clearButton.IsEnabled = !isReadOnly;
            view.addTagAutoCompleteBox.IsEnabled = !isReadOnly;
            view.tagListBox.IsEnabled = !isReadOnly;
        }
        
        public bool AddLinkedTags
        {
            get { return (bool)GetValue(AddLinkedTagsProperty); }
            set { SetValue(AddLinkedTagsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AddLinkedTags.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddLinkedTagsProperty =
            DependencyProperty.Register("AddLinkedTags", typeof(bool), typeof(TagPickerView), new PropertyMetadata(true));


        public bool IsStartsWithAutoCompleteMode
        {
            get { return (bool)GetValue(IsStartsWithAutoCompleteModeProperty); }
            set { SetValue(IsStartsWithAutoCompleteModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsStartsWithAutoCompleteMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsStartsWithAutoCompleteModeProperty =
            DependencyProperty.Register("IsStartsWithAutoCompleteMode", typeof(bool), typeof(TagPickerView), new PropertyMetadata(false));
        

        public bool AcceptOnlyExistingTags
        {
            get { return (bool)GetValue(AcceptOnlyExistingTagsProperty); }
            set { SetValue(AcceptOnlyExistingTagsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AcceptOnlyExistingTags.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AcceptOnlyExistingTagsProperty =
            DependencyProperty.Register("AcceptOnlyExistingTags", typeof(bool), typeof(TagPickerView), new PropertyMetadata(false));

        public bool EnableLinkingTags
        {
            get { return (bool)GetValue(EnableLinkingTagsProperty); }
            set { SetValue(EnableLinkingTagsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableLinkingTags.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableLinkingTagsProperty =
            DependencyProperty.Register("EnableLinkingTags", typeof(bool), typeof(TagPickerView), new PropertyMetadata(false));
              
        private void addTag(string tagName)
        {
            if (String.IsNullOrEmpty(tagName) || String.IsNullOrWhiteSpace(tagName)) return;

            tagName = tagName.Trim();

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
                    if (AcceptOnlyExistingTags) return;
                                        
                    tag = new Tag();
                    tag.Name = tagName;
                    
                }
                             
                if (!Tags.Contains(tag))
                {
                    CollectionsSort.insertIntoSortedCollection<Tag>(Tags, tag);
                }
            }           
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            addTag(addTagAutoCompleteBox.Text);
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

            if (selectedTags.Count == 0)
            {
                Tags.Clear();
            }
            else
            {
                for (int i = selectedTags.Count - 1; i >= 0; i--)
                {
                    Tags.Remove(selectedTags[i].Tag as Tag);
                    selectedTags.RemoveAt(i);
                }
            }
           
        }
    
        private void contextMenuCut_Click(object sender, RoutedEventArgs e)
        {
            List<String> tagNames = new List<string>();

            if (selectedTags.Count == 0)
            {
                foreach (Tag tag in Tags)
                {
                    tagNames.Add(tag.Name);
                }
                Tags.Clear();
            }
            else
            {
                for (int i = selectedTags.Count - 1; i >= 0; i--)
                {
                    tagNames.Add((selectedTags[i].Tag as Tag).Name);
                    Tags.Remove(selectedTags[i].Tag as Tag);
                    selectedTags.RemoveAt(i);
                }

                tagNames.Reverse();
            }
                                     
            Clipboard.SetData(tagClipboardDataFormat, tagNames);           
        }

        private void contextMenuCopy_Click(object sender, RoutedEventArgs e)
        {
            List<String> tagNames = new List<string>();

            if (selectedTags.Count == 0)
            {
                foreach (Tag tag in Tags)
                {
                    tagNames.Add(tag.Name);
                }
            }
            else
            {
                foreach (ToggleButton button in selectedTags)
                {                   
                    tagNames.Add((button.Tag as Tag).Name);                   
                }

            }
            
            Clipboard.SetData(tagClipboardDataFormat, tagNames);
        }

        private void contextMenuPaste_Click(object sender, RoutedEventArgs e)
        {
            List<String> tagNames = Clipboard.GetData(tagClipboardDataFormat) as List<String>;

            if (tagNames == null) return;

            foreach (String tagName in tagNames)
            {
                addTag(tagName);
            }
        }

        private void contextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (selectedTags.Count == 0)
            {
                contextMenuUnselect.IsEnabled = false;
            }
            else
            {
                contextMenuUnselect.IsEnabled = true;
            }

            if (selectedTags.Count == 1 && EnableLinkingTags)
            {                
                contextMenuUnlink.IsEnabled = true;
            }
            else if (EnableLinkingTags)
            {
                contextMenuUnlink.IsEnabled = false;
            }
            else
            {
                contextMenuUnlink.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (selectedTags.Count < 2 && EnableLinkingTags)
            {
                contextMenuLink.IsEnabled = false;
            }
            else
            {
                if (EnableLinkingTags)
                {
                    contextMenuLink.IsEnabled = true;
                }
                else
                {
                    contextMenuLink.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

            if (Tags.Count > 0)
            {
                contextMenuCopy.IsEnabled = true;
                if(IsReadOnly == false) contextMenuCut.IsEnabled = true;
                else contextMenuCut.IsEnabled = false;
            }
            else
            {
                contextMenuCopy.IsEnabled = false;
                contextMenuCut.IsEnabled = false;               
            }

            if (Clipboard.ContainsData(tagClipboardDataFormat) && IsReadOnly == false)
            {
                contextMenuPaste.IsEnabled = true;
            }
            else
            {
                contextMenuPaste.IsEnabled = false;
            }
        }

        void setSelectedTagColors()
        {
            for (int i = 0; i < selectedTags.Count; i++)
            {
                if (i == 0 && EnableLinkingTags)
                {
                    selectedTags[i].Background = parentSelectedTagColor;
                }
                else
                {
                    selectedTags[i].Background = childSelectedTagColor;
                }
            }
        }

        private void tag_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;

            selectedTags.Add(button);
            setSelectedTagColors();
        }

        private void tag_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;

            selectedTags.Remove(button);

            button.Background = unselectedTagColor;
            setSelectedTagColors();
        }

        void unselectAllTags()
        {
            for (int i = selectedTags.Count - 1; i >= 0; i--)
            {
                selectedTags[i].IsChecked = false;
            }
        }

        void unlinkTags(TagDbCommands tagCommands)
        {
            Tag parent = tagCommands.getTagByName((selectedTags[0].Tag as Tag).Name);

            if (parent == null)
            {
                return;
            }

            tagCommands.Db.Entry(parent).State = EntityState.Modified;

            parent.ChildTags.Clear();

            tagCommands.Db.SaveChanges();
        }
    
        void linkTags(TagDbCommands tagCommands)
        {
            Tag parent = tagCommands.getTagByName((selectedTags[0].Tag as Tag).Name);

            if (parent == null)
            {
                parent = new Tag();
                parent.Name = (selectedTags[0].Tag as Tag).Name;
                tagCommands.Db.Entry(parent).State = EntityState.Added;
            }
            else
            {
                tagCommands.Db.Entry(parent).State = EntityState.Modified;
            }

            parent.ChildTags.Clear();

            for (int i = 1; i < selectedTags.Count; i++)
            {
                Tag tag = tagCommands.getTagByName((selectedTags[i].Tag as Tag).Name);

                if (tag == null)
                {
                    tag = new Tag();
                    tag.Name = (selectedTags[0].Tag as Tag).Name;
                    tagCommands.Db.Entry(tag).State = EntityState.Added;
                }
                else
                {
                    tagCommands.Db.Entry(tag).State = EntityState.Modified;
                }

                parent.ChildTags.Add(tag);
            }

            tagCommands.Db.SaveChanges();
            
        }

        void updateTag(Action<TagDbCommands> function) {

            int nrTries = 3;

            using (TagDbCommands tagCommands = new TagDbCommands())
            {
                do
                {
                    try
                    {
                        function(tagCommands);
                        unselectAllTags();
                        nrTries = 0;
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (nrTries > 0)
                        {
                            nrTries -= 1;
                        }
                        else
                        {
                            Logger.Log.Error("Error updating tag", ex);
                            MessageBox.Show("Error updating tag\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error("Error updating tag", ex);
                        MessageBox.Show("Error updating tag\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                } while (nrTries > 0);
            }
        }

        private void contextMenuUnlink_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTags.Count != 1) return;

            updateTag(unlinkTags);
        }

        private void contextMenuLink_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTags.Count < 2) return;

            updateTag(linkTags);            
        }

        private void contextMenuUnselect_Click(object sender, RoutedEventArgs e)
        {
            unselectAllTags();
        }

        private void tag_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;
            Tag selectedTag = (button.Tag as Tag);

            String tooltip = selectedTag.Name;              

            try
            {
                using (TagDbCommands tagCommands = new TagDbCommands())
                {
                    Tag tag = tagCommands.getTagByName(selectedTag.Name);
                    if (tag == null) return;

                    foreach (Tag child in tag.ChildTags)
                    {
                        tooltip += "\n" + child.Name;
                    }
                }
            }
            finally
            {
                button.ToolTip = tooltip;
            }
        }

    }
}
