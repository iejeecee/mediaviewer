using MediaViewer.Model.Mvvm;
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
    public partial class InputView : Window
    {

        InputViewModel inputViewModel;

        public InputView()
        {
            InitializeComponent();

            DataContext = inputViewModel = new InputViewModel();

            inputViewModel.ClosingRequest += new EventHandler<CloseableBindableBase.DialogEventArgs>((o, e) =>
            {
                if (e.DialogMode == CloseableBindableBase.DialogMode.CANCEL)
                {
                    DialogResult = false;                   
                }
                else
                {
                    DialogResult = true;               
                }

                Close();

            });
        }

    
       
    }
}
