using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.MediaFileModel;
using MvvmFoundation.Wpf;
using System.Windows.Data;

namespace MediaViewer.ImageGrid
{
    class ImageGridViewModel : ObservableObject
    {

        public ImageGridViewModel()
        {
            locations = new ObservableRangeCollection<string>();
        }

        ObservableRangeCollection<String> locations;

        public ObservableRangeCollection<String> Locations
        {
            get { return locations; }
            set { locations = value; }
        }

     

       


      
    }
}
