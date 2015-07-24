using MediaViewer.MediaDatabase;
using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubePlugin.Item
{
    class YoutubeItemMetadata : BaseMetadata
    {       
        public int Url { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public long? DurationSeconds { get; set; }
        public long? ViewCount { get; set; }
        public double? FramesPerSecond { get; set; }
      
        public YoutubeItemMetadata()
        {            
        }

        public override string DefaultFormatCaption
        {
            get
            {
                if (MetadataReadError != null)
                {
                    return MetadataReadError.Message;
                }

                StringBuilder sb = new StringBuilder();

                String description = Description;

                if (Description.Length > 170)
                {
                    description = Description.Substring(0, 170) + "...";
                }

                sb.AppendLine(description);

                if (CreationDate != null)
                {
                    sb.AppendLine();
                    sb.AppendLine("Published: " + CreationDate.Value.ToString("MMM d, yyyy"));
                }

                if (ViewCount != null)
                {                    
                    sb.AppendLine("ViewCount: " + ViewCount.Value.ToString("#,##0", new CultureInfo("en-US")));                    
                }

                if (Rating != null)
                {
                    sb.AppendLine("Rating: " + Rating.Value.ToString("#.##"));                   
                }

                if (Author != null)
                {
                    sb.AppendLine("Author: " + Author);                   
                }
                
                if (MimeType != null)
                {
                    sb.AppendLine("Mime type: " + MimeType);                                                        
                }

                if (Width != null && Height != null)
                {
                    String fps = "";

                    if (FramesPerSecond != null)
                    {
                        fps = ", " + FramesPerSecond + "fps";
                    }

                    String sizeBytes = "";

                    if (SizeBytes != 0)
                    {
                        sizeBytes = ", " + MiscUtils.formatSizeBytes(SizeBytes);
                    }

                    sb.AppendLine("Video: " + Width + " x " + Height + fps + sizeBytes);                                      
                }
            
                if (DurationSeconds != null)
                {
                    sb.Append("Duration: " + MiscUtils.formatTimeSeconds(DurationSeconds.Value));                 
                }
                             
                return (sb.ToString());
            }
        }
    }
}
