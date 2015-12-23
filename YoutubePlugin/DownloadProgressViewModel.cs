using MediaViewer.Infrastructure.Utils;
using MediaViewer.Infrastructure.Video.TranscodeOptions;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.Base.Item;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.metadata.Metadata;
using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VideoLib;
using YoutubePlugin.Item;

namespace YoutubePlugin
{
    class DownloadProgressViewModel : CancellableOperationProgressBase
    {
        VideoLib.VideoTranscoder videoTranscoder;

        public DownloadProgressViewModel()
        {
            WindowTitle = "Youtube Download";
            WindowIcon = "pack://application:,,,/YoutubePlugin;component/Resources/Icons/youtube.ico";

            videoTranscoder = new VideoLib.VideoTranscoder();
            videoTranscoder.setLogCallback(muxingInfoCallback, true, VideoLib.VideoTranscoder.LogLevel.LOG_LEVEL_INFO);
        }

        public void startDownload(String outputPath, List<MediaItem> items)
        {
            TotalProgress = 0;
            TotalProgressMax = items.Count;

            try
            {
                foreach (YoutubeVideoItem item in items)
                {
                    CancellationToken.ThrowIfCancellationRequested();

                    YoutubeVideoStreamedItem videoStream, audioStream;
                    item.getBestQualityStreams(out videoStream, out audioStream);

                    if (videoStream == null)
                    {
                        InfoMessages.Add("Skipping: " + item.Name + " no streams found");
                        continue;
                    }

                    YoutubeItemMetadata metadata = item.Metadata as YoutubeItemMetadata;

                    String fullpath;
                    String ext = "." + MediaFormatConvert.mimeTypeToExtension(metadata.MimeType);
                    String filename = FileUtils.removeIllegalCharsFromFileName(item.Name, " ") + ext;

                    try
                    {
                        fullpath = FileUtils.getUniqueFileName(outputPath + "\\" + filename);
                    }
                    catch (Exception)
                    {
                        fullpath = FileUtils.getUniqueFileName(outputPath + "\\" + "stream" + ext);
                    }

                    if (audioStream == null)
                    {
                        singleStreamDownload(fullpath, videoStream);
                    }
                    else
                    {
                        downloadAndMuxStreams(fullpath, videoStream, audioStream);
                    }

                    saveMetadata(fullpath, item);

                    InfoMessages.Add("Finished: " + videoStream.Name + " -> " + fullpath);

                    TotalProgress++;
                }
            }
            catch (Exception e)
            {
                InfoMessages.Add("Error: " + e.Message);
            }

        }

        private void saveMetadata(string fullpath, YoutubeVideoItem item)
        {
            YoutubeItemMetadata metadata = item.Metadata as YoutubeItemMetadata;

            MediaFileItem fileItem = MediaFileItem.Factory.create(fullpath);

            fileItem.EnterWriteLock();

            YoutubeItemMetadata saveMetadata = metadata.Clone() as YoutubeItemMetadata;

            saveMetadata.Location = fullpath;
            ItemProgress = 0;
            ItemInfo = "Saving metadata: " + item.Name;
            MetadataFactory.write(saveMetadata, MetadataFactory.WriteOptions.WRITE_TO_DISK, this);
            ItemProgress = 100;
            InfoMessages.Add("Finished saving metadata: " + fullpath);

            fileItem.ExitWriteLock();

            /*fileItem.EnterUpgradeableReadLock();
            fileItem.readMetadata_URLock(MetadataFactory.ReadOptions.READ_FROM_DISK, CancellationToken);
            fileItem.ExitUpgradeableReadLock();*/
        }

        private void downloadAndMuxStreams(string fullpath, YoutubeVideoStreamedItem videoStream, 
            YoutubeVideoStreamedItem audioStream)
        {                    
            TotalProgress = 0;
            ItemProgressMax = 100;

            Dictionary<String, Object> options = new Dictionary<string, object>();
            options.Add("videoStreamMode", StreamOptions.Copy);
            options.Add("audioStreamMode", StreamOptions.Copy);
                           
            ItemProgress = 0;
            ItemInfo = "Downloading and muxing: " + videoStream.Name;

            try
            {
                OpenVideoArgs openArgs = new OpenVideoArgs(videoStream.Location, null, audioStream.Location, null);

                videoTranscoder.transcode(openArgs, fullpath, CancellationToken, options,muxingProgressCallback);
            }
            catch (Exception e)
            {
                InfoMessages.Add("Error muxing: " + e.Message);

                try
                {
                    File.Delete(fullpath);
                }
                catch (Exception ex)
                {
                    InfoMessages.Add("Error deleting: " + fullpath + " " + ex.Message);
                }
                    
                return;
            }
                    
            ItemProgress = 100;  
        }

        void singleStreamDownload(String fullpath, YoutubeVideoStreamedItem item)
        {
            
            FileStream outFile = null;

            try
            {
                outFile = new FileStream(fullpath, FileMode.Create);
                string mimeType;

                ItemProgressMax = 1;
                ItemProgress = 0;

                ItemInfo = "Downloading: " + fullpath;
                StreamUtils.readHttpRequest(new Uri(item.Location), outFile, out mimeType, CancellationToken, downloadProgressCallback);

                ItemProgressMax = 1;
                ItemProgress = 1;
             
                outFile.Close();
            }
            catch (Exception e)
            {
                InfoMessages.Add("Error downloading: " + fullpath + " " + e.Message);

                if (outFile != null)
                {
                    outFile.Close();
                    File.Delete(fullpath);
                }
                return;
            }

        }


        void downloadProgressCallback(long bytesDownloaded, long totalBytes)
        {
            ItemProgressMax = (int)totalBytes;
            ItemProgress = (int)bytesDownloaded;
        }

        void muxingProgressCallback(double progress)
        {
            ItemProgress = (int)(progress * 100);
        }

        void muxingInfoCallback(int logLevel, String message)
        {
            InfoMessages.Add(message);
        }

    }
}
