using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaDatabaseTest
{
    class Program
    {

        class Dummy
        {
            [Key]
            public String Name {get; set;}
        }

        class DummyContext : DbContext
        {
            public DbSet<Dummy> Dummies { get; set; }
        }

        static void Main(string[] args)
        {
            using (var db = new DummyContext())
            {
                // Create and save a new Blog

                string name = "test";

                var tag = new Dummy { Name = name };
        
                db.Dummies.Add(tag);
                db.SaveChanges();
            }
        }
    }
}
