using MediaViewer.MediaFileModel;
using MediaViewer.MediaFileModel.Watcher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase
{
    partial class Media
    {
        public Media(MediaFileItem item) {

            Location = item.Location;
            MimeType = Utils.MediaFormatConvert.fileNameToMimeType(item.Location);
            FileInfo info = new FileInfo(item.Location);
            LastModified = info.LastWriteTime;          

            Tags = new HashSet<Tag>();
            VideoProps = new VideoProps();
            ImageProps = new ImageProps();
           
            if (item.Media == null)
            {
                return;
            }

            if(item.Media.Thumbnail != null) 
            {
                Thumbnail = new Thumbnail(item.Media.Thumbnail);              
            }

            if (item.Media.MetaData == null)
            {
                return;
            }

            Description = String.IsNullOrEmpty(item.Media.MetaData.Description) ? null : item.Media.MetaData.Description;
            Author = String.IsNullOrEmpty(item.Media.MetaData.Creator) ? null : item.Media.MetaData.Creator;
            Copyright = String.IsNullOrEmpty(item.Media.MetaData.Copyright) ? null : item.Media.MetaData.Copyright;
            Title = String.IsNullOrEmpty(item.Media.MetaData.Title) ? null : item.Media.MetaData.Title;
           
            DateTime minValue = new DateTime(1753,1,1);
            DateTime maxValue = new DateTime(9999,12,31);

            if (item.Media.MetaData.CreationDate < minValue || item.Media.MetaData.CreationDate > maxValue)
            {
                CreationDate = null;
            }
            else
            {
                CreationDate = item.Media.MetaData.CreationDate;
            }           

            foreach (String tagName in item.Media.MetaData.Tags)
            {
                Tag tag = new Tag();
                tag.Name = tagName;
                Tags.Add(tag);
            }

            if (item.Media.MediaFormat == MediaFileModel.MediaFile.MediaType.VIDEO)
            {
                VideoFile video = (VideoFile)item.Media;

                VideoProps = new VideoProps(video);

            }

            if (item.Media.MediaFormat == MediaFileModel.MediaFile.MediaType.IMAGE)
            {
                ImageFile image = (ImageFile)item.Media;

                ImageProps.Width = image.Width;
                ImageProps.Height = image.Height;
            }

            
        }
    }
}
