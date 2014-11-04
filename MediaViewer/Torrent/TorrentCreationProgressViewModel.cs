using MediaViewer.Model.Media.File;
using MediaViewer.Progress;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel.Composition;
using MediaViewer.Settings;
using VideoPlayerControl;
using MediaViewer.Model.Utils;
using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Commands;

namespace MediaViewer.Torrent
{
    public class TorrentCreationProgressViewModel : CloseableBindableBase, ICancellableOperationProgress
    {
             
        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        AppSettings Settings
        {
            get;
            set;
        }

        CancellationTokenSource tokenSource;
        String createdBy;
        String encoding;
        int pieceLength;

        public TorrentCreationProgressViewModel(AppSettings settings)
        {
            Settings = settings;

            WindowTitle = "Torrent File";
            WindowIcon = "pack://application:,,,/Resources/Icons/torrent.ico";

            pieceLength = (int)Math.Pow(2, 19); // 512kb
            createdBy = App.getAppInfoString();
            encoding = "UTF-8";
            
            InfoMessages = new ObservableCollection<string>();
            tokenSource = new CancellationTokenSource();
            CancellationToken = tokenSource.Token;

            OkCommand = new Command(() =>
            {
                OnClosingRequest();
            });

            CancelCommand = new Command(() =>
            {
                tokenSource.Cancel();
            });

            OkCommand.IsExecutable = false;
            CancelCommand.IsExecutable = true;

        }

