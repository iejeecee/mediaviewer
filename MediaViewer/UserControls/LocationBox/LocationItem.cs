using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Utils;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.LocationBox
{
    public class LocationItem : BindableBase, IEquatable<LocationItem>
    {        
        public LocationItem(String location, Command<LocationItem> isSelectedCommand)
        {
            Name = location;
            Children = new ObservableCollection<LocationItem>();

            IsSelected = false;

            IsSelectedCommand = isSelectedCommand;
             
        }

        String name;

        public String Name
        {
            get { return name; }
            set { 
                String properPath = FileUtils.getProperDirectoryCapitalization(new System.IO.DirectoryInfo(value));

                SetProperty(ref name, properPath); 
            }
        }

        ObservableCollection<LocationItem> children;

        public ObservableCollection<LocationItem> Children
        {
            get { return children; }
            set { children = value; }
        }

        bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }

        public Command<LocationItem> IsSelectedCommand { get; set; }
      
        public bool Equals(LocationItem other)
        {
            return (Name.Equals(other.Name));
        }
    }
}
