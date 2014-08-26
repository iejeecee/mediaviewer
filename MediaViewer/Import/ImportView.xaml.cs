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
using System.ComponentModel.Composition;
using MediaViewer.MediaFileModel.Watcher;

namespace MediaViewer.Import
{
    /// <summary>
    /// Interaction logic for ImportView.xaml
    /// </summary>
    [Export]
    public partial class ImportView : Window
    {
      
        public ImportView()
        {
            InitializeComponent();

            ImportViewModel vm = new ImportViewModel(MediaFileWatcher.Instance);

            vm.ClosingRequest += new EventHandler<MvvmFoundation.Wpf.CloseableObservableObject.DialogEventArgs>((o, e) =>
            {
                this.Close();
            });

            DataContext = vm;     
        }
    }
}
