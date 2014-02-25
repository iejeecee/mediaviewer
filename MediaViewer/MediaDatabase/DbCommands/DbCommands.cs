using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase.DbCommands
{
    class DbCommands<T> : IDisposable
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        const int nrRetriesOnOptimisticConcurrencyException = 3;

        bool usingExistingContext;

        protected bool UsingExistingContext
        {
            get { return usingExistingContext; }
            set { usingExistingContext = value; }
        }
        MediaDatabaseContext db;

        public MediaDatabaseContext Db
        {
            get { return db; }
            set { db = value; }
        }

        protected DbCommands(MediaDatabaseContext existingContext = null)
        {
            if (existingContext == null)
            {
                Db = new MediaDatabaseContext();
                UsingExistingContext = false;
            }
            else
            {
                Db = existingContext;
                UsingExistingContext = true;
            }
        }

        ~DbCommands()
        {
            if (!UsingExistingContext && Db != null)
            {
                Db.Dispose();
                Db = null;
            }
        }

        public virtual T update(T entity)
        {
            bool retry = true;
            T result = default(T);

            for (int i = 0; i < nrRetriesOnOptimisticConcurrencyException && retry == true; i++)
            {                
                try
                {
                    result = updateFunc(entity);

                    retry = false;
                }
                catch (OptimisticConcurrencyException e)
                {
                    log.Warn("Optimistic concurrencyexception while updating entity: " + e.Message);

                    retry = true;
                }

            }

            return (result);
        }

        protected virtual T updateFunc(T entity)
        {
            return (default(T));
        }

        public virtual T create(T entity)
        {
            bool retry = true;
            T result = default(T);

            for (int i = 0; i < nrRetriesOnOptimisticConcurrencyException && retry == true; i++)
            {
                try
                {
                    result = createFunc(entity);

                    retry = false;                    
                }
                catch (OptimisticConcurrencyException e)
                {
                    log.Warn("Optimistic concurrencyexception while creating entity: " + e.Message);

                    retry = true;
                }

            }

            return (result);
        }


        protected virtual T createFunc(T entity)
        {
            return (default(T));
        }

        public virtual void delete(T entity)
        {
            bool retry = true;

            for (int i = 0; i < nrRetriesOnOptimisticConcurrencyException && retry == true; i++)
            {
                try
                {
                    deleteFunc(entity);

                    retry = false;
                }
                catch (OptimisticConcurrencyException e)
                {
                    log.Warn("Optimistic concurrencyexception while deleting entity: " + e.Message);

                    retry = true;
                }

            }
        }
  
        protected virtual void deleteFunc(T entity)
        {

        }

        public void Dispose()
        {
            if (!UsingExistingContext)
            {
                Db.Dispose();
                Db = null;
            }
        }
    }
}
