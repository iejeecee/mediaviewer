using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Plugin
{
    public interface IUploadMedia
    {
        void upload(List<String> mediaItems);
    }
}
