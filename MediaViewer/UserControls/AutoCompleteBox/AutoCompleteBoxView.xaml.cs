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
        private AutoCompleteBoxViewModel ViewModel
        {
            get { return this.Resources["viewModel"] as AutoCompleteBoxViewModel; }
        }

        bool mouseOverPopup;
     

        public AutoCompleteBoxView()
        {
            InitializeComponent();
        
            ViewModel.Suggestions.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler((s,e) => {

                if (ViewModel.Suggestions.Count > 0)
                {
                    displaySuggestions();
                }
                else if(ViewModel.Suggestions.Count == 0)
                {
                    popup.IsOpen = false;
                }
            
            });

            ViewModel.SetSuggestionCommand.Executed += new MvvmFoundation.Wpf.Delegates.CommandEventHandler<object>((o, e) =>
            {
                popup.IsOpen = false;
                autoCompleteTextBox.Focus();
                autoCompleteTextBox.CaretIndex = autoCompleteTextBox.Text.Length;
            });

            ViewModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler((o, e) =>
            {
                if (e.PropertyName.Equals("Text"))
                {
                    Text = ViewModel.Text;
                }
                else if (e.PropertyName.Equals("SelectedObject"))
                {
                    SelectedItem = ViewModel.SelectedObject;
                }
            });

            mouseOverPopup = false;
        }

        public IEnumerable<Object> Items
        {
            get { return (IEnumerable<Object>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IEnumerable<Object>), typeof(AutoCompleteBoxView), new PropertyMetadata(null,new PropertyChangedCallback(itemsChangedCallback)));

        private static void itemsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBoxView view = (AutoCompleteBoxView)d;

            view.ViewModel.Items = e.NewValue as IEnumerable<Object>;
        }


        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(AutoCompleteBoxView), new PropertyMetadata(null));
        

        public String Text
        {
            get { return (String)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(String), typeof(AutoCompleteBoxView), new PropertyMetadata(null,new PropertyChangedCallback(textChangedCallback)));

        private static void textChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBoxView view = (AutoCompleteBoxView)d;

            view.ViewModel.Text = e.NewValue as String;
        }
       
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
                if (ViewModel.Suggestions.Count == 0)
                {
                    SystemSounds.Beep.Play();
                }
                else
                {
                    ViewModel.SetSuggestionCommand.DoExecute(ViewModel.Suggestions[0]);
                }
            } 
            
        }

       
    }
}
