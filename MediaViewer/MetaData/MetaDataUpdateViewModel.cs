using MediaViewer.ImageGrid;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaFileModel;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Progress;
using MediaViewer.Utils;
using MvvmFoundation.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel.Composition;
using MediaViewer.Settings;

namespace MediaViewer.MetaData
{
    class MetaDataUpdateViewModel : CloseableObservableObject, ICancellableOperationProgress, IDisposable
    {
        AppSettings Settings
        {
            get;
            set;
        }
      
        MediaState MediaState
        {
            get;
            set;
        }

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
        public const string replaceMarker = "replace:";
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

        public MetaDataUpdateViewModel(AppSettings settings, MediaFileWatcher mediaFileWatcher)
        {
            Settings = settings;
            MediaState = mediaFileWatcher.MediaState;

            WindowIcon = "pack://application:,,,/Resources/Icons/info.ico";

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
            WindowTitle = "Updating Metadata";          
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool safe)
        {
            if (safe)
            {
                if (tokenSource != null)
                {
                    tokenSource.Dispose();
                    tokenSource = null;
                }
            }
        }

        public async Task writeMetaDataAsync(MetaDataUpdateViewModelAsyncState state)
        {

            TotalProgressMax = state.ItemList.Count;
            ItemProgressMax = 100;
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

            List<Counter> counters = new List<Counter>();
            String oldPath = "", newPath = "";
            String oldFilename = "", newFilename = "";

            foreach (MediaFileItem item in state.ItemList)
            {              
                if (CancellationToken.IsCancellationRequested) return;

                ItemProgress = 0;
                bool isModified = false;

                ItemInfo = "Opening: " + item.Location;
         
                item.RWLock.EnterUpgradeableReadLock();
                try
                {
                    if (item.Media == null)
                    {
                        ItemInfo = "Loading MetaData: " + item.Location;

                        MediaState.readMetadata(item, MediaFactory.ReadOptions.AUTO |
                            MediaFactory.ReadOptions.GENERATE_THUMBNAIL, CancellationToken);

                        if (item.Media is UnknownMedia)
                        {
                            // reload metaData in metadataviewmodel                          
                            ItemInfo = "Could not open file and/or read it's metadata: " + item.Location;
                            InfoMessages.Add("Could not open file and/or read it's metadata: " + item.Location);
                            log.Error("Could not open file and/or read it's metadata: " + item.Location);
                        }
                    }


                    if (item.Media != null && !(item.Media is UnknownMedia))
                    {

                        Media media = item.Media;

                        if (state.RatingEnabled)
                        {
                            Nullable<double> oldValue = media.Rating;

                            media.Rating = state.Rating.HasValue == false ? null : state.Rating * 5;

                            if (media.Rating != oldValue)
                            {
                                isModified = true;
                            }
                        }

                        if (state.TitleEnabled && !EqualityComparer<String>.Default.Equals(media.Title, state.Title))
                        {
                            media.Title = state.Title;
                            isModified = true;
                        }

                        if (state.DescriptionEnabled && !EqualityComparer<String>.Default.Equals(media.Description, state.Description))
                        {
                            media.Description = state.Description;
                            isModified = true;
                        }

                        if (state.AuthorEnabled && !EqualityComparer<String>.Default.Equals(media.Author, state.Author))
                        {
                            media.Author = state.Author;
                            isModified = true;
                        }

                        if (state.CopyrightEnabled && !EqualityComparer<String>.Default.Equals(media.Copyright, state.Copyright))
                        {
                            media.Copyright = state.Copyright;
                            isModified = true;
                        }

                        if (state.CreationEnabled && !(Nullable.Compare<DateTime>(media.CreationDate, state.Creation) == 0))
                        {
                            media.CreationDate = state.Creation;
                            isModified = true;
                        }

                        if (state.BatchMode == false && !state.Tags.SequenceEqual(media.Tags))
                        {
                            media.Tags.Clear();
                            foreach (Tag tag in state.Tags)
                            {
                                media.Tags.Add(tag);
                            }
                            isModified = true;
                        }
                        else if (state.BatchMode == true)
                        {
                            bool addedTag = false;
                            bool removedTag = false;

                            foreach (Tag tag in state.AddTags)
                            {
                                // Hashset compares items using their gethashcode function
                                // which can be different for the same database entities created at different times
                                if (!media.Tags.Contains(tag, EqualityComparer<Tag>.Default))
                                {
                                    media.Tags.Add(tag);
                                    addedTag = true;
                                }
                            }

                            foreach (Tag tag in state.RemoveTags)
                            {
                                Tag removeTag = media.Tags.FirstOrDefault((t) => t.Name.Equals(tag.Name));

                                if (removeTag != null)
                                {
                                    media.Tags.Remove(removeTag);
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
                            MediaState.writeMetadata(item, MediaFactory.WriteOptions.AUTO, this);

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
                        newPath = newPath.TrimEnd('\\');

                        if (state.ImportedEnabled == true)
                        {
                            if (item.Media.IsImported == true && state.IsImported == false)
                            {
                                ItemInfo = "Exporting: " + item.Location;
                                MediaState.export(item, TokenSource.Token);
                                InfoMessages.Add("Exported: " + item.Location);
                            }
                        }

                        MediaState.move(item, newPath + "\\" + newFilename + ext, this);

                        if (state.ImportedEnabled == true)
                        {
                            if (item.Media.IsImported == false && state.IsImported == true)
                            {
                                ItemInfo = "Importing: " + item.Location;
                                MediaState.import(item, TokenSource.Token);
                                InfoMessages.Add("Imported: " + item.Media.Location);
                            }

                        }

                        ItemProgress = 100;
                        TotalProgress++;
                       
                    }
                    catch (Exception e)
                    {
                        if (item.Media != null)
                        {
                            item.Media.clear();
                        }

                        // reload metaData in metadataviewmodel
                        item.readMetaData(MediaFactory.ReadOptions.AUTO |
                            MediaFactory.ReadOptions.GENERATE_THUMBNAIL, CancellationToken);

                        ItemInfo = "Error updating file: " + item.Location;
                        InfoMessages.Add("Error updating file: " + item.Location + " " + e.Message);
                        log.Error("Error updating file: " + item.Location, e);
                        MessageBox.Show("Error updating file: " + item.Location + "\n\n" + e.Message,
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);                                               
                        return;
                    }

                }
                catch (Exception e)
                {
                    log.Error("Error writing metadata", e);
                    MessageBox.Show("Error writing metadata", e.Message);
                  
                }
                finally
                {
                    item.RWLock.ExitUpgradeableReadLock();                

                    GlobalMessenger.Instance.NotifyColleagues("MetaDataUpdateViewModel_UpdateComplete", item);
                }

            }
        
            if (!oldFilename.Equals(newFilename))
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {

                    Utils.Misc.insertIntoHistoryCollection(Settings.FilenameHistory, state.Filename);
                }));
            }

            if (!oldPath.Equals(newPath))
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {

                    Utils.Misc.insertIntoHistoryCollection(Settings.MetaDataUpdateDirectoryHistory, newPath);
                }));
            }


        }

        string parseNewFilename(string newFilename, string oldFilename, List<Counter> counters, Media media)
        {
            if (String.IsNullOrEmpty(newFilename) || String.IsNullOrWhiteSpace(newFilename))
            {
                return (oldFilename);
            }

            List<Tuple<String, String>> replaceArgs = new List<Tuple<string, string>>();

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
                        }
                        else if (subString.StartsWith(resolutionMarker))
                        {

                            int width = 0;
                            int height = 0;

                            if (media != null && media is ImageMedia)
                            {
                                width = (media as ImageMedia).Width;
                                height = (media as ImageMedia).Height;
                            }
                            else if (media != null && media is VideoMedia)
                            {
                                width = (media as VideoMedia).Width;
                                height = (media as VideoMedia).Height;
                            }

                            outputFileName += width.ToString() + "x" + height.ToString();

                        }
                        else if (subString.StartsWith(dateMarker))
                        {
                            String format = subString.Substring(dateMarker.Length);
                            String dateString = "";

                            if (media.CreationDate != null)
                            {
                                dateString = media.CreationDate.Value.ToString(format);
                            }

                            outputFileName += dateString;
                        }
                        else if (subString.StartsWith(replaceMarker))
                        {
                            String replaceString = subString.Substring(replaceMarker.Length);
                            int index = replaceString.IndexOf(';');

                            if (index == -1) continue;
                            Tuple<String, String> args = new Tuple<string,string>(replaceString.Substring(0,index),replaceString.Substring(index + 1));

                            if (String.IsNullOrEmpty(args.Item1) || String.IsNullOrWhiteSpace(args.Item1)) continue;
                            if (args.Item2 == null) continue;                                                       

                            replaceArgs.Add(args);
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

            foreach (Tuple<String, String> arg in replaceArgs)
            {
                outputFileName = outputFileName.Replace(arg.Item1, arg.Item2);
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

            lock (vm.Items)
            {
                ItemList = new List<MediaFileItem>(vm.Items);
            }
           
            Rating = vm.Rating;
            RatingEnabled = vm.RatingEnabled;
            Title = vm.Title;
            TitleEnabled = vm.TitleEnabled;
            IsImported = vm.IsImported;
            ImportedEnabled = vm.ImportedEnabled;
            Tags = new List<Tag>(vm.Tags);
            AddTags = new List<Tag>(vm.AddTags);
            RemoveTags = new List<Tag>(vm.RemoveTags);
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
        Nullable<double> rating;

        public Nullable<double> Rating
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

        List<Tag> tags;

        public List<Tag> Tags
        {
            get { return tags; }
            set { tags = value; }
        }

        List<Tag> addTags;

        public List<Tag> AddTags
        {
            get { return addTags; }
            set { addTags = value; }
        }

        List<Tag> removeTags;

        public List<Tag> RemoveTags
        {
            get { return removeTags; }
            set { removeTags = value; }
        }

        Nullable<DateTime> creation;

        public Nullable<DateTime> Creation
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

        bool isImported;

        public bool IsImported
        {
            get { return isImported; }
            set { isImported = value; }
        }
        bool importedEnabled;

        public bool ImportedEnabled
        {
            get { return importedEnabled; }
            set { importedEnabled = value; }
        }

        
    }
}
