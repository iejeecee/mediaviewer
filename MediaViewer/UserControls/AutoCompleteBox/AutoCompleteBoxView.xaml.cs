using MediaViewer.MediaFileModel.Watcher;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
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

namespace MediaViewer.UserControls.AutoCompleteBox
{
    /// <summary>
    /// Interaction logic for AutoCompleteBoxView.xaml
    /// </summary>
    public partial class AutoCompleteBoxView : UserControl
    {             
        public AutoCompleteBoxView()
        {
            InitializeComponent();

            tree = new TernaryTree();
            Suggestions = new ObservableRangeCollection<object>();

            autoCompleteTextBox.DataContext = this;

            Binding textBinding = new System.Windows.Data.Binding("Text");
            textBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            textBinding.Mode = BindingMode.TwoWay;

            autoCompleteTextBox.SetBinding(TextBox.TextProperty, textBinding);

            popupItemsControl.DataContext = this;
            popupItemsControl.SetBinding(ItemsControl.ItemsSourceProperty, new System.Windows.Data.Binding("Suggestions"));

            mouseOverPopup = false;
        }

        TernaryTree tree;

        public delegate List<Object> CustomFindMatchesDelegate(String text);
        CustomFindMatchesDelegate customFindMatchesFunction;

        public CustomFindMatchesDelegate CustomFindMatchesFunction
        {
            get { return customFindMatchesFunction; }
            set { customFindMatchesFunction = value; }
        }

        bool mouseOverPopup;

        public IEnumerable<Object> Items
        {
            get { return (IEnumerable<Object>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IEnumerable<Object>), typeof(AutoCompleteBoxView), new PropertyMetadata(null));

     
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(AutoCompleteBoxView), new PropertyMetadata(null, new PropertyChangedCallback(selectedItemChangedCallback)));

        private static void selectedItemChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBoxView view = (AutoCompleteBoxView)d;
            Object item = e.NewValue;

            if (item != null)
            {
                view.Text = item.ToString();
            }
            else
            {              
                view.Text = "";               
            }

            view.popup.IsOpen = false;
            view.autoCompleteTextBox.Focus();
            view.autoCompleteTextBox.CaretIndex = view.Text.Length;
        }
               
        public String Text
        {
            get { return (String)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(String), typeof(AutoCompleteBoxView), new PropertyMetadata(null, new PropertyChangedCallback(textChangedCallback)));

        private static void textChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBoxView view = (AutoCompleteBoxView)d;
            String text = (String)e.NewValue;

            if (!String.IsNullOrEmpty(text))
            {
                view.findSuggestions(text);
                if (view.Suggestions.Count > 0)
                {
                    view.displaySuggestions();
                }
                else
                {
                    view.popup.IsOpen = false;
                }
                             
            }
            else
            {
                view.popup.IsOpen = false;
            }
        }
       
        public int MaxSuggestions
        {
            get { return (int)GetValue(MaxSuggestionsProperty); }
            set { SetValue(MaxSuggestionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxSuggestions.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxSuggestionsProperty =
            DependencyProperty.Register("MaxSuggestions", typeof(int), typeof(AutoCompleteBoxView), new PropertyMetadata(50));

        void displaySuggestions()
        {
            popup.PlacementTarget = autoCompleteTextBox;
            popup.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            popup.MinWidth = autoCompleteTextBox.ActualWidth;
            popup.IsEnabled = true;
            popup.IsOpen = true;
            scrollViewer.ScrollToVerticalOffset(0);
        }

        private void autoCompleteTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!mouseOverPopup)
            {
                popup.IsOpen = false;
            }
        }       

        private void popup_MouseEnter(object sender, MouseEventArgs e)
        {
            mouseOverPopup = true;
        }

        private void popup_MouseLeave(object sender, MouseEventArgs e)
        {
            mouseOverPopup = false;
        }

        private void autoCompleteTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                if (Suggestions.Count == 0)
                {
                    SystemSounds.Beep.Play();
                }
                else
                {
                    SelectedItem = Suggestions[0];
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (Suggestions.Count > 0 && Suggestions[0].ToString().Equals(Text))
                {
                    SelectedItem = Suggestions[0];
                }
            }
        }

        ObservableRangeCollection<Object> suggestions;

        public ObservableRangeCollection<Object> Suggestions
        {
            get { return suggestions; }
            set
            {
                suggestions = value;          
            }
        }
       
        private void findSuggestions(String text)
        {

            if (String.IsNullOrEmpty(text))
            {
                Suggestions.Clear();
                return;
            }

            List<Object> matches;

            if (CustomFindMatchesFunction != null)
            {
                matches = CustomFindMatchesFunction(text);
            }
            else
            {
                matches = tree.AutoComplete(text);
            }

            Suggestions.ReplaceRange(matches.Take(Math.Min(MaxSuggestions, matches.Count)));

        }

        private void suggestionButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SelectedItem = button.Tag;
        }
       
    }
}