        public async Task createTorrentAsync(TorrentCreationViewModel vm)
        {
            await Task.Run(() =>
            {
                try {

                    createTorrent(vm);

                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MiscUtils.insertIntoHistoryCollection(Settings.TorrentAnnounceHistory, vm.AnnounceURL);
                    }));
                    
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error creating torrent file\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    log.Error("Error creating torrent file", e);
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    OkCommand.IsExecutable = true;
                    CancelCommand.IsExecutable = false;
                });
            });
        }

        void createTorrent(TorrentCreationViewModel vm)
        {

            /*            
                info: a dictionary that describes the file(s) of the torrent. There are two possible forms: one for the case of a 'single-file' torrent with no directory structure, and one for the case of a 'multi-file' torrent (see below for details)
                announce: The announce URL of the tracker (string)
                announce-list: (optional) this is an extention to the official specification, offering backwards-compatibility. (list of lists of strings).
                    The official request for a specification change is here.
                creation date: (optional) the creation time of the torrent, in standard UNIX epoch format (integer, seconds since 1-Jan-1970 00:00:00 UTC)
                comment: (optional) free-form textual comments of the author (string)
                created by: (optional) name and version of the program used to create the .torrent (string)
                encoding: (optional) the string encoding format used to generate the pieces part of the info dictionary in the .torrent metafile (string)
            */

            BDictionary info = new BDictionary();
            info["pieces"] = new BString(buildPiecesHash(vm.Media));
            info["piece length"] = new BInteger(pieceLength);
            info["private"] = new BInteger(vm.IsPrivate ? 1 : 0);

            if (vm.Media.Count == 1)
            {
                singleFileTorrent(info, new FileInfo(vm.Media.ElementAt(0).Location));

            }
            else
            {

                List<FileInfo> fileInfo = new List<FileInfo>();

                foreach (MediaFileItem item in vm.Media)
                {

                    fileInfo.Add(new FileInfo(item.Location));
                }

                multiFileTorrent(info, vm, fileInfo);
            }

            BDictionary metaInfo = new BDictionary();

            metaInfo["info"] = info;
            metaInfo["announce"] = new BString(vm.AnnounceURL);
            metaInfo["creation date"] = new BInteger((long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds);
            metaInfo["created by"] = new BString(createdBy);

            if (!String.IsNullOrEmpty(vm.Comment) && !String.IsNullOrWhiteSpace(vm.Comment) && vm.IsCommentEnabled)
            {
                metaInfo["comment"] = new BString(vm.Comment);
            }

            if (!String.IsNullOrEmpty(encoding) && !String.IsNullOrWhiteSpace(encoding) && vm.IsCommentEnabled)
            {
                metaInfo["encoding"] = new BString(encoding);
            }

            String torrentName = String.IsNullOrEmpty(vm.TorrentName) ?
                Path.GetFileNameWithoutExtension(vm.Media.ElementAt(0).Location) : vm.TorrentName;
            String torrentFullName = vm.OutputPath + "\\" + torrentName + ".torrent";

            if (CancellationToken.IsCancellationRequested) return;

            FileStream outputTorrent = null;
            try
            {
                outputTorrent = new FileStream(torrentFullName, FileMode.CreateNew);

                BinaryWriter bw = new BinaryWriter(outputTorrent);

                foreach (char c in metaInfo.ToBencodedString())
                {
                    bw.Write((byte)c);
                }

                InfoMessages.Add("Created torrent file: " + torrentFullName);
            }
            finally
            {
                if (outputTorrent != null)
                {
                    outputTorrent.Close();
                }
            }

        }

        void singleFileTorrent(BDictionary info, FileInfo file)
        {
            info["name"] = new BString(file.Name);
            info["length"] = new BInteger(file.Length);
        }

        void multiFileTorrent(BDictionary info, TorrentCreationViewModel vm, List<FileInfo> files)
        {
            BList filesList = new BList();

            foreach (FileInfo file in files)
            {
                BDictionary fileDictionary = new BDictionary();
                fileDictionary["length"] = new BInteger(file.Length);

                BList pathList = new BList();

                String relativePath = file.FullName.Remove(0, vm.PathRoot.Length);
                foreach (String elem in relativePath.Split(new char[] { '\\' }))
                {
                    if (!String.IsNullOrEmpty(elem))
                    {
                        pathList.Add(elem);
                    }
                }

                fileDictionary["path"] = pathList;

                filesList.Add(fileDictionary);
            }

          

            info["name"] = new BString(vm.TorrentName);
            info["files"] = filesList;

        }

        byte[] buildPiecesHash(ICollection<MediaFileItem> items)
        {
            byte[] piece = new byte[pieceLength];
            byte[] result;
            MemoryStream hashStream = new MemoryStream();
            SHA1 sha = new SHA1CryptoServiceProvider();
            int bytesRead = 0;
            int bytesToRead = pieceLength;
            int offset = 0;

            TotalProgressMax = items.Count;

            for (int i = 0; i < items.Count; i++)
            {
                ItemInfo = "Calculating SHA1 hash for: " + Path.GetFileName(items.ElementAt(i).Location);
               
                FileStream fileStream = null;
                try
                {
                    fileStream = new FileStream(items.ElementAt(i).Location, FileMode.Open);
                    ItemProgressMax = (int)fileStream.Length;
                    fileStream.Seek(0, SeekOrigin.Begin);

                    do
                    {
                        if (CancellationToken.IsCancellationRequested) return(piece);

                        bytesToRead = pieceLength - offset;

                        bytesRead = fileStream.Read(piece, offset, bytesToRead);

                        offset = (offset + bytesRead) % pieceLength;

                        if (bytesToRead == bytesRead)
                        {
                            result = sha.ComputeHash(piece);

                            hashStream.Write(result, 0, 20);

                            Array.Clear(piece, 0, pieceLength);

                        }

                        ItemProgress = (int)fileStream.Position;

                    } while (bytesRead == bytesToRead);

                    TotalProgress = i + 1;
                    InfoMessages.Add("Finished calculating SHA1 hash for: " + Path.GetFileName(items.ElementAt(i).Location));
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Close();
                    }
                }
            }
          
            if (bytesToRead != bytesRead)
            {
                result = sha.ComputeHash(piece, 0, offset);

                hashStream.Write(result, 0, 20);
            }

            byte[] hashArray = hashStream.ToArray();

            hashStream.Close();

            return (hashArray);
        }

        public Command OkCommand {get;set;}   
        public Command CancelCommand {get;set;}
     
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

        string windowTitle;

        public string WindowTitle
        {
            get
            {
                return (windowTitle);
            }
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
                windowIcon = value;
                SetProperty(ref windowIcon, value);               
            }
        }

    }
}
