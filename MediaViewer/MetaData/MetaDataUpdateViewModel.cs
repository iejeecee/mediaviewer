using MediaViewer.ImageGrid;
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

namespace MediaViewer.MetaData
{
    class MetaDataUpdateViewModel : CloseableObservableObject, IProgress
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public const string oldFilenameMarker = "@";
        public const string counterMarker = "&";

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

            CancelCommand.CanExecute = true;

            OkCommand = new Command(() =>
            {
                OnClosingRequest();
            });

            OkCommand.CanExecute = false;
        }

        public async Task writeMetaData(MetaDataUpdateViewModelAsyncState state)
        {
                                  
            TotalProgressMax = state.ItemList.Count;
            TotalProgress = 0;

            await Task.Factory.StartNew(() =>
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
                  
                    List<int> counters = new List<int>();

                    foreach (MediaFileItem item in state.ItemList)
                    {
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
                                return;
                            }
                        }

                        FileMetaData metaData = item.Media.MetaData;

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

                            //if filename/location has changed rename and/or move
                            String oldPath = FileUtils.getPathWithoutFileName(item.Location);
                            String oldFileName = Path.GetFileNameWithoutExtension(item.Location);
                            String ext = Path.GetExtension(item.Location);

                            String newFileName = parseNewFilename(state.Filename, oldFileName, counters);
                            String newPath = String.IsNullOrEmpty(state.Location) ? oldPath : state.Location;
                                                
                            fileUtils.moveFile(oldPath + "\\" + oldFileName + ext, newPath + "\\" + newFileName + ext, this);
                           
                            ItemProgress = 100;
                            TotalProgress++;
                        }
                        catch (Exception e)
                        {                           
                            item.Media.MetaData.clear();
                            // reload metaData in metadataviewmodel
                            item.IsSelected = false;
                            item.IsSelected = true;
                                                   
                            ItemInfo = "Error Saving MetaData: " + item.Location;
                            InfoMessages.Add("Could not save metaData for file: " + item.Location);
                            log.Error("Could not save metaData for file: " + item.Location, e);
                            return;
                        }
                                              
                    }
               
                }
                finally
                {
                    MediaFileWatcher.Instance.MediaFilesInUseByOperation.RemoveAll(state.ItemList);
                }

            },cancellationToken);

            OkCommand.CanExecute = true;
            CancelCommand.CanExecute = false;
        }

        string parseNewFilename(string newFilename, string oldFilename, List<int> counters)
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
                        if (subString[0].Equals(oldFilenameMarker[0]))
                        {
                            // insert old filename
                            outputFileName += oldFilename;
                        }
                        else if (subString[0].Equals(counterMarker[0]))
                        {
                            // insert counter
                            nrCounters++;
                            int counterValue;
                            bool haveCounterValue = false;

                            if (counters.Count < nrCounters)
                            {
                                haveCounterValue = int.TryParse(subString.Substring(1), out counterValue);

                                if (haveCounterValue)
                                {
                                    counters.Add(counterValue);
                                }
                            }
                            else
                            {
                                counterValue = counters[nrCounters - 1];
                                haveCounterValue = true;
                            }

                            if (haveCounterValue)
                            {

                                outputFileName += counterValue.ToString();

                                // increment counter
                                counters[nrCounters - 1] += 1;
                            }
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
    }
}
