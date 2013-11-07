using MediaViewer.ImageGrid;
using MediaViewer.MediaFileModel.Watcher;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.MetaData
{
    class MetaDataUpdateViewModel : CloseableObservableObject
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        CancellationTokenSource tokenSource;

        public CancellationTokenSource TokenSource
        {
            get { return tokenSource; }
            set { tokenSource = value; }
        }
       
        public MetaDataUpdateViewModel()
        {
            InfoMessages = new ObservableCollection<string>();
            tokenSource = new CancellationTokenSource();
            CancellationToken = tokenSource.Token;

            CancelCommand = new Command(() => {

                TokenSource.Cancel();
            });

            OkCommand = new Command(() =>
            {
                OnClosingRequest();
            });
        }

        public async Task writeMetaData(MetaDataUpdateViewModelAsyncState state)
        {
                      
            // copy the viewmodel state to prevent outside changes to it influencing this function
            MetaDataViewModel vmState = state.State;

            TotalFiles = vmState.ItemList.Count;
            CurrentFile = 0;

            await Task.Factory.StartNew(() =>
            {
                foreach (MediaFileItem item in vmState.ItemList)
                {
                    if (CancellationToken.IsCancellationRequested) return;

                    CurrentFileProgress = 0;
                    bool isModified = false;

                    ItemInfo = "Opening: " + item.Location;

                    if (item.Media == null || item.Media.MetaData == null)
                    {
                        ItemInfo = "Loading MetaData: " + item.Location;

                        item.loadMetaData(CancellationToken);
                        if (item.Media == null || item.Media.MetaData == null)
                        {
                            ItemInfo = "Could not open or read metadata for file: " + item.Location;
                            InfoMessages.Add("Could not open or read metadata for file: " + item.Location);
                            log.Error("Could not open or read metadata for file: " + item.Location);
                            return;
                        }
                    }

                    FileMetaData metaData = item.Media.MetaData;

                    if (vmState.RatingEnabled && (int)metaData.Rating != (int)(vmState.Rating * 5))
                    {                       
                        metaData.Rating = vmState.Rating * 5;
                        isModified = true;
                    }

                    if (vmState.TitleEnabled && !metaData.Title.Equals(vmState.Title))
                    {
                        metaData.Title = vmState.Title;
                        isModified = true;
                    }

                    if (vmState.DescriptionEnabled && !metaData.Description.Equals(vmState.Description))
                    {
                        metaData.Description = vmState.Description;
                        isModified = true;
                    }

                    if (vmState.AuthorEnabled && !metaData.Creator.Equals(vmState.Author))
                    {
                        metaData.Creator = vmState.Author;
                        isModified = true;
                    }

                    if (vmState.CopyrightEnabled && !metaData.Copyright.Equals(vmState.Copyright))
                    {
                        metaData.Copyright = vmState.Copyright;
                        isModified = true;
                    }

                    ItemInfo = "Saving MetaData: " + item.Location;

                    try
                    {
                        if (isModified)
                        {
                            metaData.saveToDisk();
                        }
                    }
                    catch (Exception e)
                    {                  
                        ItemInfo = "Error Saving MetaData: " + item.Location;
                        InfoMessages.Add("Could not save metaData for file: " + item.Location);
                        log.Error("Could not save metaData for file: " + item.Location, e);
                        return;
                    }
                   
                    InfoMessages.Add("Completed updating Metadata for: " + item.Location);

                    CurrentFileProgress = 100;
                    CurrentFile++;
                }

                if (vmState.ItemList.Count == 1 && !Path.GetFileName(vmState.ItemList[0].Location).Equals(vmState.Filename))
                {
                    String oldName = vmState.ItemList[0].Location;
                    String destPath = Utils.FileUtils.getPathWithoutFileName(oldName);
                    String newName = destPath + "\\" + vmState.Filename;

                    try
                    {
                        System.IO.File.Move(oldName, newName);
                    }
                    catch (Exception e)
                    {
                        ItemInfo = "Error renaming file: " + oldName;
                        InfoMessages.Add("Error renaming file: " + oldName);
                        log.Error("Error renaming file: " + oldName, e);
                        return;
                    }

                }

            },cancellationToken);
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

        int totalFiles;

        public int TotalFiles
        {
            get { return totalFiles; }
            set
            {
                totalFiles = value;
                NotifyPropertyChanged();
            }
        }
        int currentFile;

        public int CurrentFile
        {
            get { return currentFile; }
            set
            {
                currentFile = value;
                NotifyPropertyChanged();
            }
        }

        int currentFileProgress;

        public int CurrentFileProgress
        {
            get { return currentFileProgress; }
            set
            {
                currentFileProgress = value;
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
    }

    class MetaDataUpdateViewModelAsyncState
    {
        MetaDataViewModel state;

        public MetaDataViewModel State
        {
            get { return state; }            
        }

        public MetaDataUpdateViewModelAsyncState(MetaDataViewModel vm)
        {
            state = vm.Clone() as MetaDataViewModel;
        }
    }
}
