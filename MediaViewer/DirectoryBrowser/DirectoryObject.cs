using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.DirectoryBrowser
{
    class DirectoryObject : PathObject
    {
       
        public DirectoryObject(PathObject parent, DirectoryInfo info)
        {
            Parent = parent;
            Name = info.Name;
            ImageUrl = "pack://application:,,,/Resources/Icons/Folder_Back.ico";
        }
    }
}
