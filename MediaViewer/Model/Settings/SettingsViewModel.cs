using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Regions;
using System.Collections.ObjectModel;
using MediaViewer.Model.Mvvm;
using MediaViewer.Infrastructure;

namespace MediaViewer.Model.Settings
{
    [Export]
    public class SettingsViewModel : CloseableBindableBase
    {
        public Command CloseCommand { get; set; }
        public ObservableCollection<SettingsBase> Categories { get; set; }

        IRegionManager RegionManager { get; set; }

        [ImportingConstructor]
        public SettingsViewModel(IRegionManager regionManager) {

            RegionManager = regionManager;
            Categories = new ObservableCollection<SettingsBase>();

            CloseCommand = new Command(() =>
            {             
                OnClosingRequest();
            });
        }

        SettingsBase selectedCategory;

        public SettingsBase SelectedCategory
        {
            get { return selectedCategory; }
            set { 
                
                SetProperty(ref selectedCategory, value);

                RegionManager.RequestNavigate(RegionNames.GlobalSettingsRegion, SelectedCategory.NavigationUri);
            }
        }

        public void AddCategory(SettingsBase item)
        {
            if (!Categories.Contains(item))
            {
                Categories.Add(item);
            }
        }
    }
}
