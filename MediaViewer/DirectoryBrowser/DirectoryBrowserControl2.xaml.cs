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

namespace MediaViewer.DirectoryBrowser
{
    /// <summary>
    /// Interaction logic for DirectoryBrowserControl2.xaml
    /// </summary>
    public partial class DirectoryBrowserControl2 : UserControl
    {
        DirectoryBrowserViewModel directoryBrowserViewModel;

        public DirectoryBrowserControl2()
        {
            InitializeComponent();

            directoryBrowserViewModel = (DirectoryBrowserViewModel)internalDirectoryBrowser.DataContext;

            directoryBrowserViewModel.PathSelectedCallback = new DirectoryBrowserViewModel.PathSelectedDelegate((pathModel) =>
            {            
                SelectedPath = pathModel.getFullPath();
            });
        }

        public static readonly DependencyProperty SelectedPathProperty =
        DependencyProperty.Register("SelectedPath", typeof(string), typeof(DirectoryBrowserControl2), new FrameworkPropertyMetadata(String.Empty,
            FrameworkPropertyMetadataOptions.AffectsRender,
            new PropertyChangedCallback(selectedPathChangedCallback)));

        public string SelectedPath
        {
            get { return (string)GetValue(SelectedPathProperty); }
            set { SetValue(SelectedPathProperty, value); }
        }

        static void selectedPathChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {

            DirectoryBrowserControl2 control = (DirectoryBrowserControl2)o;
            control.directoryBrowserViewModel.selectPath((String)e.NewValue);

        }
      
    }
}
