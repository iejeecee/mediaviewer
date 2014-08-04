using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Plugin
{
    class LoadPlugins 
    {
        [ImportMany(typeof(IUploadMedia))]
        IEnumerable<Lazy<IUploadMedia, IUploadMediaData>> operations;

        CompositionContainer container;

        public void load()
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
                container.ComposeParts(this);

                foreach (Lazy<IUploadMedia, IUploadMediaData> operation in operations)
                {
                    String info = operation.Metadata.Info;                    
                    IUploadMedia plop = operation.Value as IUploadMedia;
                    plop.upload(null);
                }
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
            }

           
        }
    }
}
