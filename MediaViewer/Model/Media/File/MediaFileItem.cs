using MediaViewer.Infrastructure.Logging;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Media.Base.Item;
using MediaViewer.Model.metadata.Metadata;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Concurrent;
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
            : base(location, Path.GetFileNameWithoutExtension(location), state)
        {
           
        }
                      
        /// <summary>
        /// Location on disk of the mediafileitem
        /// </summary>
        public override string Location
        {
            get
            {
                return (base.Location);
            }
            set
            {
                bool isMetadataChanged = false;

                String newLocation = value;
                String oldLocation = Location;               

                if (!String.IsNullOrEmpty(oldLocation) && !String.IsNullOrEmpty(newLocation))
                {
                    // check if newLocation has changed
                    if (oldLocation.Equals(newLocation)) return;
                  
                    // update newLocation in dictionary
                    Factory.renameInDictionary(oldLocation, newLocation);

                    // update newLocation in the database
                    // Note: don't use the base class getters/setters for metadata to
                    // prevent triggering a onpropertychanged event while holding a write lock
                    if (Metadata != null)
                    {
                        Metadata.Location = newLocation;

                        if (Metadata.IsImported)
                        {
                            using (MetadataDbCommands metadataCommands = new MetadataDbCommands())
                            {
                                Metadata = metadataCommands.update(Metadata);
                                isMetadataChanged = true;
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

                            isMetadataChanged = true;
                        }

                    }
                }
                else if (String.IsNullOrEmpty(newLocation) && !String.IsNullOrEmpty(oldLocation))
                {
                    Factory.deleteFromDictionary(oldLocation);
                }

                base.Location = newLocation;
                Name = Path.GetFileNameWithoutExtension(newLocation);

                if(isMetadataChanged) {

                    QueueOnPropertyChangedEvent("Metadata");
                }
           
            }
        }
                                
        public void writeMetadata_URLock(MetadataFactory.WriteOptions options, CancellableOperationProgressBase progress = null)
        {                  
            if (Metadata != null)
            {
                MetadataFactory.write(Metadata, options, progress);
                QueueOnPropertyChangedEvent("Metadata");

                EnterWriteLock();
                try
                {
                    checkVariables(Metadata);
                }
                finally
                {
                    ExitWriteLock(false);
                }
            }           
        }

        public override void readMetadata_URLock(MetadataFactory.ReadOptions options, CancellationToken token)
        {
            if (ItemState == MediaItemState.RELOAD)
            {
                reloadFromDisk_URLock(token);
                return;
            }

            EnterWriteLock();
            ItemState = MediaItemState.LOADING;
            ExitWriteLock(false);

            MediaItemState itemState = MediaItemState.ERROR;
            BaseMetadata metadata = null;

            try
            {
                metadata = MetadataFactory.read(Location, options, token);

                if (metadata == null || metadata is UnknownMetadata)
                {
                    itemState = MediaItemState.ERROR;
                }
                else
                {

                    if (metadata.MetadataReadError != null)
                    {
                        if (metadata.MetadataReadError is FileNotFoundException)
                        {
                            itemState = MediaItemState.FILE_NOT_FOUND;
                        }
                        else
                        {
                            itemState = MediaItemState.ERROR;
                        }
                    }
                    else
                    {
                        itemState = MediaItemState.LOADED;

                    }
                }


            }
            catch (TimeoutException)
            {
                itemState = MediaItemState.TIMED_OUT;
            }
            catch (Exception e)
            {
                itemState = MediaItemState.ERROR;
                Logger.Log.Info("Error loading Metadata:" + Location, e);
            }
            finally
            {
                EnterWriteLock();
                Metadata = metadata;
                ItemState = itemState;
                if (Metadata != null)
                {
                    IsReadOnly = Metadata.IsReadOnly;
                    checkVariables(Metadata);
                }
                ExitWriteLock(false);

            }
            
        }

        // update metadata from disk
        void reloadFromDisk_URLock(CancellationToken token)
        {
            EnterWriteLock();
            ItemState = MediaItemState.LOADING;
            ExitWriteLock(false);

            MetadataFactory.ReadOptions readOptions = MetadataFactory.ReadOptions.READ_FROM_DISK;

            ICollection<Thumbnail> thumbs = null;      
            int id = 0;

            if (Metadata != null)
            {                
                id = Metadata.Id;

                if (Metadata.Thumbnails.Count == 0)
                {
                    // generate thumbnail
                    readOptions |= MetadataFactory.ReadOptions.GENERATE_THUMBNAIL;

                } else {

                    // save current thumbs
                    thumbs = Metadata.Thumbnails;
                }
            }

            readMetadata_URLock(readOptions, token);

            if (Metadata != null && thumbs != null)
            {
                // restore thumbnails
                Metadata.Thumbnails = thumbs;
            }

            if (Metadata != null && id != 0)
            {
                // update changes to database
                using (MetadataDbCommands metadataCommands = new MetadataDbCommands())
                {
                    Metadata.Id = id;                    
                    Metadata = metadataCommands.update(Metadata);
                                     
                }
               
            }

        }

        // function should only be called inside a write lock
        bool checkVariables(BaseMetadata metadata)
        {
            if (metadata == null) return(false);

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

            return (true);

        }
        

        /// <summary>
        /// Returns true if deleted file was imported otherwise false
        /// </summary>
        /// <returns></returns>
        public bool delete_WLock()
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
      
        /// <summary>
        /// returns true if the moved item was a imported item otherwise false
        /// </summary>
        /// <param name="newLocation"></param>
        /// <param name="progress"></param>        
        /// <returns></returns>
        public bool move_URLock(String newLocation, CancellableOperationProgressBase progress)
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
            EnterWriteLock();
            Location = newLocation;
            ExitWriteLock(false);
                                             
            return (isImported = Metadata.IsImported);
                       
        }
            

        public bool import_WLock(CancellationToken token)
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

        public bool export_WLock(CancellationToken token)
        {           
            if (ItemState == MediaItemState.DELETED || Metadata == null || Metadata.IsImported == false)
            {
                return (false);
            }

            using (MetadataDbCommands metadataCommands = new MetadataDbCommands())
            {
                metadataCommands.delete(Metadata);
                   
            }               
           
            QueueOnPropertyChangedEvent("Metadata");
            return (true);
        }
       
 
        public override int CompareTo(MediaItem other)
        {
            if (other == null)
            {
                throw new ArgumentException();
            }          
           
            return (Path.GetFileName(Location).CompareTo(Path.GetFileName(other.Location)));               
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

            public static MediaFileItem findInDictionary(string location)
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

                        if (exists == false)
                        {
                            return (null);
                        }
                    }
                   
                    return (item);
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }
            }
        }
              
    }
}
