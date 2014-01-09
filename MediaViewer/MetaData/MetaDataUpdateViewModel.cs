using MediaViewer.ImageGrid;
using MediaViewer.MediaFileModel;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Progress;
using MediaViewer.Utils;
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

namespace MediaViewer.MetaData
{
    class MetaDataUpdateViewModel : CloseableObservableObject, IProgress
    {
        class Counter
        {
            public Counter(int value, int nrDigits)
            {
                this.value = value;
                this.nrDigits = nrDigits;
            }

            public int value;
            public int nrDigits;
        };

        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const string oldFilenameMarker = "filename";
        public const string counterMarker = "counter:";
        public const string defaultCounter = "0001";
        public const string resolutionMarker = "resolution";
        public const string dateMarker = "date:";
        public const string defaultDateFormat = "g";

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

            CancelCommand = new Command(() =>
            {

                TokenSource.Cancel();
            });

            CancelCommand.CanExecute = true;

            OkCommand = new Command(() =>
            {
                OnClosingRequest();
            });

            OkCommand.CanExecute = false;
            setWindowTitle();
        }

        public async Task writeMetaDataAsync(MetaDataUpdateViewModelAsyncState state)
        {

            TotalProgressMax = state.ItemList.Count;
            TotalProgress = 0;

            await Task.Factory.StartNew(() =>
            {
                writeMetaData(state);

            }, cancellationToken);

            OkCommand.CanExecute = true;
            CancelCommand.CanExecute = false;
        }

