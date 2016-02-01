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

            VideoPreview mediaPreview = new VideoPreview();

            try
            {
                mediaPreview.open(location, token);

                if (mediaPreview.IsAudio)
                {
                    metadata = new AudioMetadata(location, data);
                    AudioFileMetadataReader reader = new AudioFileMetadataReader();
                    reader.readMetadata(mediaPreview, data, options, metadata, token, timeoutSeconds);
                }
                else if (mediaPreview.IsVideo)
                {
                    metadata = new VideoMetadata(location, data);
                    VideoFileMetadataReader reader = new VideoFileMetadataReader();
                    reader.readMetadata(mediaPreview, data, options, metadata, token, timeoutSeconds);

                }
                else if (mediaPreview.IsImage)
                {
                    metadata = new ImageMetadata(location, data);
                    ImageFileMetadataReader reader = new ImageFileMetadataReader();
                    reader.readMetadata(mediaPreview, data, options, metadata, token, timeoutSeconds);
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
                mediaPreview.close();
                mediaPreview.Dispose();                
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
                throw new Exception("cannot write metadata");
            }

        }
    }
}
