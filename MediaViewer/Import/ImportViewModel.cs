using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Progress;
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
    class ImportViewModel : CloseableObservableObject, IProgress
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ImportViewModel()
        {
            InfoMessages = new ObservableCollection<string>();
            ItemInfo = "";

            OkCommand = new Command(() =>
            {
            });

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken = tokenSource.Token;

            CancelCommand = new Command(() =>
                {
                    tokenSource.Cancel();
                });

            OkCommand.CanExecute = false;
            CancelCommand.CanExecute = true;
        }

        public async Task importAsync(List<MediaFileItem> items)
        {
            TotalProgressMax = items.Count;
            TotalProgress = 0;
           

            await Task.Factory.StartNew(() =>
            {
                import(items);

            }, cancellationToken);

            OkCommand.CanExecute = true;
            CancelCommand.CanExecute = false;
                      
        }

        void import(List<MediaFileItem> items)
        {
            bool success = MediaFileWatcher.Instance.MediaFilesInUseByOperation.AddRange(items);
            if (success == false)
            {
                InfoMessages.Add("Cannot import files, selected file(s) are in use by another operation");
                log.Error("Cannot import files, selected file(s) are in use by another operation");
                return;
            }

            try
            {

                MediaDbCommands mediaCommands = new MediaDbCommands();

                foreach (MediaFileItem item in items)
                {
                    try
                    {
                        if (CancellationToken.IsCancellationRequested) return;
                        ItemProgress = 0;

                        if (item.Media == null || item.Media.MetaData == null)
                        {
                            ItemInfo = "Loading MetaData: " + item.Location;

                            item.loadMetaData(MediaFileModel.MediaFile.MetaDataLoadOptions.LOAD_FROM_DISK | MediaFileModel.MediaFile.MetaDataLoadOptions.GENERATE_THUMBNAIL
                            , CancellationToken);
                            if (item.Media == null || item.Media.MetaData == null)
                            {
                                ItemInfo = "Could not open file and/or read it's metadata: " + item.Location;
                                InfoMessages.Add("Could not open file and/or read it's metadata: " + item.Location);
                                log.Error("Could not open file and/or read it's metadata: " + item.Location);
                            }
                        }

                        ItemInfo = "Importing: " + item.Location;

                        ImportState.Instance.import(item, mediaCommands.Db);
                                                   
                        ItemProgress = 100;
                        TotalProgress++;
                        InfoMessages.Add("Imported: " + item.Location);
                    }
                    catch (Exception e)
                    {
                        ItemInfo = "Error importing file: " + item.Location;
                        InfoMessages.Add("Error importing file: " + item.Location + " " + e.Message);
                        log.Error("Error importing file: " + item.Location, e);
                        MessageBox.Show("Error importing file: " + item.Location + "\n\n" + e.Message,
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;

                    }
                }

            }
            finally
            {
                MediaFileWatcher.Instance.MediaFilesInUseByOperation.RemoveAll(items);
            }
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

        String itemInfo;

        public String ItemInfo
        {
            get { return itemInfo; }
            set
            {
                itemInfo = value;
                NotifyPropertyChanged();
            }
        }

        ObservableCollection<String> infoMessages;

        public ObservableCollection<String> InfoMessages
        {
            get { return infoMessages; }
            set
            {
                infoMessages = value;
                NotifyPropertyChanged();
            }
        }

        CancellationToken cancellationToken;

        public CancellationToken CancellationToken
        {
            get { return cancellationToken; }
            set { cancellationToken = value; }
        }

        int totalProgress;

        public int TotalProgress
        {
            get
            {
                return (totalProgress);
            }
            set
            {
                totalProgress = value;
                NotifyPropertyChanged();
            }
        }

        int totalProgressMax;

        public int TotalProgressMax
        {
            get
            {
                return (totalProgressMax);
            }
            set
            {
                totalProgressMax = value;
                NotifyPropertyChanged();
            }
        }

        int itemProgress;

        public int ItemProgress
        {
            get
            {
                return (itemProgress);
            }
            set
            {
                itemProgress = value;
                NotifyPropertyChanged();
            }
        }

        int itemProgressMax;

        public int ItemProgressMax
        {
            get
            {
                return (itemProgressMax);
            }
            set
            {
                itemProgressMax = value;
                NotifyPropertyChanged();
            }
        }

         
    }

 
}
