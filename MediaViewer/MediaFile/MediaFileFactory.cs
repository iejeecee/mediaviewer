using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MediaViewer.GenericEventArgs;
using MediaViewer.Utils;

namespace MediaViewer.MediaFile
{

    class MediaFileFactory
    {

        //// 60 seconds
        const int HTTP_TIMEOUT_MS = 60 * 1000;
        const int HTTP_READ_BUFFER_SIZE_BYTES = 8096;
        //// 1 hour
        const int FILE_OPEN_ASYNC_TIMEOUT_MS = 60 * 60 * 1000;
        //// 5 seconds
        const int FILE_OPEN_SYNC_TIMEOUT_MS = 5 * 1000;

        class AsyncState
        {
            private string location;
            private Object userState;
            private ModifiableGEventArgs<bool> isCancelled;
            private MediaFileBase.MetaDataMode mode;

            public AsyncState(string location, Object userState, MediaFileBase.MetaDataMode mode)
            {
                this.mode = mode;
                this.location = location;
                this.userState = userState;
                isCancelled = new ModifiableGEventArgs<bool>(false);
            }

            public Object UserState
            {
                get
                {
                    return (userState);
                }

                set
                {
                    this.userState = value;
                }
            }

            public string Location
            {
                set
                {
                    this.location = value;
                }

                get
                {
                    return (location);
                }
            }

            public MediaFileBase.MetaDataMode MetaDataMode
            {
                get
                {
                    return (mode);
                }
            }

            public ModifiableGEventArgs<bool> IsCancelled
            {
                set
                {
                    this.isCancelled = value;
                }

                get
                {
                    return (isCancelled);
                }
            }
        };

        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        DigitallyCreated.Utilities.Concurrency.FifoSemaphore openSemaphore;
        DigitallyCreated.Utilities.Concurrency.FifoSemaphore stateSemaphore;
        List<AsyncState> activeStates;

        static MediaFileBase openWebData(AsyncState state)
        {

            HttpWebResponse response = null;
            Stream responseStream = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(state.Location);
                request.Method = "GET";
                request.Timeout = HTTP_TIMEOUT_MS;

                IAsyncResult requestResult = request.BeginGetResponse(null, null);

                while (!requestResult.IsCompleted)
                {

                    if (state.IsCancelled.Value == true)
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

                    if (state.IsCancelled.Value == true)
                    {

                        throw new MediaFileException("Aborting reading image");
                    }

                    data.Write(buffer, 0, count);
                }

                data.Seek(0, System.IO.SeekOrigin.Begin);

                MediaFileBase media = newMediaFromMimeType(state, response.ContentType, data);

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

        static MediaFileBase openFileData(AsyncState state, int timeoutMs)
        {

            Stream data = FileUtils.waitForFileAccess(state.Location, FileAccess.Read,
                timeoutMs, state.IsCancelled);

            string mimeType = MediaFormatConvert.fileNameToMimeType(state.Location);

            MediaFileBase media = newMediaFromMimeType(state, mimeType, data);

            return (media);
        }

        void asyncOpen(Object asyncState)
        {

            AsyncState state = (AsyncState)asyncState;

            //// initialize media with a dummy in case of exceptions
            MediaFileBase media = new UnknownFile(state.Location, null);

            //// only allow one thread to open files at once
            openSemaphore.Acquire();

            try
            {

                if (string.IsNullOrEmpty(state.Location) || state.IsCancelled.Value == true)
                {

                    return;

                }
                else if (FileUtils.isUrl(state.Location))
                {

                    media = openWebData(state);

                }
                else
                {

                    media = openFileData(state, FILE_OPEN_ASYNC_TIMEOUT_MS);
                }

            }
            catch (Exception e)
            {

                log.Warn("Cannot open media", e);
                media.OpenError = e;
                media.close();

            }
            finally
            {

                stateSemaphore.Acquire();
                activeStates.Remove(state);
                stateSemaphore.Release();

                OpenFinished(this, media);
            }
        }


        static MediaFileBase newMediaFromMimeType(AsyncState state, string mimeType, Stream data)
        {

            MediaFileBase media = null;

            if (mimeType.ToLower().StartsWith("image"))
            {

                media = new ImageFile(state.Location, mimeType, data,
                    state.MetaDataMode);

            }
            else if (mimeType.ToLower().StartsWith("video"))
            {

                media = new VideoFile(state.Location, mimeType, data,
                    state.MetaDataMode);

            }
            else
            {

                media = new UnknownFile(state.Location, data);
            }

            media.UserState = state.UserState;

            return (media);
        }


        public event EventHandler<MediaFileBase> OpenFinished;

        public MediaFileFactory()
        {

            //// it is important to use fifo semaphores to preserve the order in which opening
            //// files are requested
            openSemaphore = new DigitallyCreated.Utilities.Concurrency.FifoSemaphore(1);
            stateSemaphore = new DigitallyCreated.Utilities.Concurrency.FifoSemaphore(1);
            activeStates = new List<AsyncState>();
        }

        //// Open (read only) a file//http stream in a non blocking fashion
        //// When the file is successfully opened a OpenFinished event is generated
        //// The function will attempt to cancel any pending opens to speed up it's operation
        public void openNonBlockingAndCancelPending(string location, MediaFileBase.MetaDataMode mode)
        {

            openNonBlockingAndCancelPending(location, null, mode);
        }

        //// Open (read only) a file//http stream in a non blocking fashion
        //// When the file is successfully opened a OpenFinished event is generated
        //// The function will attempt to cancel any pending opens to speed up it's operation
        //// userstate is attached to the returning mediafile
        public void openNonBlockingAndCancelPending(string location, Object userState,
            MediaFileBase.MetaDataMode mode)
        {

            try
            {

                WaitCallback callback = new WaitCallback(asyncOpen);

                AsyncState state = new AsyncState(location, userState, mode);

                //// lock active states
                stateSemaphore.Acquire();

                //// cancel previously started open(s)
                for (int i = 0; i < activeStates.Count; i++)
                {

                    activeStates[i].IsCancelled.Value = true;
                }

                //// add current state to active states
                activeStates.Add(state);

                stateSemaphore.Release();

                ThreadPool.QueueUserWorkItem(callback, state);

            }
            catch (Exception e)
            {

                log.Error("Cannot open media", e);
                MessageBox.Show(e.Message);
            }

        }

        //// needs to be called after the user is done with the file
        public void releaseNonBlockingOpenLock()
        {

            openSemaphore.Release();
        }

        public static MediaFileBase openBlocking(string location, MediaFileBase.MetaDataMode mode)
        {

            AsyncState state = new AsyncState(location, null, mode);

            //// initialize media with a dummy in case of exceptions
            MediaFileBase media = new UnknownFile(state.Location, null);

            try
            {
                if (string.IsNullOrEmpty(state.Location))
                {

                    return (media);

                }
                else if (FileUtils.isUrl(state.Location))
                {

                    media = openWebData(state);

                }
                else
                {

                    media = openFileData(state, FILE_OPEN_SYNC_TIMEOUT_MS);
                }

            }
            catch (Exception e)
            {

                log.Error("cannot open media", e);
                media.OpenError = e;

                if (media.Data != null)
                {

                    media.close();
                    media.Data = null;
                }
            }

            return (media);
        }
    }
}
