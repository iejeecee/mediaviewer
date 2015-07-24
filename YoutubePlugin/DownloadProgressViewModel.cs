using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base;
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
using YoutubePlugin.Item;

namespace YoutubePlugin
{
    class DownloadProgressViewModel : CancellableOperationProgressBase
    {
        public DownloadProgressViewModel()
        {
            WindowTitle = "Image Search Download";
            WindowIcon = "pack://application:,,,/ImageSearchPlugin;component/Resources/Icons/Search.ico";
        }

        public void startDownload(String outputPath, List<MediaItem> items)
        {
            TotalProgress = 0;
            TotalProgressMax = items.Count;
            
            foreach (YoutubeVideoItem item in items)
            {
                YoutubeVideoStreamedItem[] avStream = new YoutubeVideoStreamedItem[2];
                item.getBestQualityStreams(out avStream[0], out avStream[1]);

                for (int i = 0; i < 2; i++)
                {
                    if (avStream[i] == null) continue;

                    VideoMetadata metadata = avStream[i].Metadata as VideoMetadata;

                    String fullpath;
                    String ext = "." + MediaFormatConvert.mimeTypeToExtension(metadata.MimeType);
                    String filename = FileUtils.removeIllegalCharsFromFileName(avStream[i].Name, " ") + ext;
                   
                    try
                    {
                        fullpath = FileUtils.getUniqueFileName(outputPath + "\\" + filename);                        
                    }
                    catch (Exception)
                    {
                        fullpath = FileUtils.getUniqueFileName(outputPath + "\\" + "stream" + ext);                        
                    }

                    FileStream outFile = null;                  
                    
                    try
                    {
                        outFile = new FileStream(fullpath, FileMode.Create);
                        string mimeType;

                        ItemProgressMax = 1;
                        ItemProgress = 0;

                        ItemInfo = "Downloading: " + fullpath;
                        StreamUtils.readHttpRequest(new Uri(avStream[i].Location), outFile, out mimeType, CancellationToken, progressCallback);
                        
                        ItemProgressMax = 1;
                        ItemProgress = 1;
                        InfoMessages.Add("Downloaded: " + fullpath);

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

                TotalProgress++;
            }

        }

        private void progressCallback(long bytesDownloaded, long totalBytes)
        {
            ItemProgressMax = (int)totalBytes;
            ItemProgress = (int)bytesDownloaded;
        }

    }
}
