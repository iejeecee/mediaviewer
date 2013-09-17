using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.DirectoryBrowser
{
    class DummyPathModel : PathModel
    {
        public DummyPathModel(PathModel parent, DirectoryBrowserViewModel directoryBrowserViewModel) 
            : base(directoryBrowserViewModel)
        {
            Parent = parent;
            Name = "Loading...";
            ImageUrl = "pack://application:,,,/Resources/Icons/Folder_Back.ico";
        }
    }
}
