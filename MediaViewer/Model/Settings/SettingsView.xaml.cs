using MediaViewer.Infrastructure;
using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MediaViewer.Model.Settings
{
    /// <summary>
    /// Interaction logic for AppSettings.xaml
    /// </summary>
    [Export]
    public partial class SettingsView : Window
    {
        IRegionManager regionManager { get; set; }
        SettingsViewModel ViewModel { get; set; }

        public SettingsView()
        {
            //http://stackoverflow.com/questions/19221634/opening-a-prism-module-on-new-window-on-a-registered-region
            regionManager = ServiceLocator.Current.GetInstance(typeof(IRegionManager)) as IRegionManager;
            this.SetValue(RegionManager.RegionManagerProperty, regionManager);
            
            InitializeComponent();
            
            ViewModel = ServiceLocator.Current.GetInstance(typeof(SettingsViewModel)) as SettingsViewModel;
            WeakEventManager<SettingsViewModel, CloseableBindableBase.DialogEventArgs>.AddHandler(ViewModel, "ClosingRequest", ClosingRequest);

            if (ViewModel.Categories.Count > 0)
            {
                ViewModel.SelectedCategory = ViewModel.Categories[0];
            }
         
            DataContext = ViewModel;

            this.Closed += globalSettingsView_Closed;
        }

        private void ClosingRequest(object sender, CloseableBindableBase.DialogEventArgs e)
        {           
            this.Close();
        }

        private void globalSettingsView_Closed(object sender, EventArgs e)
        {
            while (regionManager.Regions[RegionNames.GlobalSettingsRegion].Views.Count() > 0)
            {
                regionManager.Regions[RegionNames.GlobalSettingsRegion].Remove(regionManager.Regions[RegionNames.GlobalSettingsRegion].Views.FirstOrDefault());
            }

            regionManager.Regions.Remove(RegionNames.GlobalSettingsRegion);

            WeakEventManager<SettingsViewModel, CloseableBindableBase.DialogEventArgs>.RemoveHandler(ViewModel, "ClosingRequest", ClosingRequest);
        }
       
    }
}
