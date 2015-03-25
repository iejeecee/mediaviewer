using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using MediaViewer;
using MediaViewer.Infrastructure;

namespace GoogleEarthGeoTagPlugin
{
    [ModuleExport(typeof(GoogleEarthGeoTagModule))]
    class GoogleEarthGeoTagModule : IModule
    {
        [Import]
        public IRegionManager RegionManager { get; set; }

        public void Initialize()
        {
            this.RegionManager.RegisterViewWithRegion(RegionNames.MediaFileBrowserPluginRegion, typeof(GoogleEarthGeoTagNavigationItemView));
        }
    }
}
