using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase
{
    class DbCommands : IDisposable
    {
        bool usingExistingContext;

        protected bool UsingExistingContext
        {
            get { return usingExistingContext; }
            set { usingExistingContext = value; }
        }
        MediaDatabaseContext db;

        protected MediaDatabaseContext Db
        {
            get { return db; }
            set { db = value; }
        }

        protected DbCommands(MediaDatabaseContext existingContext)
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
