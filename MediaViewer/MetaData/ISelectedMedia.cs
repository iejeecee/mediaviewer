using MediaViewer.Model.Media.File;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MetaData
{
    public interface ISelectedMedia
    {
        ObservableCollection<MediaFileItem> SelectedMedia { get; set; }
    }
}
