using MediaViewer.Infrastructure.Utils;
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

namespace ImageSearchPlugin
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

            foreach (ImageResultItem item in items)
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(CancellationToken);
                }

                String fullpath = null;
                String ext = "." + MediaFormatConvert.mimeTypeToExtension(item.ImageInfo.ContentType);

                try
                {
                    String filename = Path.GetFileName(item.ImageInfo.MediaUrl);
                                        
                    if (!filename.EndsWith(ext))
                    {
                        filename = filename.Substring(0, filename.LastIndexOf('.'));
                        filename += ext;
                    }

                    fullpath = FileUtils.getUniqueFileName(outputPath + "\\" + filename);
                   
                }
                catch (Exception)
                {
                    fullpath = FileUtils.getUniqueFileName(outputPath + "\\" + "image" + ext);
                }

                FileStream outFile = null;

                try
                {
                    outFile = new FileStream(fullpath, FileMode.Create);
                    string mimeType;

                    ItemProgressMax = 1;
                    ItemProgress = 0;

                    ItemInfo = "Downloading: " + fullpath;
                    StreamUtils.readHttpRequest(new Uri(item.ImageInfo.MediaUrl), outFile, out mimeType, CancellationToken, progressCallback);
                    TotalProgress++;
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

        }

        private void progressCallback(long bytesDownloaded, long totalBytes)
        {
            ItemProgressMax = (int)totalBytes;
            ItemProgress = (int)bytesDownloaded;
        }
        
    }
}
