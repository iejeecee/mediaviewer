using MediaViewer.Model.Media.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchPlugin
{
    class ImageSearchQuery
    {
        public ImageSearchQuery(ImageSearchViewModel vm)
        {
            Query = vm.Query;
            Size = vm.Size.CurrentItem.ToString();
            SafeSearch = vm.SafeSearch.CurrentItem.ToString();
            Layout = vm.Layout.CurrentItem.ToString();
            Type = vm.Type.CurrentItem.ToString();
            People = vm.People.CurrentItem.ToString();
            Color = vm.Color.CurrentItem.ToString();
            GeoTag = vm.GeoTag;            
        }

        public String Query { get; set; }
        public String Size { get; set; }
        public String SafeSearch { get; set; }
        public String Layout { get; set; }
        public String Type { get; set; }
        public String People { get; set; }
        public String Color { get; set; }
        public GeoTagCoordinatePair GeoTag { get; set; }
    }
}
