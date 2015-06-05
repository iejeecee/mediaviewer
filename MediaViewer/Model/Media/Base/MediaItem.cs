using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.metadata.Metadata;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.Base
{
    public abstract class MediaItem : BindableBase, IEquatable<MediaItem>, IComparable<MediaItem>
    {
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

        protected MediaItem(String location, MediaItemState state = MediaItemState.EMPTY)
        {
            rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

            this.id = Id;
            Location = location;           
            ItemState = state;
       
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

        public bool Equals(MediaItem other)
        {
            if (other == null) return (false);

            return (Id.Equals(other.Id));
        }

        protected string location;

        /// <summary>
        /// Location on disk of the mediafileitem
        /// </summary>
        public virtual string Location
        {
            get { return location; }
            set
            {
                rwLock.EnterWriteLock();
                try
                {                                   
                    if (object.Equals(location, value)) return; 

                    location = value;                   
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }

                OnPropertyChanged("Location");
            }
        }

        public virtual int CompareTo(MediaItem other)
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
                    return (Location.CompareTo(other.Location));
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

        protected MediaItemState itemState;

        /// <summary>
        /// Current state of the mediafileitem
        /// </summary>
        public MediaItemState ItemState
        {
            get { return itemState; }
            set
            {
                rwLock.EnterWriteLock();
                try
                {
                    if (object.Equals(itemState, value)) return;

                    itemState = value;     
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }

                OnPropertyChanged("ItemState");
            }
        }

        protected BaseMetadata metadata;

        public BaseMetadata Metadata
        {
            get { return metadata; }
            protected set
            {
                RWLock.EnterWriteLock();
                try
                {
                    if (object.Equals(metadata, value)) return;

                    metadata = value;
                }
                finally
                {
                    RWLock.ExitWriteLock();
                }

                OnPropertyChanged("Metadata");
            }
        }

        protected bool hasGeoTag;

        public bool HasGeoTag
        {
            get { return hasGeoTag; }
            protected set
            {

                RWLock.EnterWriteLock();
                try
                {
                    if (object.Equals(hasGeoTag, value)) return;

                    hasGeoTag = value;
                }
                finally
                {
                    RWLock.ExitWriteLock();
                }

                OnPropertyChanged("HasGeoTag");
            }
        }


        protected bool hasTags;

        public bool HasTags
        {
            get { return hasTags; }
            protected set
            {
                RWLock.EnterWriteLock();
                try
                {
                    if (object.Equals(hasTags, value)) return;

                    hasTags = value;
                }
                finally
                {
                    RWLock.ExitWriteLock();
                }

                OnPropertyChanged("HasTags");
            }
        }

        public abstract void readMetadata(MetadataFactory.ReadOptions options, CancellationToken token);

    }
}
