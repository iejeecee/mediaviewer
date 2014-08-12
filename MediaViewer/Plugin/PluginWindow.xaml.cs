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
using System.Windows.Shapes;

namespace MediaViewer.Plugin
{
    /// <summary>
    /// Interaction logic for PluginWindow.xaml
    /// </summary>
    public partial class PluginWindow : Window
    {
        public PluginWindow()
        {
            InitializeComponent();

            PluginWindowViewModel vm = new PluginWindowViewModel();
           
            // add view to window         
            Resources.MergedDictionaries.Add(vm.View);                
            
            DataContext = vm;
        }
    }
}
