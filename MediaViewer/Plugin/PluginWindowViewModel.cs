using MediaViewer.MediaFileModel.Watcher;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MediaViewer.Plugin
{
    class PluginWindowViewModel : ObservableObject
    {
        [ImportMany(typeof(ResourceDictionary))]
        private IEnumerable<Lazy<ResourceDictionary, IPluginMetadata>> importedViews { get; set; }

        [ImportMany(typeof(IGeoTagViewModel))]
        private IEnumerable<Lazy<IGeoTagViewModel, IPluginMetadata>> importedViewModels { get; set; }

        ResourceDictionary view;

        public ResourceDictionary View
        {
            get { return view; }
        
        }

        IGeoTagViewModel viewModel;

        public IGeoTagViewModel ViewModel
        {
            get { return viewModel;            
            }
        }

        CompositionContainer container;

        public PluginWindowViewModel()
        {
            //An aggregate catalog that combines multiple catalogs
            AggregateCatalog catalog = new AggregateCatalog();
            //Adds all the parts found in the same assembly as the Program class          
            catalog.Catalogs.Add(new DirectoryCatalog(".\\Plugins"));

            //Create the CompositionContainer with the parts in the catalog
            container = new CompositionContainer(catalog);

            //Fill the imports of this object
            try
            {
                List<MediaFileItem> items = MediaFileWatcher.Instance.MediaState.getSelectedItemsUIState();

                container.ComposeExportedValue<List<MediaFileItem>>("GeoTagItems", items);
                container.ComposeParts(this);
            
                foreach (Lazy<ResourceDictionary, IPluginMetadata> dict in importedViews)
                {
                    if (dict.Metadata.Name.Equals("GoogleEarthGeoTagView"))
                    {
                        view = dict.Value;
                    }                   
                }

                foreach (Lazy<IGeoTagViewModel, IPluginMetadata> vm in importedViewModels)
                {
                    if (vm.Metadata.Name.Equals("GoogleEarthGeoTagViewModel"))
                    {
                        viewModel = vm.Value;
                    }                  
                }
           
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
            }
           
        }
    }
}
