using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase
{
    class MediaDatabaseConfiguration : DbConfiguration
    {
        public MediaDatabaseConfiguration()
        {
           
            SetDefaultConnectionFactory(new LocalDbConnectionFactory("v11.0")); 
        }

    }
}
