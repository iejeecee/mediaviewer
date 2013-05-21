using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.DirectoryBrowser
{
    class LoadingObject : PathObject
    {
        public LoadingObject(PathObject parent) 
        {
            Parent = parent;
            Name = "Loading...";
            ImageUrl = "pack://application:,,,/Resources/Icons/Folder_Back.ico";
        }
    }
}
