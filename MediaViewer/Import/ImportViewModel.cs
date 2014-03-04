using MediaViewer.DirectoryPicker;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.MediaFileModel;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Progress;
using MediaViewer.Search;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MediaViewer.Import
{
    class ImportViewModel : CloseableObservableObject
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ImportViewModel()
        {
            
            OkCommand = new Command(async () =>
            {
                ImportProgressView progress = new ImportProgressView();
                ImportProgressViewModel vm = progress.DataContext as ImportProgressViewModel;
                progress.Show();
                Task t = vm.importAsync(Locations);
                OnClosingRequest();
                await t;
            });
          
            CancelCommand = new Command(() =>
                {
                    OnClosingRequest();
                });
          
            Locations = new ObservableCollection<ImportExportLocation>();

            Locations.Add(new ImportExportLocation(MediaFileWatcher.Instance.Path));

            addLocationCommand = new Command(new Action(() =>
            {
                DirectoryPickerView directoryPicker = new DirectoryPickerView();
                DirectoryPickerViewModel vm = (DirectoryPickerViewModel)directoryPicker.DataContext;

                if (SelectedLocation == null)
                {
                    vm.MovePath = MediaFileWatcher.Instance.Path;
                }
                else
                {
                    vm.MovePath = SelectedLocation.Location;
                }

                if (directoryPicker.ShowDialog() == true)
                {
                    ImportExportLocation newLocation = new ImportExportLocation(vm.MovePath);
                    if (!Locations.Contains(newLocation))
                    {
                        Locations.Add(newLocation);
                    }
                }

                if (Locations.Count > 0)
                {
                    OkCommand.CanExecute = true;
                }
               
            }));

            removeLocationCommand = new Command(new Action(() =>
                {
                    for (int i = Locations.Count() - 1; i >= 0; i--)
                    {
                        if (Locations[i].IsSelected == true)
                        {
                            Locations.RemoveAt(i);
                        }
                    }

                    if (Locations.Count == 0)
                    {
                        OkCommand.CanExecute = false;
                    }
                }));

            clearLocationsCommand = new Command(new Action(() =>
            {
                Locations.Clear();
                OkCommand.CanExecute = false;
            }));
        }

        ObservableCollection<ImportExportLocation> locations;

        public ObservableCollection<ImportExportLocation> Locations
        {
            get { return locations; }
            set { locations = value;
            NotifyPropertyChanged();
            }
        }

        ImportExportLocation selectedLocation;

        public ImportExportLocation SelectedLocation
        {
            get { return selectedLocation; }
            set { selectedLocation = value;
            NotifyPropertyChanged();
            }
        }

        Command addLocationCommand;

        public Command AddLocationCommand
        {
            get { return addLocationCommand; }
            set { addLocationCommand = value; }
        }

        Command removeLocationCommand;

        public Command RemoveLocationCommand
        {
            get { return removeLocationCommand; }
            set { removeLocationCommand = value; }
        }

        Command clearLocationsCommand;

        public Command ClearLocationsCommand
        {
            get { return clearLocationsCommand; }
            set { clearLocationsCommand = value; }
        }
        
    
        Command okCommand;

        public Command OkCommand
        {
            get { return okCommand; }
            set
            {
                okCommand = value;
                NotifyPropertyChanged();
            }
        }

        Command cancelCommand;

        public Command CancelCommand
        {
            get { return cancelCommand; }
            set
            {
                cancelCommand = value;
                NotifyPropertyChanged();
            }
        }        
         
    }


    class ImportExportLocation : ObservableObject, IEquatable<ImportExportLocation>
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
            set { isSelected = value;
            NotifyPropertyChanged();
            }
        }

        bool isRecursive;

        public bool IsRecursive
        {
            get { return isRecursive; }
            set { isRecursive = value;
            NotifyPropertyChanged();
            }
        }

        MediaType mediaType;

        public MediaType MediaType
        {
            get { return mediaType; }
            set { mediaType = value;
            NotifyPropertyChanged();
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
