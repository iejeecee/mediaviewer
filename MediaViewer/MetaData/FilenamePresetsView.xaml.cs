using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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

namespace MediaViewer.MetaData
{
    /// <summary>
    /// Interaction logic for FilenamePresetsView.xaml
    /// </summary>
    public partial class FilenamePresetsView : Window
    {
        FilenamePresetsViewModel filenamePresetsViewModel;

        public FilenamePresetsView()
        {
            InitializeComponent();
            DataContext = filenamePresetsViewModel = new FilenamePresetsViewModel();

            filenamePresetsViewModel.ClosingRequest += new EventHandler<CloseableBindableBase.DialogEventArgs>((s, e) =>
            {

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
        }

        private void insertCounterButton_Click(object sender, RoutedEventArgs e)
        {
            int index = presetTextBox.CaretIndex;
            filenamePresetsViewModel.InsertCounterCommand.Execute(index);
        }

        private void insertFilenameButton_Click(object sender, RoutedEventArgs e)
        {
            int index = presetTextBox.CaretIndex;
            filenamePresetsViewModel.InsertFilenameCommand.Execute(index);
        }

        private void insertDate_Click(object sender, RoutedEventArgs e)
        {
            int index = presetTextBox.CaretIndex;
            filenamePresetsViewModel.InsertDateCommand.Execute(index);
        }

        private void insertResolution_Click(object sender, RoutedEventArgs e)
        {
            int index = presetTextBox.CaretIndex;
            filenamePresetsViewModel.InsertResolutionCommand.Execute(index);

        }

        private void insertReplace_Click(object sender, RoutedEventArgs e)
        {
            int index = presetTextBox.CaretIndex;
            filenamePresetsViewModel.InsertReplaceCommand.Execute(index);

        }
           
       
       
    }
}
