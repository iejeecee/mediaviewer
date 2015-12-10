using MediaViewer.Model.Global.Commands;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Settings;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Settings
{
    public class SettingsBase : BindableBase, IComparable<SettingsBase>, IEquatable<SettingsBase>
    {
        public event EventHandler SettingsChanged;

        private Command SaveCommand { get; set; }

        protected SettingsBase(String title, Uri navigationUri)
        {
            Title = title;
            NavigationUri = navigationUri;

            SaveCommand = new Command(() =>
            {
                OnSave();
            });

            GlobalCommands.SaveGlobalSettingsCommand.RegisterCommand(SaveCommand);

            SettingsViewModel globalSettings = ServiceLocator.Current.GetInstance(typeof(SettingsViewModel)) as SettingsViewModel;

            globalSettings.AddCategory(this);
                 
        }
           
        String title;

        public String Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        Uri navigationUri;

        public Uri NavigationUri
        {
            get { return navigationUri; }
            set { SetProperty(ref navigationUri, value); }
        }
 
        public bool Equals(SettingsBase other)
        {
            return(Title.Equals(other.Title));
        }

        public int CompareTo(SettingsBase other)
        {
            return (Title.CompareTo(other.Title));
        }
     
        protected virtual void OnSave()
        {
            if (SettingsChanged != null)
            {
                SettingsChanged(this, EventArgs.Empty);
            }
        }
       
    }
}
