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
        public event EventHandler<LocationItem> LocationSelected;

        public PopupViewModel()
        {
            Locations = new ObservableCollection<LocationItem>();
            LocationSelectedCommand = new Command<LocationItem>((item) =>
            {                
                foreach (LocationItem items in Locations)
                {
                    items.IsSelected = false;
                }

                item.IsSelected = true;

                if (LocationSelected != null)
                {
                    LocationSelected(this, item);
                }
                             
            });
       
        }

        public void setLocations(ObservableCollection<String> locations)
        {
           
            Locations.Clear();

            for(int i = locations.Count() - 1; i >= 0; i--)
            {
                try
                {
                    Locations.Insert(0, new LocationItem(locations[i], LocationSelectedCommand));
                }
                catch(Exception e)
                {
                    Logger.Log.Error("Removed incorrect directory", e);
                    locations.RemoveAt(i);
                }
            }
        }

        Command<LocationItem> LocationSelectedCommand { get; set; }

        ObservableCollection<LocationItem> locations;

        public ObservableCollection<LocationItem> Locations
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
