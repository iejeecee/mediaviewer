using MediaViewer.MediaFileModel.Watcher;
using MvvmFoundation.Wpf;
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

namespace MediaViewer.Search
{
    /// <summary>
    /// Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView : UserControl
    {
        bool firstFocus;

        public SearchView()
        {
            InitializeComponent();
            SearchViewModel viewModel = new SearchViewModel(MediaFileWatcher.Instance);
            DataContext = viewModel;
            firstFocus = true;
        }
                           
        private void queryTextBox_KeyDown(object sender, KeyEventArgs e)
        {           
            if (e.Key == Key.Enter)
            {
                BindingExpression be = queryTextBox.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }
        }

        private void queryTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (firstFocus == true)
            {
                queryTextBox.Text = "";
                BindingExpression be = queryTextBox.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
                firstFocus = false;
            }
        }

        
        
    }
}
