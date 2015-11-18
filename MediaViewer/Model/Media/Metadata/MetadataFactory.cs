using MediaViewer.Infrastructure.Logging;
using MediaViewer.Logging;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.MetaData;
using MediaViewer.Model.Media.Metadata;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.Model.metadata.Metadata
{
    public class MetadataFactory
    {
        static int maxConcurrentReads = 10;
        static SemaphoreSlim limitConcurrentReadsSemaphore = new SemaphoreSlim(maxConcurrentReads, maxConcurrentReads);
       

        public enum ReadOptions
        {
            AUTO = 1,
            READ_FROM_DISK = 1 << 1,
            READ_FROM_DATABASE = 1 << 2,
            GENERATE_THUMBNAIL = 1 << 3,
            GENERATE_MULTIPLE_THUMBNAILS = 1 << 4,
            LEAVE_STREAM_OPENED_AFTER_READ = 1 << 5
        }

        public enum WriteOptions
        {
            AUTO = 0x1,
            WRITE_TO_DISK = 0x2,
            WRITE_TO_DATABASE = 0x4
        }

        // 60 seconds
        const int HTTP_TIMEOUT_MS = 60 * 1000;
        const int HTTP_READ_BUFFER_SIZE_BYTES = 8096;
        // 5 seconds
        const int FILE_OPEN_ASYNC_TIMEOUT_MS = 5 * 1000;
        // 5 seconds
        const int FILE_OPEN_SYNC_TIMEOUT_MS = 5 * 1000;
        
        static BaseMetadata readMetadataFromWeb(string location, ReadOptions options, CancellationToken token, int timeoutSeconds)
        {

            HttpWebResponse response = null;
            Stream responseStream = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(location);
                request.Method = "GET";
                request.Timeout = timeoutSeconds * 1000;

                IAsyncResult requestResult = request.BeginGetResponse(null, null);

                while (!requestResult.IsCompleted)
                {

                    if (token.IsCancellationRequested)
                    {

                        request.Abort();
                        throw new MediaStateException("Aborting opening image");
                    }

                    Thread.Sleep(100);
                }

                response = (HttpWebResponse)request.EndGetResponse(requestResult);

                responseStream = response.GetResponseStream();
                responseStream.ReadTimeout = HTTP_TIMEOUT_MS;

                Stream data = new MemoryStream();

                int bufSize = HTTP_READ_BUFFER_SIZE_BYTES;
                int count = 0;

                byte[] buffer = new byte[bufSize];

                while ((count = responseStream.Read(buffer, 0, bufSize)) > 0)
                {

                    if (token.IsCancellationRequested)
                    {

                        throw new MediaStateException("Aborting reading image");
                    }

                    data.Write(buffer, 0, count);
                }

                data.Seek(0, System.IO.SeekOrigin.Begin);

                BaseMetadata metadata = createMetadataFromMimeType(location, options, response.ContentType, data, token, timeoutSeconds);

                metadata.IsReadOnly = true;

                return (metadata);

            }
            finally
            {
                if (responseStream != null)
                {
                    responseStream.Close();
                }

                if (response != null)
                {
                    response.Close();
                }
            }
        }

        static BaseMetadata readMetadataFromFile(String location, ReadOptions options, CancellationToken token, int timeoutSeconds)
        {

            Stream data = FileUtils.waitForFileAccess(location, FileAccess.Read,
                FILE_OPEN_ASYNC_TIMEOUT_MS, token);

            string mimeType = MediaFormatConvert.fileNameToMimeType(location);

            BaseMetadata metadata = createMetadataFromMimeType(location, options, mimeType, data, token, timeoutSeconds);

            FileInfo info = new FileInfo(location);
            info.Refresh();

            if (info.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                metadata.IsReadOnly = true;
            }

            return (metadata);
        }

        static BaseMetadata createMetadataFromMimeType(String location, ReadOptions options,
            string mimeType, Stream data, CancellationToken token, int timeoutSeconds)
        {

            BaseMetadata metadata = null;
            MetadataReader reader = null;

            if (mimeType.ToLower().StartsWith("image"))
            {
                metadata = new ImageMetadata(location, data);
                reader = new ImageMetadataReader();
                reader.readMetadata(data, options, metadata, token, timeoutSeconds);

            }
            else if (mimeType.ToLower().StartsWith("video"))
            {
                metadata = new VideoMetadata(location, data);
                reader = new VideoMetadataReader();
                reader.readMetadata(data, options, metadata, token, timeoutSeconds);

            }
            else if (mimeType.ToLower().StartsWith("audio"))
            {
                metadata = new AudioMetadata(location, data);
                reader = new AudioMetadataReader();
                reader.readMetadata(data, options, metadata, token, timeoutSeconds);
            }
            else
            {
                metadata = new UnknownMetadata(location);
            }

            if(!options.HasFlag(ReadOptions.LEAVE_STREAM_OPENED_AFTER_READ)) {

                metadata.close();
            }

            return (metadata);
        }

        private static BaseMetadata readMetadataFromDatabase(string location, ReadOptions options, CancellationToken token, int timeoutSeconds)
        {
            BaseMetadata metadata = null;

            using (MetadataDbCommands metadataCommands = new MetadataDbCommands())
            {
                metadata = metadataCommands.findMetadataByLocation(location);

                if (metadata != null)
                {
                    metadata.IsImported = true;
                                     
                    if (options.HasFlag(ReadOptions.LEAVE_STREAM_OPENED_AFTER_READ))
                    {
                        Stream data = FileUtils.waitForFileAccess(location, FileAccess.Read,
                            FILE_OPEN_ASYNC_TIMEOUT_MS, token);

                        metadata.Data = data;
                    }

                    // check if metadata stored in the database is outdated                
                    FileInfo info = new FileInfo(metadata.Location);
                    info.Refresh();

                    if (info.Exists == false)
                    {
                        metadata.MetadataReadError = new FileNotFoundException("File not found", metadata.Location);
                        return (metadata);
                    }

                    if ((info.LastWriteTime - metadata.LastModifiedDate) > TimeSpan.FromSeconds(10))
                    {
                        // metadata is outdated so update in database
                        Logger.Log.Info("Updated: " + metadata.Location + " - Database timestamp: " + metadata.LastModifiedDate.ToString() + " Disk timestamp: " + info.LastWriteTime.ToString());
                        int id = metadata.Id;
                        metadata = readMetadataFromFile(metadata.Location, options, token, timeoutSeconds);

                        if (metadata != null)
                        {
                            metadata.IsImported = true;
                            metadata.Id = id;
                            write(metadata, WriteOptions.WRITE_TO_DATABASE, null);
                        }

                    }

                    if (info.Attributes.HasFlag(FileAttributes.ReadOnly))
                    {
                        metadata.IsReadOnly = true;
                    }

                }
            }

            return (metadata);
        }

        public static void write(BaseMetadata metadata, WriteOptions options, CancellableOperationProgressBase progress = null)
        {
           
            if (options.HasFlag(WriteOptions.AUTO) || options.HasFlag(WriteOptions.WRITE_TO_DISK))
            {

                if (metadata.MimeType.ToLower().StartsWith("image"))
                {
                    ImageMetadataWriter imageMetadataWriter = new ImageMetadataWriter();
                    imageMetadataWriter.writeMetadata(metadata, progress);

                }
                else if (metadata.MimeType.ToLower().StartsWith("video"))
                {
                    VideoMetadataWriter videoMetadataWriter = new VideoMetadataWriter();
                    videoMetadataWriter.writeMetadata(metadata, progress);

                }
                else if (metadata.MimeType.ToLower().StartsWith("audio"))
                {
                    AudioMetadataWriter audioMetadataWriter = new AudioMetadataWriter();
                    audioMetadataWriter.writeMetadata(metadata, progress);
                }
            }

            if (metadata.IsImported && (options.HasFlag(WriteOptions.AUTO) || options.HasFlag(WriteOptions.WRITE_TO_DATABASE)))
            {
                using (MetadataDbCommands metadataCommands = new MetadataDbCommands())
                {
                    metadata = metadataCommands.update(metadata);
                }
            }

            metadata.IsModified = false;
            
        }
       

        /// <summary>
        /// Returns null when metadata does not exist
        /// UnknownMetadata when metadata is not recognized
        /// </summary>
        /// <param name="location"></param>
        /// <param name="options"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static BaseMetadata read(string location, ReadOptions options, CancellationToken token, int timeoutSeconds = 60)
        {

            limitConcurrentReadsSemaphore.Wait(token);
            try
            {
                // initialize metadata with a dummy in case of exceptions
                BaseMetadata metadata = null;

                if (string.IsNullOrEmpty(location) || token.IsCancellationRequested == true)
                {
                    return (metadata);
                }
                else if (FileUtils.isUrl(location))
                {
                    metadata = readMetadataFromWeb(location, options, token, timeoutSeconds);
                }
                else
                {
                    if (options.HasFlag(ReadOptions.AUTO) || options.HasFlag(ReadOptions.READ_FROM_DATABASE))
                    {
                        metadata = readMetadataFromDatabase(location, options, token, timeoutSeconds);
                    }

                    if ((metadata == null && options.HasFlag(ReadOptions.AUTO)) ||
                        options.HasFlag(ReadOptions.READ_FROM_DISK))
                    {
                        metadata = readMetadataFromFile(location, options, token, timeoutSeconds);
                    }
                }

                metadata.IsModified = false;
                return (metadata);

            }
            catch (OperationCanceledException)
            {
                // convert operationcanceled to taskcanceled when the token aborts waiting for the semaphore
                throw new TaskCanceledException();
            }
            finally
            {
                limitConcurrentReadsSemaphore.Release();
            }
        }



    }
}
