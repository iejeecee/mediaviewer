using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MediaViewer.Model.Media.File
{
    public class MediaFileItem : BindableBase, IEquatable<MediaFileItem>, IComparable<MediaFileItem>, IDisposable
    {

        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        ReaderWriterLockSlim rwLock;

        public ReaderWriterLockSlim RWLock
        {
            get { return rwLock; }
            set { rwLock = value; }
        }

        Guid id;

        /// <summary>
        /// Unique mediafileitem identifier
        /// </summary>
        public Guid Id
        {
            get { return id; }
        }

        protected MediaFileItem(String location, MediaFileItemState state = MediaFileItemState.EMPTY)
        {
            rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

            this.id = Id;
            Location = location;     
            Media = null;
            ItemState = state;
            hasTags = false;
            hasGeoTag = false;
            id = Guid.NewGuid();
    
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool safe)
        {
            if (safe)
            {
                if (rwLock != null)
                {
                    rwLock.Dispose();
                    rwLock = null;
                }
            }
        }

        MediaFileItemState itemState;

        /// <summary>
        /// Current state of the mediafileitem
        /// </summary>
        public MediaFileItemState ItemState
        {
            get { return itemState; }
            set
            {
                rwLock.EnterWriteLock();
                try
                {
                    SetProperty(ref itemState, value);                                      
                }
                finally
                {
                    rwLock.ExitWriteLock();                                     
                }
            }
        }
       
        string location;

        /// <summary>
        /// Location on disk of the mediafileitem
        /// </summary>
        public string Location
        {
            get { return location; }
            set
            {                              
                rwLock.EnterWriteLock();
                try
                {
                    String oldLocation = location; 
                    String newLocation = value;
                    

                    if (!String.IsNullOrEmpty(oldLocation) && !String.IsNullOrEmpty(newLocation))
                    {
                        // check if newLocation has changed
                        if (oldLocation.Equals(newLocation)) return;
                     
                        // update newLocation in dictionary
                        Factory.renameInDictionary(oldLocation, newLocation);

                        // update newLocation in the database
                        if (Media != null)
                        {
                            Media.Location = newLocation;

                            if (Media.IsImported)
                            {
                                using (MediaDbCommands mediaCommands = new MediaDbCommands())
                                {
                                    Media = mediaCommands.update(Media);
                                }
                            }
                        }
                        else
                        {
                            using (MediaDbCommands mediaCommands = new MediaDbCommands())
                            {
                                Media = mediaCommands.findMediaByLocation(oldLocation);
                                if (Media != null)
                                {
                                    Media.Location = newLocation;
                                    Media = mediaCommands.update(Media);
                                }
                            }

                        }
                    }
                    else if (String.IsNullOrEmpty(newLocation) && !String.IsNullOrEmpty(oldLocation))
                    {
                        Factory.deleteFromDictionary(oldLocation);
                    }

                    SetProperty(ref location, newLocation);                                             
                }
                finally
                {
                    rwLock.ExitWriteLock();
                      
                }
                               
            }
        }

        bool hasGeoTag;

        public bool HasGeoTag
        {
            get { return hasGeoTag; }
            protected set { 
            SetProperty(ref hasGeoTag, value);     
            }
        }


        bool hasTags;

        public bool HasTags
        {
            get { return hasTags; }
            protected set
            {                              
                SetProperty(ref hasTags, value);              
            }
        }
          
        BaseMedia media;

        public BaseMedia Media
        {
            get { return media; }
            protected set
            {
                rwLock.EnterWriteLock();
                try
                {                  
                    SetProperty(ref media, value);
                }
                finally
                {
                    rwLock.ExitWriteLock();
                   
                }
            }
        }

        public void writeMetaData(MediaFactory.WriteOptions options, ICancellableOperationProgress progress)
        {
            // this can be a read lock since the mediafileitem instance is not modified during writing to disk.
            // but we don't want multiple writes at the same time so we use the upgradablereadlock
            rwLock.EnterUpgradeableReadLock();
            try
            {
                if (media != null)
                {               
                    MediaFactory.write(Media, options, progress);

                    checkVariables(media);
                }
            }
            finally
            {            
                rwLock.ExitUpgradeableReadLock();
                OnPropertyChanged("Media");               
            }

        }

        public void readMetaData(MediaFactory.ReadOptions options, CancellationToken token)
        {
            BaseMedia media = null;
            MediaFileItemState result = MediaFileItemState.LOADED;

            rwLock.EnterUpgradeableReadLock();           
            try
            {
                ItemState = MediaFileItemState.LOADING;

                media = MediaFactory.read(Location, options, token);

                if (media == null || media is UnknownMedia)
                {
                    result = MediaFileItemState.ERROR;
                }
                else {

                    if (media.MetadataReadError != null)
                    {
                        if (media.MetadataReadError is FileNotFoundException)
                        {
                            result = MediaFileItemState.FILE_NOT_FOUND;
                        }
                        else
                        {
                            result = MediaFileItemState.ERROR;
                        }
                    }                   
                }

            }
            catch(TimeoutException) {

                result = MediaFileItemState.TIMED_OUT;              
            }
            catch (Exception e)
            {
                result = MediaFileItemState.ERROR;
                log.Info("Error loading image grid item:" + Location, e);
            }
            finally
            {               
                Media = media;
                ItemState = result;

                checkVariables(media);

                rwLock.ExitUpgradeableReadLock();
            }
        }


        void checkVariables(BaseMedia media)
        {
            if (media == null) return;

            if (media.Tags.Count > 0)
            {
                HasTags = true;
            }
            else
            {
                HasTags = false;
            }

            if (media.Longitude != null && media.Latitude != null)
            {
                HasGeoTag = true;
            }
            else
            {
                HasGeoTag = false;
            }

        }

        public async Task readMetaDataAsync(MediaFactory.ReadOptions options, CancellationToken token)
        {

            //http://msdn.microsoft.com/en-us/magazine/jj991977.aspx
            //await mutex.WaitAsync().ConfigureAwait(false);
            await Task.Run(() =>
            {
                readMetaData(options, token);

            }).ConfigureAwait(false);

        }

        /// <summary>
        /// Returns true if deleted file was imported otherwise false
        /// </summary>
        /// <returns></returns>
        public bool delete()
        {
            rwLock.EnterWriteLock();
            try
            {
                bool isImported = false;

                if (ItemState == MediaFileItemState.DELETED)
                {
                    return (isImported);
                }

                FileUtils fileUtils = new FileUtils();

                if (ItemState != MediaFileItemState.FILE_NOT_FOUND)
                {
                    fileUtils.deleteFile(Location);
                }

                if (Media != null && Media.IsImported)
                {
                    using (MediaDbCommands mediaCommands = new MediaDbCommands())
                    {
                        mediaCommands.delete(Media);
                    }

                    Media = null;

                    isImported = true;
                }

                ItemState = MediaFileItemState.DELETED;
                //Factory.deleteFromDictionary(location);

                return (isImported);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
           
        }
      
        /// <summary>
        /// returns true if the moved item was a imported item otherwise false
        /// </summary>
        /// <param name="newLocation"></param>
        /// <param name="progress"></param>
        /// <param name="updateLocationOnly">if true, don't physically move the item</param>
        /// <returns></returns>
        public bool move(String newLocation, ICancellableOperationProgress progress)
        {
            rwLock.EnterUpgradeableReadLock();
            try
            {
                
                bool isImported = false;

                if (ItemState == MediaFileItemState.DELETED)
                {
                    return (isImported);
                }

               
                FileUtils fileUtils = new FileUtils();

                fileUtils.moveFile(Location, newLocation, progress);
                           
                // A delete event will be fired by the mediafilewatcher for the current item with it's old location.
                // If location is changed to it's new location it will not be be found in the current mediastate. 
                // So only update the location when mediafilewatcher is not active.
                Location = newLocation;
                             
                return (isImported = Media.IsImported);
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
            
        }
            

        public bool import(CancellationToken token)
        {
            rwLock.EnterUpgradeableReadLock();
            try
            {

                if (ItemState == MediaFileItemState.DELETED || media == null || media.IsImported == true)
                {
                    return (false);
                }

                using (MediaDbCommands mediaCommands = new MediaDbCommands())
                {
                    Media = mediaCommands.create(Media);
                }

                return (true);
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }

            
        }

        public bool export(CancellationToken token)
        {
            rwLock.EnterUpgradeableReadLock();
            try
            {

                if (ItemState == MediaFileItemState.DELETED || media == null || media.IsImported == false)
                {
                    return (false);
                }

                using (MediaDbCommands mediaCommands = new MediaDbCommands())
                {
                    mediaCommands.delete(Media);
                }

                return (true);
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
            
        }
       
        public bool Equals(MediaFileItem other)
        {
            if (other == null) return (false);
          
            return (Id.Equals(other.Id));
                          
        }

        public int CompareTo(MediaFileItem other)
        {
            if (other == null)
            {
                throw new ArgumentException();
            }          
            rwLock.EnterReadLock();
            try
            {
                other.rwLock.EnterReadLock();
                try
                {
                    return (Path.GetFileName(Location).CompareTo(Path.GetFileName(other.Location)));
                }
                finally
                {
                    other.rwLock.ExitReadLock();
                }
            }
            finally
            {
                rwLock.ExitReadLock();
            }

        }

        public class Factory
        {
            public static MediaFileItem EmptyItem = new MediaFileItem("");
          
            static ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
            static Dictionary<String, WeakReference<MediaFileItem>> dictionary = new Dictionary<String, WeakReference<MediaFileItem>>();

            public static MediaFileItem create(string location)
            {                               
                rwLock.EnterWriteLock();
                try
                {  
                    WeakReference<MediaFileItem> reference = null;
                    MediaFileItem item = null;

                    bool success = dictionary.TryGetValue(location, out reference);

                    if (success == true)
                    {
                        bool exists = reference.TryGetTarget(out item);

                        if (exists == false) {
                      
                            // item has been garbage collected, recreate
                            item = new MediaFileItem(location);
                            reference = new WeakReference<MediaFileItem>(item);
                            dictionary.Remove(location);
                            dictionary.Add(location, reference);
                            
                        }
                    }
                    else
                    {
                        // item did not exist yet
                        item = new MediaFileItem(location);
                        reference = new WeakReference<MediaFileItem>(item);
                        dictionary.Add(location, reference);
                  
                    }

                    return (item);
                }
                finally
                {
                    rwLock.ExitWriteLock();                   
                }
            }

            public static void renameInDictionary(String oldLocation, String newLocation)
            {
                rwLock.EnterWriteLock();
                try
                {
                    WeakReference<MediaFileItem> reference = null;
                    MediaFileItem item = null;

                    bool potentialFailure = dictionary.TryGetValue(newLocation, out reference);

                    if (potentialFailure == true)
                    {
                        if (reference.TryGetTarget(out item))
                        {
                            if (item.itemState == MediaFileItemState.DELETED)
                            {
                                // the mediafileitem in the hash has been deleted on disk
                                dictionary.Remove(newLocation);
                            }
                            else
                            {
                                // there is a live mediafileitem in the hash clashing with the newly renamed item
                                throw new InvalidOperationException("Trying to rename item to existing item in media dictionary: " + oldLocation + " to " + newLocation);
                            }
                        }
                        else
                        {
                            // the mediafileitem in the hash is already dead
                            dictionary.Remove(newLocation);
                        }
                    }

                    bool success = dictionary.TryGetValue(oldLocation, out reference);
                    
                    if (success == true)
                    {
                        bool exists = reference.TryGetTarget(out item);

                        if (exists == false)
                        {
                            // item has been garbage collected, recreate
                            dictionary.Remove(oldLocation);
                            item = new MediaFileItem(newLocation);
                            reference = new WeakReference<MediaFileItem>(item);

                            dictionary.Add(newLocation, reference);                          
                        }
                        else
                        {
                            dictionary.Remove(oldLocation);
                            dictionary.Add(newLocation, reference);
                        }
                    }
                    else
                    {
                        // item does not exist
                        throw new InvalidOperationException("Trying to rename non-existing item in media dictionary: " + oldLocation + " to " + newLocation);
                       
                    }
                 
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }
            }

            public static void deleteFromDictionary(string location)
            {
                rwLock.EnterWriteLock();
                try
                {                 
                    dictionary.Remove(location);                    
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }
            }
         
        }
              
    }
}
