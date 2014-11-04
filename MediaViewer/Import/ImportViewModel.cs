using MediaViewer.DirectoryPicker;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Mvvm;
using MediaViewer.Progress;
using MediaViewer.Search;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
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
   
    class ImportViewModel : CloseableBindableBase
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
                    OkCommand.IsExecutable = true;
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
                        OkCommand.IsExecutable = false;
                    }
                }));

            ClearIncludeLocationsCommand = new Command(new Action(() =>
            {
                IncludeLocations.Clear();
                OkCommand.IsExecutable = false;
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
            set { 
            SetProperty(ref includeLocations, value);
            }
        }

        ImportExportLocation selectedIncludeLocation;

        public ImportExportLocation SelectedIncludeLocation
        {
            get { return selectedIncludeLocation; }
            set { 
            SetProperty(ref selectedIncludeLocation, value);
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
                SetProperty(ref excludeLocations, value);
            }
        }

        ImportExportLocation selectedExcludeLocation;

        public ImportExportLocation SelectedExcludeLocation
        {
            get { return selectedExcludeLocation; }
            set
            {              
                SetProperty(ref selectedExcludeLocation, value);
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
            
        public Command OkCommand { get; set; }            
        public Command CancelCommand {get;set;}
        
         
    }


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
            set { 
                SetProperty(ref isSelected, value);
            }
        }

        bool isRecursive;

        public bool IsRecursive
        {
            get { return isRecursive; }
            set { 
                SetProperty(ref isRecursive, value);
            }
        }

        MediaType mediaType;

        public MediaType MediaType
        {
            get { return mediaType; }
            set { 
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
