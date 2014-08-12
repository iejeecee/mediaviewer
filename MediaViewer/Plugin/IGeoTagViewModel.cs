using MediaViewer.MediaFileModel.Watcher;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Plugin
{
    public interface IGeoTagViewModel 
    {
        event EventHandler Loaded;
      
        MediaFileItem SelectedMedia { get; set; }

        Command AddGeoTag { get; }
        Command RemoveGeoTag { get; }
        Command<String> LookAt {get;}
    }
}
