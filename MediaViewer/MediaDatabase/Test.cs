using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase
{
    class Test
    {
        public static void test()
        {

            String name1 = "this";
            String name2 = "sucks";
            String name3 = "ass";
/*
            using (var db = new MediaDatabaseContext())
            {
                // Create and save a new Blog
               
                var tag1 = new Tag { Name = name1 };                
                var tag2 = new Tag { Name = name2 };
                var tag3 = new Tag { Name = name3 };

                tag2.LinkedTags = new List<Tag>();
                tag2.LinkedTags.Add(tag1);
                tag2.LinkedTags.Add(tag3);

                db.Tags.Add(tag1);
                db.Tags.Add(tag2);
                db.Tags.Add(tag3);
                db.SaveChanges();
            }
*/
            using (var db = new MediaDatabaseContext()) {

                // Display all Blogs from the database
/*
                List<Tag> result1 = (from b in db.Tags.Include("LinkedTags")
                            where b.Name =="dummy"
                            select b).ToList();

                List<Tag> result2 = (from b in db.Tags.Include("LinkedTags")
                             where b.Name == "ass"
                                     select b).ToList();

                result1[0].LinkedTags.Add(result2[0]);

                db.SaveChanges();
 */ 
            }
        }
    }
}
