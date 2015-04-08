using MediaViewer.DirectoryPicker;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Settings;
using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ImageSearchPlugin
{
    [Export]
    public class ImageSearchSettingsViewModel : SettingsBase
    {
        public ImageSearchSettings Settings {get;set;}

        public ListCollectionView Size { get; set; }
        public ListCollectionView SafeSearch { get; set; }
        public ListCollectionView Layout { get; set; }
        public ListCollectionView Type { get; set; }
        public ListCollectionView People { get; set; }
        public ListCollectionView Color { get; set; }

        public Command DownloadDirectoryCommand { get; set; }

        bool isAskDownloadPath;
        public bool IsAskDownloadPath {
            get
            {
                return (isAskDownloadPath);
            }
            set
            {
                SetProperty(ref isAskDownloadPath, value);
            }
        }

        bool isCurrentDownloadPath;
        public bool IsCurrentDownloadPath
        {
            get
            {
                return (isCurrentDownloadPath);
            }
            set
            {
                SetProperty(ref isCurrentDownloadPath, value);
            }
        }

        bool isFixedDownloadPath;
        public bool IsFixedDownloadPath
        {
            get
            {
                return (isFixedDownloadPath);
            }
            set
            {
                SetProperty(ref isFixedDownloadPath, value);
            }
        }

        string fixedDownloadPath;
        public String FixedDownloadPath
        {
            get
            {
                return (fixedDownloadPath);
            }
            set
            {
                SetProperty(ref fixedDownloadPath, value);
            }
        }

        public ObservableCollection<String> FixedDownloadPathHistory { get; set; }
        
        public ImageSearchSettingsViewModel() : base("Image Search Plugin", new Uri(typeof(ImageSearchSettingsView).FullName, UriKind.Relative))
        {            
            DownloadDirectoryCommand = new Command(() =>
            {                            
                DirectoryPickerView directoryPicker = new DirectoryPickerView();                
                DirectoryPickerViewModel vm = (DirectoryPickerViewModel)directoryPicker.DataContext;
                vm.SelectedPath = Settings.FixedDownloadPath;

                if (directoryPicker.ShowDialog() == true)
                {
                    FixedDownloadPath = vm.SelectedPath;
                    MiscUtils.insertIntoHistoryCollection(FixedDownloadPathHistory, FixedDownloadPath);
                }
            });

            
        }

        protected override void OnLoad()
        {
            Size = new ListCollectionView(ImageSearchViewModel.size);
            SafeSearch = new ListCollectionView(ImageSearchViewModel.safeSearch);
            Layout = new ListCollectionView(ImageSearchViewModel.layout);
            Type = new ListCollectionView(ImageSearchViewModel.type);
            People = new ListCollectionView(ImageSearchViewModel.people);
            Color = new ListCollectionView(ImageSearchViewModel.color);

            Settings = getSettings<ImageSearchSettings>();
            Settings.SetDefaults();

            Size.MoveCurrentTo(Settings.Size);
            SafeSearch.MoveCurrentTo(Settings.SafeSearch); 
            Layout.MoveCurrentTo(Settings.Layout);
            Type.MoveCurrentTo(Settings.Type); 
            People.MoveCurrentTo(Settings.Type);
            Color.MoveCurrentTo(Settings.Color);

            IsAskDownloadPath = Settings.IsAskDownloadPath;
            IsCurrentDownloadPath = Settings.IsCurrentDownloadPath;
            IsFixedDownloadPath = Settings.IsFixedDownloadPath;
            FixedDownloadPath = Settings.FixedDownloadPath;

            FixedDownloadPathHistory = Settings.FixedDownloadPathHistory;
        }

        protected override void OnSave()
        {
            Settings.Size = (string)Size.CurrentItem;
            Settings.SafeSearch = (string)SafeSearch.CurrentItem;
            Settings.Type = (string)Type.CurrentItem;
            Settings.People = (string)People.CurrentItem;
            Settings.Color = (string)Color.CurrentItem;

            Settings.IsAskDownloadPath = IsAskDownloadPath;
            Settings.IsCurrentDownloadPath = IsCurrentDownloadPath;
            Settings.IsFixedDownloadPath = Settings.IsFixedDownloadPath;
            Settings.FixedDownloadPath = FixedDownloadPath;

            saveSettings(Settings);
        }


    }
}
