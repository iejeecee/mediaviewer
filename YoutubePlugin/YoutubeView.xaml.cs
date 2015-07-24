using Microsoft.Practices.Prism.PubSubEvents;
//http://stackoverflow.com/questions/19221634/opening-a-prism-module-on-new-window-on-a-registered-region
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
    [Export]
    public partial class YoutubeView : UserControl, INavigationAware
    {
        YoutubeViewModel ViewModel {get;set;}
        IRegionManager RegionManager { get; set; }
        IEventAggregator EventAggregator { get; set; }

        [ImportingConstructor]
        public YoutubeView(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            RegionManager = regionManager;
            EventAggregator = eventAggregator;

            this.SetValue(Microsoft.Practices.Prism.Regions.RegionManager.RegionManagerProperty, RegionManager);

            DataContext = ViewModel = new YoutubeViewModel(regionManager,eventAggregator);                      
        }

        private async void mediaGridView_ScrolledToEnd(object sender, EventArgs e)
        {
            await ViewModel.LoadNextPageCommand.ExecuteAsync();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return (true);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            /*while (Rm.Regions["youtubeExpanderPanelRegion"].Views.Count() > 0)
            {
                Rm.Regions["youtubeExpanderPanelRegion"].Remove(Rm.Regions["youtubeExpanderPanelRegion"].Views.FirstOrDefault());
            }

            Rm.Regions.Remove("youtubeExpanderPanelRegion");*/
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            
        }
    }
}
