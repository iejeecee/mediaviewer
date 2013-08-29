using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MediaViewer.Input
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window, INotifyPropertyChanged
    {
        public InputWindow()
        {
            InitializeComponent();
          
        }

        string inputText;

        public string InputText
        {
            get { return inputText; }
            set { inputText = value; 
                PropertyChanged(this, new PropertyChangedEventArgs("InputText"));
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void copyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(InputText))
            {
                Clipboard.SetText(InputText);
            }

        }

        private void pasteButton_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                InputText = Clipboard.GetText();
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
