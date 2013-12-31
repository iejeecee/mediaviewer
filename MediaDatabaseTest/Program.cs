using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaDatabaseTest
{
    class Program
    {

        static void Main(string[] args)
        {
            using (var db = new MediaDatabaseTestContainer())
            {
                // Create and save a new Blog

                string name = "test";

                var tag = new Tag { Name = name };
        
                db.TagSet.Add(tag);
                db.SaveChanges();
            }
        }
    }
}
