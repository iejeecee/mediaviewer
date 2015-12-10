using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Progress;
using Microsoft.Practices.Prism.Mvvm;
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
using MediaViewer.Model.Settings;
using MediaViewer.Model.Media.Base.State;
using MediaViewer.Model.Utils;
using Microsoft.Practices.Prism.PubSubEvents;
using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Commands;
using MediaViewer.Model.Global.Commands;
using MediaViewer.Infrastructure.Logging;
using MediaViewer.Model.Concurrency;
using MediaViewer.Model.metadata.Metadata;
using MediaViewer.Properties;

namespace MediaViewer.MetaData
{
    class MetaDataUpdateViewModel : CancellableOperationProgressBase
    {
        MediaFileState MediaFileState { get; set; }
        IEventAggregator EventAggregator { get; set; }

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

        public const string oldFilenameMarker = "filename";
        public const string counterMarker = "counter:";
        public const string replaceMarker = "replace:";
        public const string defaultCounter = "0001";
        public const string resolutionMarker = "resolution";
        public const string dateMarker = "date:";
        public const string defaultDateFormat = "g";

        public MetaDataUpdateViewModel(MediaFileWatcher mediaFileWatcher, IEventAggregator eventAggregator)
        {
            MediaFileState = mediaFileWatcher.MediaFileState;
            EventAggregator = eventAggregator;

            WindowTitle = "Updating Metadata";
            WindowIcon = "pack://application:,,,/Resources/Icons/info.ico";

            CancelCommand.IsExecutable = true;
            OkCommand.IsExecutable = false;


        }

        public async Task writeMetaDataAsync(MetaDataUpdateViewModelAsyncState state)
        {
            TotalProgressMax = state.ItemList.Count;
            ItemProgressMax = 100;
            TotalProgress = 0;

            await Task.Factory.StartNew(() =>
            {
                GlobalCommands.MetaDataUpdateCommand.Execute(state);
                writeMetaData(state);

            }, CancellationToken, TaskCreationOptions.None, PriorityScheduler.BelowNormal);

            OkCommand.IsExecutable = true;
            CancelCommand.IsExecutable = false;
        }

        void writeMetaData(MetaDataUpdateViewModelAsyncState state)
        {
            List<Counter> counters = new List<Counter>();
            String oldPath = "", newPath = "";
            String oldFilename = "", newFilename = "", ext = "";

            foreach (MediaFileItem item in state.ItemList)
            {
                if (CancellationToken.IsCancellationRequested) return;

                ItemProgress = 0;
                ItemInfo = "Opening: " + item.Location;
                bool isModified = false;

                try
                {
                    // Update Metadata values
                    item.EnterWriteLock();
                    try
                    {
                        isModified = updateMetadata(item, state);
                    }
                    catch (Exception e)
                    {
                        string info = "Error updating Metadata: " + item.Location;

                        InfoMessages.Add(info);
                        Logger.Log.Error(info, e);
                        MessageBox.Show(info + "\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    finally
                    {
                        item.ExitWriteLock();
                    }

                    // Save Metadata to disk
                    if (isModified)
                    {
                        String info;
                        ItemInfo = "Saving MetaData: " + item.Location;

                        item.EnterUpgradeableReadLock();
                        try
                        {
                            item.writeMetadata_URLock(MetadataFactory.WriteOptions.AUTO, this);
                        }
                        catch (Exception e)
                        {
                            info = "Error saving Metadata: " + item.Location;

                            InfoMessages.Add(info);
                            Logger.Log.Error(info, e);
                            MessageBox.Show(info + "\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                            // reload metaData, exceptions are caught in readMetadata
                            if (item.Metadata != null)
                            {
                                item.Metadata.clear();
                            }
                           
                            item.readMetadata_URLock(MetadataFactory.ReadOptions.AUTO |
                                MetadataFactory.ReadOptions.GENERATE_THUMBNAIL, CancellationToken);

                            return;
                        }
                        finally
                        {
                            item.ExitUpgradeableReadLock();
                        }

                        info = "Completed updating Metadata for: " + item.Location;

                        InfoMessages.Add(info);
                        Logger.Log.Info(info);
                    }
                    else
                    {
                        string info = "Skipped updating Metadata (no changes) for: " + item.Location;

                        InfoMessages.Add(info);
                        Logger.Log.Info(info);
                    }

                    // Export if requested
                    if (state.ImportedEnabled == true && state.IsImported == false)
                    {
                        bool success = MediaFileState.export(item, CancellationToken);

                        if (success)
                        {
                            string info = "Exported: " + item.Location;

                            InfoMessages.Add(info);
                            Logger.Log.Info(info);
                        }
                    }

                    //rename and/or move   
                    item.EnterReadLock();
                    try
                    {
                        oldPath = FileUtils.getPathWithoutFileName(item.Location);
                        oldFilename = Path.GetFileNameWithoutExtension(item.Location);
                        ext = Path.GetExtension(item.Location);

                        newFilename = parseNewFilename(state.Filename, oldFilename, counters, item.Metadata);
                        newPath = String.IsNullOrEmpty(state.Location) ? oldPath : state.Location;
                        newPath = newPath.TrimEnd('\\');
                    }
                    finally
                    {
                        item.ExitReadLock();
                    }

                    try
                    {
                        MediaFileState.move(item, newPath + "\\" + newFilename + ext, this);
                    }
                    catch (Exception e)
                    {
                        string info = "Error moving/renaming: " + item.Location;

                        InfoMessages.Add(info);
                        Logger.Log.Error(info, e);
                        MessageBox.Show(info + "\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;

                    }

                    // import if requested
                    if (state.ImportedEnabled == true && state.IsImported == true)
                    {
                        bool success = false;
                        try
                        {
                            success = MediaFileState.import(item, CancellationToken);
                        }
                        catch (Exception e)
                        {
                            string info = "Error importing media: " + item.Location;

                            InfoMessages.Add(info);
                            Logger.Log.Error(info, e);
                            MessageBox.Show(info + "\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (success)
                        {
                            string info = "Imported: " + item.Metadata.Location;

                            InfoMessages.Add(info);
                            Logger.Log.Info(info);
                        }

                    }

                    ItemProgress = 100;
                    TotalProgress++;

                }
                finally
                {
                    EventAggregator.GetEvent<MetaDataUpdateCompleteEvent>().Publish(item);
                }

            }

            if (state.BatchMode == true)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MiscUtils.insertIntoHistoryCollection(Settings.Default.FilenameHistory, state.Filename);
                }));

            }
            else if (!oldFilename.Equals(newFilename))
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MiscUtils.insertIntoHistoryCollection(Settings.Default.FilenameHistory, newFilename);
                }));
            }

