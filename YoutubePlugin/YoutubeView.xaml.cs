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

namespace YoutubePlugin
{
    /// <summary>
    /// Interaction logic for YoutubeView.xaml
    /// </summary>
    public partial class YoutubeView : Window
    {
        YoutubeViewModel vm {get;set;}

        public YoutubeView()
        {
            InitializeComponent();

            DataContext = vm = new YoutubeViewModel();

            vm.ClosingRequest += vm_ClosingRequest;

            this.Closing += youtubeView_Closing;
        }

        private void youtubeView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void vm_ClosingRequest(object sender, MediaViewer.Model.Mvvm.CloseableBindableBase.DialogEventArgs e)
        {
            this.Close();
        }

        private void queryTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                vm.SearchCommand.Execute();
            }

        }
    }
}
