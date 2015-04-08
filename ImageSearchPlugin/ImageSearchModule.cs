using MediaViewer.Infrastructure;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchPlugin
{
    [ModuleExport(typeof(ImageSearchModule))]
    class ImageSearchModule : IModule
    {
        [Import]
        public IRegionManager RegionManager { get; set; }

        public void Initialize()
        {
            this.RegionManager.RegisterViewWithRegion(RegionNames.MediaFileBrowserToolBarRegion, typeof(ImageSearchNavigationItemView));

            ServiceLocator.Current.GetInstance(typeof(ImageSearchSettingsViewModel));
        }
    }
}