            if (!oldPath.Equals(newPath))
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MiscUtils.insertIntoHistoryCollection(Settings.Default.MetaDataUpdateDirectoryHistory, newPath);
                }));
            }


        }

        bool updateMetadata(MediaFileItem item, MetaDataUpdateViewModelAsyncState state)
        {
            bool isModified = false;

            if (item.Metadata == null || item.Metadata is UnknownMetadata)
            {
                throw new Exception("Missing or invalid metadata in media");
            }

            isModified = item.Metadata.IsModified;

            BaseMetadata media = item.Metadata;

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

            if (state.IsGeoTagEnabled && !(Nullable.Compare<double>(media.Latitude, state.Latitude) == 0))
            {
                media.Latitude = state.Latitude;
                isModified = true;
            }

            if (state.IsGeoTagEnabled && !(Nullable.Compare<double>(media.Longitude, state.Longitude) == 0))
            {
                media.Longitude = state.Longitude;
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

            return (isModified);
        }

        

        string parseNewFilename(string newFilename, string oldFilename, List<Counter> counters, BaseMetadata media)
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

                            if (media != null && media is ImageMetadata)
                            {
                                width = (media as ImageMetadata).Width;
                                height = (media as ImageMetadata).Height;
                            }
                            else if (media != null && media is VideoMetadata)
                            {
                                width = (media as VideoMetadata).Width;
                                height = (media as VideoMetadata).Height;
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
                            Tuple<String, String> args = new Tuple<string, string>(replaceString.Substring(0, index), replaceString.Substring(index + 1));

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

            outputFileName = FileUtils.removeIllegalCharsFromFileName(outputFileName, "-");

            return (outputFileName);
        }

    }

    public class MetaDataUpdateViewModelAsyncState
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
            IsEnabled = vm.IsEnabled;

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

            IsGeoTagEnabled = vm.IsGeotagEnabled;
            Latitude = vm.Geotag.LatDecimal;
            Longitude = vm.Geotag.LonDecimal;
        }

        public String Location { get; set; }
        public String Author { get; set; }
        public bool AuthorEnabled { get; set; }
        public bool BatchMode { get; set; }
        public String Copyright { get; set; }
        public bool CopyrightEnabled { get; set; }
        public String Description { get; set; }
        public bool DescriptionEnabled { get; set; }
        public String Filename { get; set; }
        public bool IsEnabled { get; set; }
        public List<MediaFileItem> ItemList { get; set; }
        public Nullable<double> Rating { get; set; }
        public bool RatingEnabled { get; set; }
        public String Title { get; set; }
        public bool TitleEnabled { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Tag> AddTags { get; set; }
        public List<Tag> RemoveTags { get; set; }
        public Nullable<DateTime> Creation { get; set; }
        public bool CreationEnabled { get; set; }
        public bool IsImported { get; set; }
        public bool ImportedEnabled { get; set; }
        public Nullable<double> Latitude { get; set; }
        public Nullable<double> Longitude { get; set; }
        public bool IsGeoTagEnabled { get; set; }

    }
}
