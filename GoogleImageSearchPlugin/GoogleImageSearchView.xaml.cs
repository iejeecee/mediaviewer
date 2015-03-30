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

namespace GoogleImageSearchPlugin
{
    /// <summary>
    /// Interaction logic for GoogleImageSearchView.xaml
    /// </summary>
    public partial class GoogleImageSearchView : Window
    {
        GoogleImageSearchViewModel vm;

        public GoogleImageSearchView()
        {
            InitializeComponent();

            DataContext = vm = new GoogleImageSearchViewModel();

            vm.ClosingRequest += vm_ClosingRequest;

            this.Closing += googleImageSearchView_Closing;
        }

        void googleImageSearchView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            vm.MediaStateCollectionView.MediaState.clearUIState("Empty",DateTime.Now,MediaStateType.SearchResult);
        }

        void vm_ClosingRequest(object sender, MediaViewer.Model.Mvvm.CloseableBindableBase.DialogEventArgs e)
        {           
            this.Close();
        }

      
    }
}
