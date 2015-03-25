using MediaViewer.Infrastructure;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleImageSearchPlugin
{
    [ModuleExport(typeof(GoogleImageSearchModule))]
    class GoogleImageSearchModule : IModule
    {
        [Import]
        public IRegionManager RegionManager { get; set; }

        public void Initialize()
        {
            this.RegionManager.RegisterViewWithRegion(RegionNames.MediaFileBrowserToolBarRegion, typeof(GoogleImageSearchNavigationItemView));
        }
    }
}
