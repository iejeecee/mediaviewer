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
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Mvvm;

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
         
            DataContextChanged += ImportView_DataContextChanged;
        }

        void vm_ClosingRequest(object sender, CloseableBindableBase.DialogEventArgs e)
        {
            this.Close();
        }

        void ImportView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                (e.OldValue as CloseableBindableBase).ClosingRequest -= vm_ClosingRequest;
            }

            if (e.NewValue != null)
            {
                (e.NewValue as CloseableBindableBase).ClosingRequest += vm_ClosingRequest;
            }
        }
    }
}
