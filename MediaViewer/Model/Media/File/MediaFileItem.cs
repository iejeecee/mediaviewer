using MediaViewer.Infrastructure.Logging;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.metadata.Metadata;
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
    public class MediaFileItem : MediaItem
    {        
 

        protected MediaFileItem(String location, MediaItemState state = MediaItemState.EMPTY)
            : base(location, state)
        {
            
        }
                      
        /// <summary>
        /// Location on disk of the mediafileitem
        /// </summary>
        public override string Location
        {
            get
            {
                return (location);
            }
            set
            {                              
                RWLock.EnterWriteLock();
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
                        if (Metadata != null)
                        {
                            Metadata.Location = newLocation;

                            if (Metadata.IsImported)
                            {
                                using (MetadataDbCommands metadataCommands = new MetadataDbCommands())
                                {
                                    Metadata = metadataCommands.update(Metadata);
                                }
                            }
                        }
                        else
                        {
                            using (MetadataDbCommands metadataCommands = new MetadataDbCommands())
                            {
                                Metadata = metadataCommands.findMetadataByLocation(oldLocation);
                                if (Metadata != null)
                                {
                                    Metadata.Location = newLocation;
                                    Metadata = metadataCommands.update(Metadata);
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
                    RWLock.ExitWriteLock();
                      
                }
                               
            }
        }

                         
        public void writeMetadata(MetadataFactory.WriteOptions options, ICancellableOperationProgress progress)
        {
            // this can be a read lock since the mediafileitem instance is not modified during writing to disk.
            // but we don't want multiple writes at the same time so we use the upgradablereadlock
            RWLock.EnterUpgradeableReadLock();
            try
            {
                if (Metadata != null)
                {
                    MetadataFactory.write(Metadata, options, progress);

                    checkVariables(Metadata);
                }
            }
            finally
            {            
                RWLock.ExitUpgradeableReadLock();
                OnPropertyChanged("Metadata");               
            }

        }

        public override void readMetadata(MetadataFactory.ReadOptions options, CancellationToken token)
        {
            BaseMetadata metadata = null;
            MediaItemState result = MediaItemState.LOADED;

            RWLock.EnterUpgradeableReadLock();           
            try
            {
                ItemState = MediaItemState.LOADING;

                metadata = MetadataFactory.read(Location, options, token);

                if (metadata == null || metadata is UnknownMetadata)
                {
                    result = MediaItemState.ERROR;
                }
                else {

                    if (metadata.MetadataReadError != null)
                    {
                        if (metadata.MetadataReadError is FileNotFoundException)
                        {
                            result = MediaItemState.FILE_NOT_FOUND;
                        }
                        else
                        {
                            result = MediaItemState.ERROR;
                        }
                    }                   
                }

            }
            catch(TimeoutException) {

                result = MediaItemState.TIMED_OUT;              
            }
            catch (Exception e)
            {
                result = MediaItemState.ERROR;
                Logger.Log.Info("Error loading image grid item:" + Location, e);
            }
            finally
            {               
                Metadata = metadata;
                ItemState = result;

                checkVariables(metadata);

                RWLock.ExitUpgradeableReadLock();
            }
        }


        void checkVariables(BaseMetadata metadata)
        {
            if (metadata == null) return;

            if (metadata.Tags.Count > 0)
            {
                HasTags = true;
            }
            else
            {
                HasTags = false;
            }

            if (metadata.Longitude != null && metadata.Latitude != null)
            {
                HasGeoTag = true;
            }
            else
            {
                HasGeoTag = false;
            }

        }
        

        /// <summary>
        /// Returns true if deleted file was imported otherwise false
        /// </summary>
        /// <returns></returns>
        public bool delete()
        {
            RWLock.EnterWriteLock();
            try
            {
                bool isImported = false;

                if (ItemState == MediaItemState.DELETED)
                {
                    return (isImported);
                }

                FileUtils fileUtils = new FileUtils();

                if (ItemState != MediaItemState.FILE_NOT_FOUND)
                {
                    fileUtils.deleteFile(Location);
                }

                if (Metadata != null && Metadata.IsImported)
                {
                    using (MetadataDbCommands metadataCommands = new MetadataDbCommands())
                    {
                        metadataCommands.delete(Metadata);
                    }

                    Metadata = null;

                    isImported = true;
                }

                ItemState = MediaItemState.DELETED;
                //Factory.deleteFromDictionary(location);

                return (isImported);
            }
            finally
            {
                RWLock.ExitWriteLock();
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
            RWLock.EnterUpgradeableReadLock();
            try
            {
                
                bool isImported = false;

                if (ItemState == MediaItemState.DELETED)
                {
                    return (isImported);
                }

               
                FileUtils fileUtils = new FileUtils();

                fileUtils.moveFile(Location, newLocation, progress);
                           
                // A delete event will be fired by the mediafilewatcher for the current item with it's old location.
                // If location is changed to it's new location it will not be be found in the current mediastate. 
                // So only update the location when mediafilewatcher is not active.
                Location = newLocation;
                             
                return (isImported = Metadata.IsImported);
            }
            finally
            {
                RWLock.ExitUpgradeableReadLock();
            }
            
        }
            

        public bool import(CancellationToken token)
        {
            RWLock.EnterUpgradeableReadLock();
            try
            {

                if (ItemState == MediaItemState.DELETED || Metadata == null || Metadata.IsImported == true)
                {
                    return (false);
                }

                using (MetadataDbCommands metadataCommands = new MetadataDbCommands())
                {
                    Metadata = metadataCommands.create(Metadata);
                }

                return (true);
            }
            finally
            {
                RWLock.ExitUpgradeableReadLock();
            }

            
        }

        public bool export(CancellationToken token)
        {
            RWLock.EnterUpgradeableReadLock();
            try
            {

                if (ItemState == MediaItemState.DELETED || Metadata == null || Metadata.IsImported == false)
                {
                    return (false);
                }

                using (MetadataDbCommands metadataCommands = new MetadataDbCommands())
                {
                    metadataCommands.delete(Metadata);
                    OnPropertyChanged("Media");
                }

                return (true);
            }
            finally
            {
                RWLock.ExitUpgradeableReadLock();
            }
            
        }
       
 
        public override int CompareTo(MediaItem other)
        {
            if (other == null)
            {
                throw new ArgumentException();
            }          
            RWLock.EnterReadLock();
            try
            {
                other.RWLock.EnterReadLock();
                try
                {
                    return (Path.GetFileName(Location).CompareTo(Path.GetFileName(other.Location)));
                }
                finally
                {
                    other.RWLock.ExitReadLock();
                }
            }
            finally
            {
                RWLock.ExitReadLock();
            }

        }

        // factory to ensure mediafileitems are globally unique
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
                            if (item.ItemState == MediaItemState.DELETED)
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