        void writeMetaData(MetaDataUpdateViewModelAsyncState state)
        {
            FileUtils fileUtils = new FileUtils();

            bool success = MediaFileWatcher.Instance.MediaFilesInUseByOperation.AddRange(state.ItemList);
            if (success == false)
            {
                InfoMessages.Add("Error selected file(s) are in use by another operation");
                log.Error("Error selected file(s) are in use by another operation");
                return;
            }

            try
            {

                List<Counter> counters = new List<Counter>();
                String oldPath = "", newPath = "";
                String oldFilename = "", newFilename = "";

                foreach (MediaFileItem item in state.ItemList)
                {
                    setWindowTitle();
                    if (CancellationToken.IsCancellationRequested) return;

                    ItemProgress = 0;
                    bool isModified = false;

                    ItemInfo = "Opening: " + item.Location;

                    if (item.Media == null || item.Media.MetaData == null)
                    {
                        ItemInfo = "Loading MetaData: " + item.Location;

                        item.loadMetaData(MediaFileModel.MediaFile.MetaDataLoadOptions.AUTO | MediaFileModel.MediaFile.MetaDataLoadOptions.GENERATE_THUMBNAIL
                        , CancellationToken);
                        if (item.Media == null || item.Media.MetaData == null)
                        {
                            // reload metaData in metadataviewmodel
                            item.IsSelected = false;
                            item.IsSelected = true;

                            ItemInfo = "Could not open file and/or read it's metadata: " + item.Location;
                            InfoMessages.Add("Could not open file and/or read it's metadata: " + item.Location);
                            log.Error("Could not open file and/or read it's metadata: " + item.Location);                           
                        }
                    }

                    FileMetaData metaData = null;

                    if (item.Media != null && item.Media.MetaData != null)
                    {

                        metaData = item.Media.MetaData;

                        if (state.RatingEnabled && (int)metaData.Rating != (int)(state.Rating * 5))
                        {
                            metaData.Rating = state.Rating * 5;
                            isModified = true;
                        }

                        if (state.TitleEnabled && !metaData.Title.Equals(state.Title))
                        {
                            metaData.Title = state.Title;
                            isModified = true;
                        }

                        if (state.DescriptionEnabled && !metaData.Description.Equals(state.Description))
                        {
                            metaData.Description = state.Description;
                            isModified = true;
                        }

                        if (state.AuthorEnabled && !metaData.Creator.Equals(state.Author))
                        {
                            metaData.Creator = state.Author;
                            isModified = true;
                        }

                        if (state.CopyrightEnabled && !metaData.Copyright.Equals(state.Copyright))
                        {
                            metaData.Copyright = state.Copyright;
                            isModified = true;
                        }

                        if (state.CreationEnabled && !metaData.CreationDate.Equals(state.Creation))
                        {
                            metaData.CreationDate = state.Creation;
                            isModified = true;
                        }

                        if (state.BatchMode == false && !state.Tags.SequenceEqual(metaData.Tags))
                        {
                            metaData.Tags.Clear();
                            metaData.Tags.AddRange(state.Tags);
                            isModified = true;
                        }
                        else if (state.BatchMode == true)
                        {
                            bool addedTag = false;
                            bool removedTag = false;

                            foreach (string tag in state.AddTags)
                            {
                                if (!metaData.Tags.Contains(tag))
                                {
                                    metaData.Tags.Add(tag);
                                    addedTag = true;
                                }
                            }

                            foreach (string tag in state.RemoveTags)
                            {
                                if (metaData.Tags.Remove(tag) == true)
                                {
                                    removedTag = true;
                                }

                            }

                            if (removedTag || addedTag)
                            {
                                isModified = true;
                            }

                        }

                    }

                    try
                    {
                        if (isModified)
                        {
                            // Save metadata changes
                            ItemInfo = "Saving MetaData: " + item.Location;
                            metaData.saveToDisk();

                            InfoMessages.Add("Completed updating Metadata for: " + item.Location);
                        }
                        else
                        {
                            InfoMessages.Add("Skipped updating Metadata (no changes) for: " + item.Location);
                        }

                        //rename and/or move
                        oldPath = FileUtils.getPathWithoutFileName(item.Location);
                        oldFilename = Path.GetFileNameWithoutExtension(item.Location);
                        String ext = Path.GetExtension(item.Location);

                        newFilename = parseNewFilename(state.Filename, oldFilename, counters, item.Media);
                        newPath = String.IsNullOrEmpty(state.Location) ? oldPath : state.Location;
                     
                        fileUtils.moveFile(oldPath + "\\" + oldFilename + ext, newPath + "\\" + newFilename + ext, this);

                        ItemProgress = 100;
                        TotalProgress++;
                    }
                    catch (Exception e)
                    {
                        if (item.Media != null && item.Media.MetaData != null)
                        {
                            item.Media.MetaData.clear();
                        }
                        // reload metaData in metadataviewmodel
                        item.IsSelected = false;
                        item.IsSelected = true;

                        ItemInfo = "Error updating file: " + item.Location;
                        InfoMessages.Add("Error updating file: " + item.Location + " " + e.Message);
                        log.Error("Error updating file: " + item.Location, e);
                        MessageBox.Show("Error updating file: " + item.Location + "\n\n" + e.Message,
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                }

                setWindowTitle();

                if (!oldFilename.Equals(newFilename))
                {
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {

                        Utils.Misc.insertIntoHistoryCollection(Settings.AppSettings.Instance.FilenameHistory, state.Filename);
                    }));
                }

                if (!oldPath.Equals(newPath))
                {
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {

                        Utils.Misc.insertIntoHistoryCollection(Settings.AppSettings.Instance.MetaDataUpdateDirectoryHistory, newPath);
                    }));
                }
            }
            finally
            {
                MediaFileWatcher.Instance.MediaFilesInUseByOperation.RemoveAll(state.ItemList);
            }
        }

        string parseNewFilename(string newFilename, string oldFilename, List<Counter> counters, MediaFile media)
        {
            if (String.IsNullOrEmpty(newFilename) || String.IsNullOrWhiteSpace(newFilename))
            {
                return (oldFilename);
            }

            int nrCounters = 0;
            string outputFileName = "";

            for (int i = 0; i < newFilename.Length; i++)
            {

                if (newFilename[i].Equals('\"'))
                {
                    // grab substring
                    string subString = "";

                    int k = i + 1;

                    while (k < newFilename.Length && !newFilename[k].Equals('\"'))
                    {
                        subString += newFilename[k];
                        k++;
                    }

                    // replace
                    if (subString.Length > 0)
                    {
                        if (subString.StartsWith(oldFilenameMarker))
                        {
                            // insert old filename
                            outputFileName += oldFilename;
                        }
                        else if (subString.StartsWith(counterMarker))
                        {
                            // insert counter
                            nrCounters++;
                            int counterValue;
                            bool haveCounterValue = false;

                            if (counters.Count < nrCounters)
                            {
                                String stringValue = subString.Substring(counterMarker.Length);
                                haveCounterValue = int.TryParse(stringValue, out counterValue);

                                if (haveCounterValue)
                                {
                                    counters.Add(new Counter(counterValue, stringValue.Length));
                                }
                            }
                            else
                            {
                                counterValue = counters[nrCounters - 1].value;
                                haveCounterValue = true;
                            }

                            if (haveCounterValue)
                            {

                                outputFileName += counterValue.ToString("D" + counters[nrCounters - 1].nrDigits);

                                // increment counter
                                counters[nrCounters - 1].value += 1;
                            }
                        } else if(subString.StartsWith(resolutionMarker)) {

                            int width = 0;
                            int height = 0;

                            if (media != null && media is ImageFile)
                            {
                                width = (media as ImageFile).Width;
                                height = (media as ImageFile).Height;
                            }
                            else if (media != null && media is VideoFile)
                            {
                                width = (media as VideoFile).Width;
                                height = (media as VideoFile).Height;
                            }

                            outputFileName += width.ToString() + "x" + height.ToString();

                        }
                        else if (subString.StartsWith(dateMarker))
                        {
                            String format = subString.Substring(dateMarker.Length);
                            String dateString = "";

                            if (media != null && media.MetaData != null && !media.MetaData.CreationDate.Equals(DateTime.MinValue))
                            {
                                dateString = media.MetaData.CreationDate.ToString(format);                                                              
                            }

                            outputFileName += dateString;
                        }
                    }

                    if (newFilename[k].Equals('\"'))
                    {
                        i = k;
                    }
                }
                else
                {
                    outputFileName += newFilename[i];
                }
            }

            outputFileName = Utils.FileUtils.removeIllegalCharsFromFileName(outputFileName, "-");

            return (outputFileName);
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

        void setWindowTitle()
        {
            WindowTitle = "Updating Metadata - Completed: " + TotalProgress.ToString() + "/" + TotalProgressMax.ToString() + " file(s)";
        }

        String windowTitle;

        public String WindowTitle
        {
            get { return windowTitle; }
            set
            {
                windowTitle = value;
                NotifyPropertyChanged();
            }
        }
    }

    class MetaDataUpdateViewModelAsyncState
    {
        public MetaDataUpdateViewModelAsyncState(MetaDataViewModel vm)
        {
            Location = vm.Location;
            Author = vm.Author;
            AuthorEnabled = vm.AuthorEnabled;
            BatchMode = vm.BatchMode;
            Copyright = vm.Copyright;
            CopyrightEnabled = vm.CopyrightEnabled;
            Creation = vm.Creation;
            CreationEnabled = vm.CreationEnabled;
            Description = vm.Description;
            DescriptionEnabled = vm.DescriptionEnabled;
            Filename = vm.Filename;
            isEnabled = vm.IsEnabled;
            ItemList = new List<MediaFileItem>(vm.ItemList);
            Rating = vm.Rating;
            RatingEnabled = vm.RatingEnabled;
            Title = vm.Title;
            TitleEnabled = vm.TitleEnabled;
            Tags = new List<String>(vm.Tags);
            AddTags = new List<String>(vm.AddTags);
            RemoveTags = new List<String>(vm.RemoveTags);
        }

        String location;

        public String Location
        {
            get { return location; }
            set { location = value; }
        }

        String author;

        public String Author
        {
            get { return author; }
            set { author = value; }
        }
        bool authorEnabled;

        public bool AuthorEnabled
        {
            get { return authorEnabled; }
            set { authorEnabled = value; }
        }
        bool batchMode;

        public bool BatchMode
        {
            get { return batchMode; }
            set { batchMode = value; }
        }
        String copyright;

        public String Copyright
        {
            get { return copyright; }
            set { copyright = value; }
        }
        bool copyrightEnabled;

        public bool CopyrightEnabled
        {
            get { return copyrightEnabled; }
            set { copyrightEnabled = value; }
        }
        String description;

        public String Description
        {
            get { return description; }
            set { description = value; }
        }
        bool descriptionEnabled;

        public bool DescriptionEnabled
        {
            get { return descriptionEnabled; }
            set { descriptionEnabled = value; }
        }

        String filename;

        public String Filename
        {
            get { return filename; }
            set { filename = value; }
        }

        bool isEnabled;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }
        List<MediaFileItem> itemList;

        public List<MediaFileItem> ItemList
        {
            get { return itemList; }
            set { itemList = value; }
        }
        float rating;

        public float Rating
        {
            get { return rating; }
            set { rating = value; }
        }
        bool ratingEnabled;

        public bool RatingEnabled
        {
            get { return ratingEnabled; }
            set { ratingEnabled = value; }
        }
        String title;

        public String Title
        {
            get { return title; }
            set { title = value; }
        }
        bool titleEnabled;

        public bool TitleEnabled
        {
            get { return titleEnabled; }
            set { titleEnabled = value; }
        }

        List<String> tags;

        public List<String> Tags
        {
            get { return tags; }
            set { tags = value; }
        }

        List<String> addTags;

        public List<String> AddTags
        {
            get { return addTags; }
            set { addTags = value; }
        }

        List<String> removeTags;

        public List<String> RemoveTags
        {
            get { return removeTags; }
            set { removeTags = value; }
        }

        DateTime creation;

        public DateTime Creation
        {
            get { return creation; }
            set { creation = value; }
        }

        bool creationEnabled;

        public bool CreationEnabled
        {
            get { return creationEnabled; }
            set { creationEnabled = value; }
        }
    }
}
