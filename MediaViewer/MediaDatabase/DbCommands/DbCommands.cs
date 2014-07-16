// Updating a entity:
// http://stackoverflow.com/questions/15336248/entity-framework-5-updating-a-record
// Debug generated SQL, add the following line
// context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);

using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase.DbCommands
{
    class DbCommands<T> : IDisposable
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        const int nrRetriesOnOptimisticConcurrencyException = 100;

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
                catch (DbUpdateConcurrencyException e)
                {
                    if (i == nrRetriesOnOptimisticConcurrencyException - 1)
                    {
                        log.Error("Concurrencyexception while updating entity, nr retries exhausted: " + e.Message);
                        throw e;
                    }
                    else
                    {
                        foreach (DbEntityEntry conflictingEntity in e.Entries)
                        {
                            // reload the conflicting entity (database wins)
                            conflictingEntity.Reload();
                        }
                                               
                        log.Warn("Concurrencyexception while updating entity: " + e.Message);
                        retry = true;
                    }
                   
                }

            }
                

            return (result);
        }

        protected virtual T updateFunc(T entity)
        {
            throw new NotImplementedException();        
        }

        public virtual T create(T entity)
        {            
            T result = default(T);
        
            try
            {
                result = createFunc(entity);                                   
            }
            catch (DbUpdateConcurrencyException e)
            {
                log.Error("Concurrencyexception while creating entity: " + e.Message);

                throw e;
            }
                           
            return (result);
        }


        protected virtual T createFunc(T entity)
        {
            throw new NotImplementedException();     
        }

        public virtual void delete(T entity)
        {
                     
            try
            {
                deleteFunc(entity);              
            }
            catch (DbUpdateConcurrencyException e)
            {
                log.Error("Concurrencyexception while deleting entity: " + e.Message);

                throw e;
            }

            
        }
  
        protected virtual void deleteFunc(T entity)
        {
            throw new NotImplementedException();     
        }

        public void Dispose()
        {
            if (!UsingExistingContext && Db != null)
            {
                Db.Dispose();
                Db = null;
            }
        }

        public virtual void clearAll()
        {
            throw new NotImplementedException();  
        }
    }
}
