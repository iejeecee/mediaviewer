using MediaViewer.DirectoryBrowser;
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

namespace MediaViewer.MoveRename
{
    /// <summary>
    /// Interaction logic for MoveRenameView.xaml
    /// </summary>
    public partial class MoveRenameView : Window
    {
       
        MoveRenameViewModel moveRenameViewModel;

        public MoveRenameView()
        {
            InitializeComponent();

            moveRenameViewModel = new MoveRenameViewModel();
            DataContext = moveRenameViewModel;
            directoryBrowser.DataContext = moveRenameViewModel;

/*
            directoryBrowserViewModel = (DirectoryBrowserViewModel)directoryBrowser.DataContext;            

            directoryBrowserViewModel.PathSelectedCallback = new DirectoryBrowserViewModel.PathSelectedDelegate((pathModel) => {
                String selectedPath = pathModel.getFullPath();

                moveRenameViewModel.MovePath = selectedPath;

            });
*/
            moveRenameViewModel.MoveRenameFilesCommand.Executing += new MvvmFoundation.Wpf.Delegates.CancelCommandEventHandler((s, e) =>
            {
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

        private void oldNameButton_Click(object sender, RoutedEventArgs e)
        {
            TextBox textBox = newFileNameComboBox.getChildrenOfType<TextBox>().
                       FirstOrDefault(element => element.Name == "PART_EditableTextBox");

            if (textBox == null)
            {
                return;
            } 

            int index = textBox.CaretIndex;

            moveRenameViewModel.InsertOldFilenameCommand.DoExecute(index);
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

            moveRenameViewModel.InsertCounterCommand.DoExecute(args);
        }

      

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
