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

namespace MediaViewer.Import
{
    /// <summary>
    /// Interaction logic for ImportView.xaml
    /// </summary>
    public partial class ImportProgressView : Window
    {
      
        public ImportProgressView()
        {
            InitializeComponent();
            ImportProgressViewModel vm = new ImportProgressViewModel();
            DataContext = vm;
            vm.ClosingRequest += new EventHandler<MvvmFoundation.Wpf.CloseableObservableObject.DialogEventArgs>((o, e) =>
            {
                this.Close();
            });

            Closing += new System.ComponentModel.CancelEventHandler((s, e) =>
            {
                vm.CancelCommand.DoExecute();
            });
        }

        
    }
}
