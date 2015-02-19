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
    public class PopupLocationItem : BindableBase, IEquatable<PopupLocationItem>
    {        
        public PopupLocationItem(String location, Command<PopupLocationItem> isSelectedCommand,
            Command<PopupLocationItem> removeCommand)
        {
            Name = location;
            Children = new ObservableCollection<PopupLocationItem>();

            IsSelected = false;
            IsRemovable = false;

            IsSelectedCommand = isSelectedCommand;
            RemoveCommand = removeCommand;
             
        }

        String name;

        public String Name
        {
            get { return name; }
            set { 
                
                SetProperty(ref name, value); 
            }
        }

        ObservableCollection<PopupLocationItem> children;

        public ObservableCollection<PopupLocationItem> Children
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

        bool isRemovable;

        public bool IsRemovable
        {
            get { return isRemovable; }
            set { SetProperty(ref isRemovable, value); }
        }

        public Command<PopupLocationItem> IsSelectedCommand { get; set; }
        public Command<PopupLocationItem> RemoveCommand { get; set; }
      
        public bool Equals(PopupLocationItem other)
        {
            return (Name.Equals(other.Name));
        }
    }
}
