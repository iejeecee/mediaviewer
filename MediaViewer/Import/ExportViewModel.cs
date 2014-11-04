using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Mvvm;
using MediaViewer.Progress;
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

namespace MediaViewer.Export
{

    class ExportViewModel : CloseableBindableBase, ICancellableOperationProgress
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

            OkCommand.IsExecutable = false;
            CancelCommand.IsExecutable = true;
        }

        public async Task exportAsync(ICollection<MediaFileItem> items)
        {
            TotalProgressMax = items.Count;
            TotalProgress = 0;


            await Task.Factory.StartNew(() =>
            {
                export(items);

            }, cancellationToken);

            OkCommand.IsExecutable = true;
            CancelCommand.IsExecutable = false;

        }

        void export(ICollection<MediaFileItem> items)
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
      
        public Command OkCommand {get;set;}
          
        public Command CancelCommand {get;set;}
   

        String itemInfo;

        public String ItemInfo
        {
            get { return itemInfo; }
            set
            {            
                SetProperty(ref itemInfo, value);            
            }
        }

        ObservableCollection<String> infoMessages;

        public ObservableCollection<String> InfoMessages
        {
            get { return infoMessages; }
            set
            {
                SetProperty(ref infoMessages, value);          
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
                SetProperty(ref totalProgress, value);       
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
                SetProperty(ref totalProgressMax, value);            
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
                SetProperty(ref itemProgress, value);             
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
                SetProperty(ref itemProgressMax, value);         
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
                SetProperty(ref windowTitle, value);         
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
                SetProperty(ref windowIcon, value);          
            }
        }
    }


}
