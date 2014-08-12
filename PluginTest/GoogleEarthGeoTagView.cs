//http://www.codeproject.com/Articles/43625/Building-an-Extensible-Application-with-MEF-WPF-an
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PluginTest
{
    [Export(typeof(ResourceDictionary))]
    [ExportMetadata("Name", "GoogleEarthGeoTagView")]
    [ExportMetadata("Version", "1.0")]
    public partial class GoogleEarthGeoTagView : ResourceDictionary
    {
        public GoogleEarthGeoTagView()
        {
            InitializeComponent();
        }

    }
}
