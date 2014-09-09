using MediaViewer.DirectoryPicker;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Progress;
using MediaViewer.Search;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
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
  
        public ImportViewModel(MediaFileWatcher mediaFileWatcher)
        {
            
            OkCommand = new Command(async () =>
            {
                CancellableOperationProgressView progress = new CancellableOperationProgressView();
                ImportProgressViewModel vm = new ImportProgressViewModel(mediaFileWatcher.MediaState);
                progress.DataContext = vm;
                progress.Show();
                Task t = vm.importAsync(IncludeLocations, ExcludeLocations);
                OnClosingRequest();
                await t;
            });
          
            CancelCommand = new Command(() =>
                {
                    OnClosingRequest();
                });
          
            IncludeLocations = new ObservableCollection<ImportExportLocation>();

            IncludeLocations.Add(new ImportExportLocation(mediaFileWatcher.Path));

            AddIncludeLocationCommand = new Command(new Action(() =>
            {
                DirectoryPickerView directoryPicker = new DirectoryPickerView();
                DirectoryPickerViewModel vm = (DirectoryPickerViewModel)directoryPicker.DataContext;

                if (SelectedIncludeLocation == null)
                {
                    vm.MovePath = mediaFileWatcher.Path;
                }
                else
                {
                    vm.MovePath = SelectedIncludeLocation.Location;
                }

                if (directoryPicker.ShowDialog() == true)
                {
                    ImportExportLocation newLocation = new ImportExportLocation(vm.MovePath);
                    if (!IncludeLocations.Contains(newLocation))
                    {
                        IncludeLocations.Add(newLocation);
                    }
                }

                if (IncludeLocations.Count > 0)
                {
                    OkCommand.CanExecute = true;
                }
               
            }));

            RemoveIncludeLocationCommand = new Command(new Action(() =>
                {
                    for (int i = IncludeLocations.Count() - 1; i >= 0; i--)
                    {
                        if (IncludeLocations[i].IsSelected == true)
                        {
                            IncludeLocations.RemoveAt(i);
                        }
                    }

                    if (IncludeLocations.Count == 0)
                    {
                        OkCommand.CanExecute = false;
                    }
                }));

            ClearIncludeLocationsCommand = new Command(new Action(() =>
            {
                IncludeLocations.Clear();
                OkCommand.CanExecute = false;
            }));

            ExcludeLocations = new ObservableCollection<ImportExportLocation>();
       
            AddExcludeLocationCommand = new Command(new Action(() =>
            {
                DirectoryPickerView directoryPicker = new DirectoryPickerView();
                DirectoryPickerViewModel vm = (DirectoryPickerViewModel)directoryPicker.DataContext;

                if (SelectedExcludeLocation == null)
                {
                    vm.MovePath = mediaFileWatcher.Path;
                }
                else
                {
                    vm.MovePath = SelectedExcludeLocation.Location;
                }

                if (directoryPicker.ShowDialog() == true)
                {
                    ImportExportLocation newLocation = new ImportExportLocation(vm.MovePath);
                    if (!ExcludeLocations.Contains(newLocation))
                    {
                        ExcludeLocations.Add(newLocation);
                    }
                }
          
            }));

            RemoveExcludeLocationCommand = new Command(new Action(() =>
            {
                for (int i = ExcludeLocations.Count() - 1; i >= 0; i--)
                {
                    if (ExcludeLocations[i].IsSelected == true)
                    {
                        ExcludeLocations.RemoveAt(i);
                    }
                }
             
            }));

            ClearExcludeLocationsCommand = new Command(new Action(() =>
            {
                ExcludeLocations.Clear();             
            }));
        }

        ObservableCollection<ImportExportLocation> includeLocations;

        public ObservableCollection<ImportExportLocation> IncludeLocations
        {
            get { return includeLocations; }
            set { includeLocations = value;
            NotifyPropertyChanged();
            }
        }

        ImportExportLocation selectedIncludeLocation;

        public ImportExportLocation SelectedIncludeLocation
        {
            get { return selectedIncludeLocation; }
            set { selectedIncludeLocation = value;
            NotifyPropertyChanged();
            }
        }

        Command addIncludeLocationCommand;

        public Command AddIncludeLocationCommand
        {
            get { return addIncludeLocationCommand; }
            set { addIncludeLocationCommand = value; }
        }

        Command removeIncludeLocationCommand;

        public Command RemoveIncludeLocationCommand
        {
            get { return removeIncludeLocationCommand; }
            set { removeIncludeLocationCommand = value; }
        }

        Command clearIncludeLocationsCommand;

        public Command ClearIncludeLocationsCommand
        {
            get { return clearIncludeLocationsCommand; }
            set { clearIncludeLocationsCommand = value; }
        }

        ObservableCollection<ImportExportLocation> excludeLocations;

        public ObservableCollection<ImportExportLocation> ExcludeLocations
        {
            get { return excludeLocations; }
            set
            {
                excludeLocations = value;
                NotifyPropertyChanged();
            }
        }

        ImportExportLocation selectedExcludeLocation;

        public ImportExportLocation SelectedExcludeLocation
        {
            get { return selectedExcludeLocation; }
            set
            {
                selectedExcludeLocation = value;
                NotifyPropertyChanged();
            }
        }

        Command addExcludeLocationCommand;

        public Command AddExcludeLocationCommand
        {
            get { return addExcludeLocationCommand; }
            set { addExcludeLocationCommand = value; }
        }

        Command removeExcludeLocationCommand;

        public Command RemoveExcludeLocationCommand
        {
            get { return removeExcludeLocationCommand; }
            set { removeExcludeLocationCommand = value; }
        }

        Command clearExcludeLocationsCommand;

        public Command ClearExcludeLocationsCommand
        {
            get { return clearExcludeLocationsCommand; }
            set { clearExcludeLocationsCommand = value; }
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
