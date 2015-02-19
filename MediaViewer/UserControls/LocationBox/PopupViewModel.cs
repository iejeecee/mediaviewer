using MediaViewer.Infrastructure.Logging;
using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.LocationBox
{
    class PopupViewModel : BindableBase
    {        
        public event EventHandler<PopupLocationItem> LocationSelected;
        public event EventHandler<PopupLocationItem> LocationRemoved;

        public PopupViewModel()
        {
            Locations = new ObservableCollection<PopupLocationItem>();
            LocationSelectedCommand = new Command<PopupLocationItem>((item) =>
            {
                if (!Locations.Contains(item)) return;

                foreach (PopupLocationItem items in Locations)
                {
                    items.IsSelected = false;
                }

                item.IsSelected = true;

                if (LocationSelected != null)
                {
                    LocationSelected(this, item);
                }
                             
            });

            LocationRemovedCommand = new Command<PopupLocationItem>((item) =>
            {
                Locations.Remove(item);

                if (LocationRemoved != null)
                {
                    LocationRemoved(this, item);
                }
            });
       
        }

        public void setLocations(ObservableCollection<String> locations, bool isRemovable)
        {
           
            Locations.Clear();

            for(int i = locations.Count() - 1; i >= 0; i--)
            {
                
               PopupLocationItem location = new PopupLocationItem(locations[i], LocationSelectedCommand, 
                   LocationRemovedCommand);
               location.IsRemovable = isRemovable;

               Locations.Insert(0, location);
              
            }
        }

        Command<PopupLocationItem> LocationSelectedCommand { get; set; }
        Command<PopupLocationItem> LocationRemovedCommand { get; set; }

        ObservableCollection<PopupLocationItem> locations;

        public ObservableCollection<PopupLocationItem> Locations
        {
            get { return locations; }
            set { locations = value; }
        }

        public int getIndexOfSelectedLocation()
        {
            int i = 0;

            for (; i < Locations.Count(); i++)
            {
                if (Locations[i].IsSelected == true)
                {
                    return (i);
                }
            }

            return (-1);
        }
    }
}
