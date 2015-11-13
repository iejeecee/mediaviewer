using MediaViewer.MediaDatabase;
using MediaViewer.Model.Concurrency;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.metadata.Metadata;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.Base
{
    // Class state should only be modified under write lock and read under read lock
    // Lock recursion is not supported, see this for a good explanation why 
    // http://blog.stephencleary.com/2013/04/recursive-re-entrant-locks.html
    // All propertychanged events are queued and fired after releasing the write lock
    // to make sure no events are fired while a lock is held
    public abstract class MediaItem : BindableBase, IEquatable<MediaItem>, IComparable<MediaItem>, ILockable
    {
        ConcurrentQueue<String> eventQueue;

        ReaderWriterLockSlim rwLock;

        Guid id;

        /// <summary>
        /// Unique mediafileitem identifier
        /// </summary>
        public Guid Id
        {
            get { return id; }
        }

        protected MediaItem(String location, String name = null, MediaItemState state = MediaItemState.EMPTY, bool isReadOnly = false)
        {
            rwLock = new ReaderWriterLockSlim();
            eventQueue = new ConcurrentQueue<string>();
          
            this.id = Id;
            this.location = location;           
            this.itemState = state;
            this.name = name;
            this.isReadOnly = isReadOnly;
       
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

        virtual public bool Equals(MediaItem other)
        {
            if (other == null) return (false);

            return (Id.Equals(other.Id));
        }

        public void EnterReadLock()
        {
            rwLock.EnterReadLock();
        }

        public void ExitReadLock()
        {
            rwLock.ExitReadLock();
        }

        public void EnterWriteLock()
        {
            rwLock.EnterWriteLock();
        }

        public void ExitWriteLock(bool fireQueuedEvents = true)
        {
            rwLock.ExitWriteLock();

            if (fireQueuedEvents)
            {
                FireQueuedEvents();
            }
        }

        public void EnterUpgradeableReadLock()
        {
            rwLock.EnterUpgradeableReadLock();
        }

        public void ExitUpgradeableReadLock(bool fireQueuedEvents = true)
        {
            rwLock.ExitUpgradeableReadLock();
            if (fireQueuedEvents)
            {
                FireQueuedEvents();
            }
        }
        
        string location;

        /// <summary>
        /// Location of the mediaitem (e.g. url or disk path)
        /// </summary>
        public virtual string Location
        {
            get { return location; }
            set
            {                                                
                if (object.Equals(location, value)) return; 

                location = value;                   
               
                QueueOnPropertyChangedEvent("Location");
            }
        }

        string name;
        /// <summary>
        /// Name of the media item
        /// </summary>
        public virtual string Name
        {
            get { return name; }
            set
            {              
                if (object.Equals(Name, value)) return;

                name = value;
              
                QueueOnPropertyChangedEvent("Name");
            }
        }

        public virtual int CompareTo(MediaItem other)
        {
            if (other == null)
            {
                throw new ArgumentException();
            }
            
            return (Location.CompareTo(other.Location));               
        }

        bool isReadOnly;

        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set
            {               
                if (object.Equals(isReadOnly, value)) return;

                isReadOnly = value;
               
                QueueOnPropertyChangedEvent("IsReadOnly");
            }
        }

        MediaItemState itemState;

        /// <summary>
        /// Current state of the mediafileitem
        /// </summary>
        public MediaItemState ItemState
        {
            get { return itemState; }
            set
            {               
                if (object.Equals(itemState, value)) return;

                itemState = value;     
                
                QueueOnPropertyChangedEvent("ItemState");
            }
        }

        BaseMetadata metadata;

        public BaseMetadata Metadata
        {
            get { return metadata; }
            protected set
            {               
                if (object.Equals(metadata, value)) return;

                metadata = value;
               
                QueueOnPropertyChangedEvent("Metadata");
            }
        }

        bool hasGeoTag;

        public bool HasGeoTag
        {
            get { return hasGeoTag; }
            protected set
            {               
                if (object.Equals(hasGeoTag, value)) return;

                hasGeoTag = value;
                
                QueueOnPropertyChangedEvent("HasGeoTag");
            }
        }


        bool hasTags;

        public bool HasTags
        {
            get { return hasTags; }
            protected set
            {               
                if (object.Equals(hasTags, value)) return;

                hasTags = value;
               
                QueueOnPropertyChangedEvent("HasTags");
            }
        }

        protected virtual void QueueOnPropertyChangedEvent(String propertyName)
        {
            if (!eventQueue.Contains(propertyName))
            {
                eventQueue.Enqueue(propertyName);
            }
        }

        public void FireQueuedEvents()
        {           
            String propertyName;

            while (eventQueue.TryDequeue(out propertyName))
            {
                OnPropertyChanged(propertyName);
            }       

        }

        public abstract void readMetadata_URLock(MetadataFactory.ReadOptions options, CancellationToken token);

    }
}
