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
using MediaViewer.ExtensionMethods;
using MediaViewer.Model.Mvvm;

namespace MediaViewer.DirectoryPicker
{
    /// <summary>
    /// Interaction logic for MoveRenameView.xaml
    /// </summary>
    public partial class DirectoryPickerView : Window
    {
       
        DirectoryPickerViewModel directoryPickerViewModel;

        public DirectoryPickerView()
        {
            InitializeComponent();

            directoryPickerViewModel = new DirectoryPickerViewModel();
            DataContext = directoryPickerViewModel;
            directoryBrowser.DataContext = directoryPickerViewModel;

            directoryPickerViewModel.ClosingRequest += new EventHandler<CloseableBindableBase.DialogEventArgs>((s,e) => {

                if (e.DialogMode == CloseableBindableBase.DialogMode.CANCEL)
                {
                    this.DialogResult = false;
                }
                else
                {
                    this.DialogResult = true;
                }
               
                this.Close();
            });

            Closing += new System.ComponentModel.CancelEventHandler((s, e) =>
            {
                directoryBrowser.stopDirectoryPickerInfoGatherTask();
            });
          
        }
     
        private void currentPathComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                directoryPickerViewModel.MovePath = (String)e.AddedItems[0];
            }
                  
        }
              
    }
}
