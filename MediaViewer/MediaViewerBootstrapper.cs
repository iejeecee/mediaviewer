using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.MefExtensions;
using Microsoft.Practices.Prism.Modularity;
using System.Windows;
using System.ComponentModel.Composition.Hosting;

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

        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = (Window)this.Shell;
            Application.Current.MainWindow.Show();
        }
    }
}
