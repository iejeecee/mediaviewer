//http://stackoverflow.com/questions/19221634/opening-a-prism-module-on-new-window-on-a-registered-region
using Microsoft.Practices.Prism.Regions;
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
        IRegionManager Rm;

        public YoutubeView(IRegionManager regionManager)
        {
            InitializeComponent();
            Rm = regionManager;

            this.SetValue(RegionManager.RegionManagerProperty, regionManager);

            DataContext = vm = new YoutubeViewModel(regionManager);

            vm.ClosingRequest += vm_ClosingRequest;

            this.Closing += youtubeView_Closing;
        }

        private void youtubeView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            while (Rm.Regions["youtubeExpanderPanelRegion"].Views.Count() > 0)
            {
                Rm.Regions["youtubeExpanderPanelRegion"].Remove(Rm.Regions["youtubeExpanderPanelRegion"].Views.FirstOrDefault());
            }

            Rm.Regions.Remove("youtubeExpanderPanelRegion");
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
