using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Media.State;
using MediaViewer.Progress;
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

namespace MediaViewer.Export
{

    class ExportViewModel : CloseableObservableObject, ICancellableOperationProgress
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
     
        MediaState MediaState
        {
            get;
            set;
        }
      
        public ExportViewModel(MediaState mediaState)
        {

            MediaState = mediaState;

            WindowTitle = "Exporting Media";
            WindowIcon = "pack://application:,,,/Resources/Icons/export.ico";

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

        public async Task exportAsync(List<MediaFileItem> items)
        {
            TotalProgressMax = items.Count;
            TotalProgress = 0;


            await Task.Factory.StartNew(() =>
            {
                export(items);

            }, cancellationToken);

            OkCommand.CanExecute = true;
            CancelCommand.CanExecute = false;

        }

        void export(List<MediaFileItem> items)
        {
            foreach (MediaFileItem item in items)
            {
                try
                {
                    if (CancellationToken.IsCancellationRequested) return;
                    ItemProgress = 0;

                    if (item.Media == null)
                    {
                        ItemInfo = "Reading Metadata: " + item.Location;

                        MediaState.readMetadata(item, MediaFactory.ReadOptions.AUTO, CancellationToken);
                        if (item.Media is UnknownMedia)
                        {
                            ItemInfo = "Could not open file and/or read it's metadata: " + item.Location;
                            InfoMessages.Add("Could not open file and/or read it's metadata: " + item.Location);
                            log.Error("Could not open file and/or read it's metadata: " + item.Location);
                        }
                    }

                    if (item.Media.IsImported == false)
                    {
                        InfoMessages.Add("Skipping non-imported file: " + item.Location);
                        ItemProgress = 100;
                        TotalProgress++;
                        continue;
                    }

                    ItemInfo = "Exporting: " + item.Location;

                    MediaState.export(item, CancellationToken);

                    ItemProgress = 100;
                    TotalProgress++;
                    InfoMessages.Add("Exported: " + item.Location);
                }
                catch (Exception e)
                {
                    ItemInfo = "Error exporting file: " + item.Location;
                    InfoMessages.Add("Error exporting file: " + item.Location + " " + e.Message);
                    log.Error("Error exporting file: " + item.Location, e);
                    MessageBox.Show("Error exporting file: " + item.Location + "\n\n" + e.Message,
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

        string windowTitle;

        public string WindowTitle
        {
            get
            {
                return (windowTitle);
            }
            set
            {
                windowTitle = value;
                NotifyPropertyChanged();
            }
        }

        string windowIcon;

        public string WindowIcon
        {
            get
            {
                return (windowIcon);
            }
            set
            {
                windowIcon = value;
                NotifyPropertyChanged();
            }
        }
    }


}
