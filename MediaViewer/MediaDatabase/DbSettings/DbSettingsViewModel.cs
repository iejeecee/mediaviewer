using MediaViewer.Model.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Mvvm;
using Microsoft.Win32;
using MediaViewer.Progress;
using System.Xml;
using System.Runtime.Serialization;
using MediaViewer.MediaDatabase.DataTransferObjects;
using AutoMapper;
using System.Windows;
using MediaViewer.Model.Media.File;
using MediaViewer.Export;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Media.File.Watcher;

namespace MediaViewer.MediaDatabase.DbSettings
{
    [Export]
    public class DbSettingsViewModel : SettingsBase
    {
        public Command ExportTagsCommand { get; set; }
        public Command ImportTagsCommand { get; set; }
        public Command ClearMediaCommand { get; set; }
        public Command ClearTagsCommand { get; set; }


        public DbSettingsViewModel()
            : base("Database", new Uri(typeof(DbSettingsView).FullName, UriKind.Relative))
        {
            ExportTagsCommand = new Command(exportTags);
            ImportTagsCommand = new Command(importTags);
            ClearMediaCommand = new Command(async () => await clearMedia());
            ClearTagsCommand = new Command(clearTags);
        }

        public int NrVideos
        {
            get
            {
                int result = 0;

                using (MediaDbCommands mediaCommands = new MediaDbCommands())
                {
                    result = mediaCommands.getNrVideos();
                }

                return (result);
            }
            private set
            {
                OnPropertyChanged("NrVideos");
            }
        }

        public int NrImages
        {
            get
            {
                int result = 0;

                using (MediaDbCommands mediaCommands = new MediaDbCommands())
                {
                    result = mediaCommands.getNrImages();
                }

                return (result);
            }
            private set
            {
                OnPropertyChanged("NrImages");
            }
        }

        public int NrMedia
        {
            get
            {
                int result = 0;

                using (MediaDbCommands mediaCommands = new MediaDbCommands())
                {
                    result = mediaCommands.getNrMedia();
                }

                return (result);
            }

            private set
            {
                OnPropertyChanged("NrMedia");
            }
          
        }

        public int NrTags
        {
            get
            {
                int result = 0;

                using (TagDbCommands tagCommands = new TagDbCommands())
                {
                    result = tagCommands.getNrTags();
                }
               

                return (result);
            }

            private set
            {
                OnPropertyChanged("NrTags");
            }
        }

        private void exportTags()
        {
            SaveFileDialog saveTagsDialog = MediaViewer.Model.Utils.Windows.FileDialog.createSaveTagsFileDialog();
            if (saveTagsDialog.ShowDialog() == false) return;

            CancellableOperationProgressView exportView = new CancellableOperationProgressView();
            TagOperationsViewModel vm = new TagOperationsViewModel();
            exportView.DataContext = vm;
            Task.Run(() => vm.export(saveTagsDialog.FileName));                      
            exportView.ShowDialog();           
        }

        private void importTags()
        {
            OpenFileDialog loadTagsDialog = MediaViewer.Model.Utils.Windows.FileDialog.createLoadTagsFileDialog();
            if (loadTagsDialog.ShowDialog() == false) return;

            CancellableOperationProgressView importView = new CancellableOperationProgressView();
            TagOperationsViewModel vm = new TagOperationsViewModel();
            importView.DataContext = vm;
            Task.Run(() => vm.import(loadTagsDialog.FileName));
            importView.ShowDialog();
            NrTags = 0;           
        }

        void clearTags()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to clear all unused Tags from the database?", "Clear All Unused Tags", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result == MessageBoxResult.No) return;

            CancellableOperationProgressView clearView = new CancellableOperationProgressView();
            TagOperationsViewModel vm = new TagOperationsViewModel();
            clearView.DataContext = vm;
            Task.Run(() => vm.clear());
            clearView.ShowDialog();
            NrTags = 0;

        }


        private async Task clearMedia()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to clear all Media from the database?", "Clear All Media", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result == MessageBoxResult.No) return;

            List<BaseMedia> media;

            using (MediaDbCommands mediaCommands = new MediaDbCommands())
            {
                media = mediaCommands.getAllMedia();
            }

            List<MediaFileItem> items = new List<MediaFileItem>();

            foreach (BaseMedia m in media)
            {
                items.Add(MediaFileItem.Factory.create(m.Location));
            }

            ExportViewModel export = new ExportViewModel(MediaFileWatcher.Instance.MediaState);

            CancellableOperationProgressView exportView = new CancellableOperationProgressView();
            exportView.DataContext = export;
            exportView.ShowDialog();
            await export.exportAsync(items);

            NrMedia = 0;
        }
    }
}
