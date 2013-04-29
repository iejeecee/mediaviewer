using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SqlTypes;
using System.Transactions;

namespace MediaDatabase
{
  
    public class Context : IDisposable
    {
        private MediaDatabaseContext ctx;

        public Context()
        {

            ctx = new MediaDatabaseContext();
            ctx.Database.Connection.Open();

        }

        public void close() {

            if (ctx != null)
            {
                ctx.Database.Connection.Close();
                ctx.Dispose();
                ctx = null;
            }
        }

        public static Media newMediaItem(FileInfo file)
        {
            Media media = new Media();
            media.Location = file.FullName;
            media.FileCreationTimeTicks = file.CreationTime.Ticks;
            media.FileLastWriteTimeTicks = file.LastWriteTime.Ticks;

            return (media);
        }

        public static bool isMediaItemOutdated(Media item, FileInfo file)
        {
            return (item.FileLastWriteTimeTicks < file.LastWriteTime.Ticks ||
                    item.FileCreationTimeTicks != file.CreationTime.Ticks);

        }

        public void needUpdate(DirectoryInfo root, List<FileInfo> files,
            out List<Media> updateMedia, out List<Media> insertMedia)
        {

            updateMedia = new List<Media>();
            insertMedia = new List<Media>();

            // to speed this up first select all media starting with root
            /*
                var relMedia = from item in ctx.Media
                               where item.Location.StartsWith(root.FullName)
                               select item;
            */

            foreach (FileInfo file in files)
            {

                Media item = ctx.Media.FirstOrDefault(media => media.Location == file.FullName);              

                if(item == null) 
                {
                    item = new Media();
                    item.Location = file.FullName;
                    item.FileLastWriteTimeTicks = file.LastWriteTime.Ticks;
                    item.FileCreationTimeTicks = file.CreationTime.Ticks;

                    insertMedia.Add(item);
                }
                else if(isMediaItemOutdated(item, file) == true)
                {                  

                    updateMedia.Add(item);
                    
                }
              
            }

   
        }

        public List<FileInfo> findMediaByTags(List<String> tags)
        {

            var locations = from item in ctx.MediaTag
                            where tags.Contains(item.Tag)
                            group item by item.Location into itemGroup
                            let count = itemGroup.Count()
                            where count == tags.Count
                            select new FileInfo(itemGroup.Key);

            return (locations.ToList());
        }

        public Media getMediaByLocation(string location)
        {
            Media item = ctx.Media.Include("MediaTag").Include("MediaThumb").Where(p => p.Location == location).FirstOrDefault();
         
            return (item);
          
        }

        public void clearAllData()
        {
            ctx.Database.ExecuteSqlCommand("DELETE FROM MediaTag");
            ctx.Database.ExecuteSqlCommand("DELETE FROM MediaThumb");
            ctx.Database.ExecuteSqlCommand("DELETE FROM Media");
        }

        public void insert(Media mediaItem)
        {
            ctx.Media.Add(mediaItem);
        }

        public void saveChanges()
        {
            ctx.SaveChanges();
        }

        public void batchInsertAndSaveChanges(List<Media> mediaItems)
        {
            int commitCount = 100;

            bool oldConfig = ctx.Configuration.AutoDetectChangesEnabled;
            //using(TransactionScope scope = new TransactionScope())
            //{

            ctx.Configuration.AutoDetectChangesEnabled = false;

            int i = 0;

            foreach (Media item in mediaItems)
            {

                ctx.Media.Add(item);

                if (++i % commitCount == 0)
                {
                    // speed up large batches by periodically saving
                    // changes and creating new contexts
                    ctx.SaveChanges();

                    ctx.Dispose();
                    ctx = new MediaDatabaseContext();
                    ctx.Configuration.AutoDetectChangesEnabled = false;

                }
            }

            ctx.SaveChanges();

            ctx.Configuration.AutoDetectChangesEnabled = oldConfig;

            // scope.Complete();
            //}
        }


        #region IDisposable Members

        public void Dispose()
        {
            close();
        }

        #endregion
    }
}
