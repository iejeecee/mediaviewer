using MediaViewer.MetaData;
using MediaViewer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.MediaFileModel
{
    class MediaFileFactory
    {
        // 60 seconds
        const int HTTP_TIMEOUT_MS = 60 * 1000;
        const int HTTP_READ_BUFFER_SIZE_BYTES = 8096;
        // 1 hour
        const int FILE_OPEN_ASYNC_TIMEOUT_MS = 60 * 60 * 1000;
        // 5 seconds
        const int FILE_OPEN_SYNC_TIMEOUT_MS = 5 * 1000;

        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static MediaFile openWebData(string location, MediaFile.MetaDataMode mode, CancellationToken token, Object userState)
        {

            HttpWebResponse response = null;
            Stream responseStream = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(location);
                request.Method = "GET";
                request.Timeout = HTTP_TIMEOUT_MS;

                IAsyncResult requestResult = request.BeginGetResponse(null, null);

                while (!requestResult.IsCompleted)
                {

                    if (token.IsCancellationRequested)
                    {

                        request.Abort();
                        throw new MediaFileException("Aborting opening image");
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

                        throw new MediaFileException("Aborting reading image");
                    }

                    data.Write(buffer, 0, count);
                }

                data.Seek(0, System.IO.SeekOrigin.Begin);

                MediaFile media = newMediaFromMimeType(location, mode, userState, response.ContentType, data);

                return (media);

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

        static MediaFile openFileData(String location, MediaFile.MetaDataMode mode,
            Object userState, CancellationToken token, int timeoutMs)
        {

            Stream data = FileUtils.waitForFileAccess(location, FileAccess.Read,
                timeoutMs, token);

            string mimeType = MediaFormatConvert.fileNameToMimeType(location);

            MediaFile media = newMediaFromMimeType(location, mode, userState, mimeType, data);

            return (media);
        }

        static MediaFile newMediaFromMimeType(String location, MediaFile.MetaDataMode mode,
            Object userState, string mimeType, Stream data)
        {

            MediaFile media = null;

            if (mimeType.ToLower().StartsWith("image"))
            {
                media = new ImageFile(location, mimeType, data, mode);
            }
            else if (mimeType.ToLower().StartsWith("video"))
            {
                media = new VideoFile(location, mimeType, data, mode);
            }
            else
            {
                media = new UnknownFile(location, data);
            }

            media.UserState = userState;

            return (media);
        }


        public static MediaFile open(string location, MediaFile.MetaDataMode mode, CancellationToken token, Object userState = null)
        {
            // initialize media with a dummy in case of exceptions
            MediaFile media = new UnknownFile(location, null);
           
            try
            {

                if (string.IsNullOrEmpty(location) || token.IsCancellationRequested == true)
                {
                    return(media);
                }
                else if (FileUtils.isUrl(location))
                {
                    media = openWebData(location, mode, token, userState);
                }
                else
                {
                    media = openFileData(location, mode, userState, 
                        token, FILE_OPEN_ASYNC_TIMEOUT_MS);

                    media.readMetaData();

                    if (media.Thumbnail == null)
                    {
                        media.generateThumbnails();
                    }
                                        
                }

            }
            catch (Exception e)
            {
                log.Warn("Error loading media: " + location, e);

                media.OpenError = e;
                media.close();
              
            }
            
            return (media);
        }
       

        public static async Task<MediaFile> openAsync(string location, MediaFile.MetaDataMode mode, CancellationToken token, Object userState = null)
        {

            return await Task<MediaFile>.Run(() => open(location, mode, token, userState), token).ConfigureAwait(false);
           

        }

    }
}
