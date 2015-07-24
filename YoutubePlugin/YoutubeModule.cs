using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using System.ComponentModel.Composition;
using MediaViewer.Infrastructure;

namespace YoutubePlugin
{
    [ModuleExport(typeof(YoutubeModule))]
    class YoutubeModule : IModule
    {
        [Import]
        public IRegionManager RegionManager { get; set; }

        public void Initialize()
        {
            this.RegionManager.RegisterViewWithRegion(RegionNames.MainNavigationToolBarRegion, typeof(YoutubeNavigationItemView));

            //ServiceLocator.Current.GetInstance(typeof(ImageSearchSettingsViewModel));
        }
    }
}
