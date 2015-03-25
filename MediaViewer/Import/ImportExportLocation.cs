using MediaViewer.Search;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Import
{
    class ImportExportLocation : BindableBase, IEquatable<ImportExportLocation>
    {
        public ImportExportLocation(String location)
        {
            this.location = location;
            IsRecursive = true;
            MediaType = Search.MediaType.All;
            IsSelected = false;
        }

        String location;

        public String Location
        {
            get { return location; }
        }

        bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                SetProperty(ref isSelected, value);
            }
        }

        bool isRecursive;

        public bool IsRecursive
        {
            get { return isRecursive; }
            set
            {
                SetProperty(ref isRecursive, value);
            }
        }

        MediaType mediaType;

        public MediaType MediaType
        {
            get { return mediaType; }
            set
            {
                SetProperty(ref mediaType, value);
            }
        }

        public bool Equals(ImportExportLocation other)
        {
            if (other == null)
            {
                throw new ArgumentException();
            }

            return (other.Location.Equals(Location));
        }
    }
}
