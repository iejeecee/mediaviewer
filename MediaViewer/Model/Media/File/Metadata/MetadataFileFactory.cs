using MediaViewer.MediaDatabase;
using MediaViewer.Progress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.Model.Media.Base.Metadata;
using System.Threading;
using MediaViewer.Infrastructure.Logging;
using MediaViewer.Model.Utils;
using System.IO;
using VideoLib;

namespace MediaViewer.Model.Media.File.Metadata
{
    class MetadataFileFactory
    {

        public static BaseMetadata read(String location, MetadataFactory.ReadOptions options, CancellationToken token, int timeoutSeconds)
        {
            BaseMetadata metadata = new UnknownMetadata(location);

            Logger.Log.Info("Reading metadata for: " + location);

            int timeoutMs = timeoutSeconds * 1000;

            Stream data = FileUtils.waitForFileAccess(location, FileAccess.Read, timeoutMs, token);

            MediaProbe mediaProbe = new MediaProbe();

            try
            {
                mediaProbe.open(location, token);

                switch (mediaProbe.MediaType)
                {
                    case MediaType.AUDIO_MEDIA:
                        {
                            metadata = new AudioMetadata(location, data);
                            AudioFileMetadataReader reader = new AudioFileMetadataReader();
                            reader.readMetadata(mediaProbe, data, options, metadata, token, timeoutSeconds);
                            break;
                        }
                    case MediaType.IMAGE_MEDIA:
                        {
                            metadata = new ImageMetadata(location, data);
                            ImageFileMetadataReader reader = new ImageFileMetadataReader();
                            reader.readMetadata(mediaProbe, data, options, metadata, token, timeoutSeconds);
                            break;
                        }                
                    case MediaType.VIDEO_MEDIA:
                        {
                            metadata = new VideoMetadata(location, data);
                            VideoFileMetadataReader reader = new VideoFileMetadataReader();
                            reader.readMetadata(mediaProbe, data, options, metadata, token, timeoutSeconds);
                            break;
                        }
                    default:
                        break;
                }
                
                FileInfo info = new FileInfo(location);
                info.Refresh();

                if (info.Attributes.HasFlag(FileAttributes.ReadOnly))
                {
                    metadata.IsReadOnly = true;
                }

                if (!options.HasFlag(MetadataFactory.ReadOptions.LEAVE_STREAM_OPENED_AFTER_READ))
                {
                    metadata.close();
                }

            }
            catch (Exception e)
            {
                metadata.MetadataReadError = e;                
            }
            finally
            {                
                mediaProbe.close();
                mediaProbe.Dispose();                
            }
                        
            return metadata;
        }

        public static void write(BaseMetadata metadata, CancellableOperationProgressBase progress = null)
        {
            if (metadata is ImageMetadata)
            {
                ImageFileMetadataWriter imageMetadataWriter = new ImageFileMetadataWriter();
                imageMetadataWriter.writeMetadata(metadata, progress);

            }
            else if (metadata is VideoMetadata)
            {
                VideoFileMetadataWriter videoMetadataWriter = new VideoFileMetadataWriter();
                videoMetadataWriter.writeMetadata(metadata, progress);

            }
            else if (metadata is AudioMetadata)
            {
                AudioFileMetadataWriter audioMetadataWriter = new AudioFileMetadataWriter();
                audioMetadataWriter.writeMetadata(metadata, progress);
            }
            else 
            {
                MetadataFileWriter metadataFileWriter = new MetadataFileWriter();
                metadataFileWriter.writeMetadata(metadata, progress);
            } 

        }
    }
}
