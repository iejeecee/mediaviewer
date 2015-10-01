using ImageSearchPlugin.Properties;
using MediaViewer.Model.Media.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace ImageSearchPlugin
{
    /// <summary>
    /// Interaction logic for GoogleImageSearchView.xaml
    /// </summary>
    public partial class GoogleImageSearchView : Window
    {
        ImageSearchViewModel vm;

        public GoogleImageSearchView()
        {
            InitializeComponent();

            DataContext = vm = new ImageSearchViewModel();

            vm.ClosingRequest += vm_ClosingRequest;

            this.Closing += googleImageSearchView_Closing;
        }

        void googleImageSearchView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            vm.MediaState.clearUIState("Empty",DateTime.Now,MediaStateType.SearchResult);
            vm.shutdown();
            Settings.Default.Save();
        }

        void vm_ClosingRequest(object sender, MediaViewer.Model.Mvvm.CloseableBindableBase.DialogEventArgs e)
        {           
            this.Close();
        }

        private void queryTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                vm.SearchCommand.Execute(0);
            }

            
        }

        async void mediaGridView_ScrolledToEnd(object sender, EventArgs e)
        {
            int nrItems = vm.MediaState.UIMediaCollection.Count;
            await vm.SearchCommand.ExecuteAsync(nrItems);
        }

      
    }
}
