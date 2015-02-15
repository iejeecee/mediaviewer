using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel.Composition;
using MediaViewer.About;

namespace MediaViewer.About
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    [Export]
    public partial class AboutView : UserControl
    {
        [ImportingConstructor]
        public AboutView(AboutViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
