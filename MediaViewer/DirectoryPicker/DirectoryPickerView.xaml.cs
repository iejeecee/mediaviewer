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

            directoryPickerViewModel.ClosingRequest += new EventHandler<MvvmFoundation.Wpf.CloseableObservableObject.DialogEventArgs>((s,e) => {

                if (e.DialogMode == MvvmFoundation.Wpf.CloseableObservableObject.DialogMode.CANCEL)
                {
                    this.DialogResult = false;
                }
                else
                {
                    this.DialogResult = true;
                }

                this.Close();
            });
          
        }

        private void currentPathComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression be = currentPathComboBox.GetBindingExpression(ComboBox.TextProperty);          
                be.UpdateSource();              
            }
        }

        private void currentPathComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                directoryPickerViewModel.MovePath = (String)e.AddedItems[0];
            }
            /*
            if (e.AddedItems.Count > 0)
            {
                //directoryPickerViewModel.MovePath = (String)e.AddedItems[0];
                currentPathComboBox.SetCurrentValue(ComboBox.TextProperty, (String)e.AddedItems[0]);
                //currentPathComboBox.Text = (String)e.AddedItems[0];
                BindingExpression be = currentPathComboBox.GetBindingExpression(ComboBox.TextProperty);
                be.UpdateSource();
            }
            else
            {
                currentPathComboBox.Text = (String)e.RemovedItems[0];
            }
            */
           
        }
/*
        private void oldNameButton_Click(object sender, RoutedEventArgs e)
        {
            TextBox textBox = newFileNameComboBox.getChildrenOfType<TextBox>().
                       FirstOrDefault(element => element.Name == "PART_EditableTextBox");

            if (textBox == null)
            {
                return;
            } 

            int index = textBox.CaretIndex;

            directoryPickerViewModel.InsertOldFilenameCommand.DoExecute(index);
        }

       

        private void counterButton_Click(object sender, RoutedEventArgs e)
        {
            TextBox textBox = newFileNameComboBox.getChildrenOfType<TextBox>().
                       FirstOrDefault(element => element.Name == "PART_EditableTextBox");

            if (textBox == null)
            {
                return;
            } 
            int index = textBox.CaretIndex;

            int counter = (int)counterIntegerUpDown.Value;

            Tuple<int, int> args = new Tuple<int, int>(index, counter);

            directoryPickerViewModel.InsertCounterCommand.DoExecute(args);
        }
*/
      

        
    }
}
