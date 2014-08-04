using MediaViewer.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginTest
{
    [Export(typeof(IUploadMedia))]
    [ExportMetadata("Info", "Uploading to some site")]
    public class UploadImages : IUploadMedia
    {
        public void upload(List<string> mediaItems)
        {
            int i = 0;
            int k = i + 5;
        }
    }
}
