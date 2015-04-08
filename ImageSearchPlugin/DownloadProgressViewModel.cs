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
    class DownloadProgressViewModel : CloseableBindableBase, ICancellableOperationProgress
    {
        public DownloadProgressViewModel()
        {
            WindowTitle = "Image Search Download";
            WindowIcon = "pack://application:,,,/ImageSearchPlugin;component/Resources/Icons/Search.ico";

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken = tokenSource.Token;

            OkCommand = new Command(() =>
            {
                OnClosingRequest();
            });

            CancelCommand = new Command(() =>
                {
                    tokenSource.Cancel();
                });

            InfoMessages = new ObservableCollection<string>();
        }

        public void startDownload(String outputPath, List<MediaItem> items)
        {
            TotalProgress = 0;
            TotalProgressMax = items.Count;

            foreach (ImageResultItem item in items)
            {
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
          
                FileStream outFile = new FileStream(fullpath, FileMode.Create);
                string mimeType;

                try
                {
                    ItemProgressMax = 1;
                    ItemProgress = 0;

                    ItemInfo = "Downloading: " + fullpath;
                    StreamUtils.download(new Uri(item.ImageInfo.MediaUrl), outFile, out mimeType, CancellationToken, progressCallback);
                    TotalProgress++;
                    ItemProgressMax = 1;
                    ItemProgress = 1;
                    InfoMessages.Add("Downloaded: " + fullpath);

                    outFile.Close();
                }
                catch (Exception e)
                {
                    InfoMessages.Add("Error downloading: " + fullpath + " " + e.Message);

                    outFile.Close();
                    File.Delete(fullpath);
                    return;
                }
                
            }

        }

        private void progressCallback(long bytesDownloaded, long totalBytes)
        {
            ItemProgressMax = (int)totalBytes;
            ItemProgress = (int)bytesDownloaded;
        }


        public Command OkCommand { get; set; }
        public Command CancelCommand { get; set; }

        String itemInfo;

        public String ItemInfo
        {
            get { return itemInfo; }
            set
            {
                SetProperty(ref itemInfo, value);
            }
        }

        ObservableCollection<String> infoMessages;

        public ObservableCollection<String> InfoMessages
        {
            get { return infoMessages; }
            set
            {
                SetProperty(ref infoMessages, value);
            }
        }

        CancellationToken cancellationToken;

        public CancellationToken CancellationToken
        {
            get { return cancellationToken; }
            set { cancellationToken = value; }
        }

        int totalProgress;

        public int TotalProgress
        {
            get
            {
                return (totalProgress);
            }
            set
            {
                SetProperty(ref totalProgress, value);
            }
        }

        int totalProgressMax;

        public int TotalProgressMax
        {
            get
            {
                return (totalProgressMax);
            }
            set
            {
                SetProperty(ref totalProgressMax, value);
            }
        }

        int itemProgress;

        public int ItemProgress
        {
            get
            {
                return (itemProgress);
            }
            set
            {
                SetProperty(ref itemProgress, value);
            }
        }

        int itemProgressMax;

        public int ItemProgressMax
        {
            get
            {
                return (itemProgressMax);
            }
            set
            {
                SetProperty(ref itemProgressMax, value);
            }
        }

        String windowTitle;

        public String WindowTitle
        {
            get { return windowTitle; }
            set
            {
                SetProperty(ref windowTitle, value);
            }
        }

        string windowIcon;

        public string WindowIcon
        {
            get
            {
                return (windowIcon);
            }
            set
            {
                SetProperty(ref windowIcon, value);
            }
        }
    }
}
