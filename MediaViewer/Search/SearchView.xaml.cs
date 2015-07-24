using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.UserControls.TabbedExpander;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Windows.Themes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
    [Export]
    public partial class SearchView : UserControl, INavigationAware, ITabbedExpanderAware
    {
        bool firstFocus;

        public SearchView()
        {
            InitializeComponent();
            SearchViewModel viewModel = new SearchViewModel(MediaFileWatcher.Instance);
            DataContext = viewModel;
            firstFocus = true;

            TabName = "Search";
            TabIsSelected = false;
            TabMargin = new Thickness(2);
            TabBorderThickness = new Thickness(2);
            TabBorderBrush = ClassicBorderDecorator.ClassicBorderBrush;
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
                  
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
 	        return(true);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
 	        
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
 	        
        }

        public string TabName { get; set; }
        public bool TabIsSelected { get; set; }
        public Thickness TabMargin { get; set; }
        public Thickness TabBorderThickness { get; set; }
        public Brush TabBorderBrush { get; set; }
    }
}
