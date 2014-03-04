using MediaViewer.MediaDatabase;
using MediaViewer.MediaFileModel;
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
    class ImportProgressViewModel : CloseableObservableObject, IProgress
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public ImportProgressViewModel() {

            InfoMessages = new ObservableCollection<string>();
            ItemInfo = "";

            OkCommand = new Command(() =>
            {
                OnClosingRequest();
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

        public async Task importAsync(ObservableCollection<ImportExportLocation> locations)
        {
                        
            TotalProgress = 0;

            await Task.Factory.StartNew(() =>
            {
                import(locations);

            }, cancellationToken);

            OkCommand.CanExecute = true;
            CancelCommand.CanExecute = false;

        }

        private bool getMediaFiles(System.IO.FileInfo info, object state)
        {
            if (CancellationToken.IsCancellationRequested)
            {
                return (false);
            }

            ImportExportLocation location = (state as Tuple<ImportExportLocation, List<MediaFileItem>>).Item1;
            List<MediaFileItem> items = (state as Tuple<ImportExportLocation, List<MediaFileItem>>).Item2;

            MediaFileItem addItem = null;

            switch (location.MediaType)
            {
                case Search.MediaType.All:
                    {
                        if (Utils.MediaFormatConvert.isMediaFile(info.Name))
                        {
                           addItem = new MediaFileItem(info.FullName);
                        }
                        break;
                    }
                case Search.MediaType.Images:
                    {
                        if (Utils.MediaFormatConvert.isImageFile(info.Name))
                        {
                           addItem = new MediaFileItem(info.FullName);
                        }
                        break;
                    }
                case Search.MediaType.Video:
                    {
                        if (Utils.MediaFormatConvert.isVideoFile(info.Name))
                        {
                           addItem = new MediaFileItem(info.FullName);
                        }
                        break;
                    }
            }

            if (addItem != null)
            {
                if (!items.Contains(addItem))
                {
                    items.Add(addItem);
                }
            }

            return (true);
        }

        void import(ObservableCollection<ImportExportLocation> locations)
        {
            List<MediaFileItem> items = new List<MediaFileItem>();

            foreach (ImportExportLocation location in locations)
            {
                if (CancellationToken.IsCancellationRequested) return;
                ItemInfo = "Searching files in: " + location.Location;

                Tuple<ImportExportLocation, List<MediaFileItem>> state = new Tuple<ImportExportLocation, List<MediaFileItem>>(location, items);

                Utils.FileUtils.walkDirectoryTree(new DirectoryInfo(location.Location),
                    getMediaFiles, state, location.IsRecursive);

                InfoMessages.Add("Completed searching files in: " + location.Location);
            }

            TotalProgressMax = items.Count;

            foreach (MediaFileItem item in items)
            {
                try
                {
                    if (CancellationToken.IsCancellationRequested) return;
                    ItemProgress = 0;

                    ItemInfo = "Importing: " + item.Location;

                    MediaFileWatcher.Instance.MediaState.import(item, CancellationToken);

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


    }
}
