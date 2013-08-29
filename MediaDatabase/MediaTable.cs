using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SqlTypes;

namespace MediaDatabase
{
  

    public class MediaTable : IDisposable
    {
        private const string connectStr = "Data Source=C:\\game\\imageviewer\\MediaDatabase\\MediaDatabase.sdf";
        private MediaDatabaseContext db;
        private DateTime minDateSQL;

        public MediaTable()
        {    
            db = new MediaDatabaseContext(connectStr);
            minDateSQL = new DateTime(1753, 1, 1);
        }

        public void needUpdate(DirectoryInfo root, List<FileInfo> files,
            out List<Media> updateMedia, out List<Media> insertMedia)
        {

            updateMedia = new List<Media>();
            insertMedia = new List<Media>();

            // to speed this up first select all media starting with root
            /*
                var relMedia = from item in db.Media
                               where item.Location.StartsWith(root.FullName)
                               select item;
            */

            foreach (FileInfo file in files)
            {

                Media item = db.Media.FirstOrDefault(media => media.Location == file.FullName);              

                if(item == null) 
                {
                    item = new Media();
                    item.Location = file.FullName;
                    item.LastWriteTime = file.LastWriteTime.Ticks;

                    insertMedia.Add(item);
                }
                else if(item.LastWriteTime < file.LastWriteTime.Ticks)
                {                  

                    updateMedia.Add(item);
                    
                }
              
            }

        }

        public Media getMediaByLocation(string location)
        {

            Media result = db.Media.FirstOrDefault(media => media.Location == location);

            return (result);
    
        }

        public void delete()
        {

            db.ExecuteCommand("DELETE FROM Media");
        }

        public void submitChanges()
        {
            db.SubmitChanges();
        }

        public void insertOnSubmit(Media mediaItem)
        {

            db.Media.InsertOnSubmit(mediaItem);

        }

        public void insertOnSubmit(List<Media> mediaItems)
        {
          
            db.Media.InsertAllOnSubmit(mediaItems);
            
        }

        public void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
            }
        }

       
    }
}
