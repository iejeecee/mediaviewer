using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.MefExtensions;
using Microsoft.Practices.Prism.Modularity;
using System.Windows;
using System.ComponentModel.Composition.Hosting;
using Microsoft.Practices.Prism.Regions;
using MediaViewer.UserControls.Layout;
using MediaViewer.UserControls.TabbedExpander;

namespace MediaViewer
{
    class MediaViewerBootstrapper : MefBootstrapper
    {
        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();
           
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(MediaViewerBootstrapper).Assembly));
            this.AggregateCatalog.Catalogs.Add(new DirectoryCatalog(".\\Plugins"));
                      
        }

        protected override System.Windows.DependencyObject CreateShell()
        {
            // issue with shell OnImportSatisfied called twice, see: https://compositewpf.codeplex.com/workitem/7634
            //  return this.Container.GetExportedValue<Shell>();
            return new Shell();
        }

        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            RegionAdapterMappings mappings = base.ConfigureRegionAdapterMappings();       
            mappings.RegisterMapping(typeof(TabbedExpanderPanel), Container.GetExport<TabbedExpanderPanelRegionAdapter>().Value);
            mappings.RegisterMapping(typeof(TabbedExpanderView), Container.GetExport<TabbedExpanderRegionAdapter>().Value);
            return mappings;
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = (Window)this.Shell;
            Application.Current.MainWindow.Show();
        }
    }
}
