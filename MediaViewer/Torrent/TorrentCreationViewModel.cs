using MediaViewer.MediaFileModel.Watcher;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Torrent
{
    class TorrentCreationViewModel : ObservableObject
    {
        

        public TorrentCreationViewModel()
        {
            pieceLength = (int)Math.Pow(2, 19); // 512kb
            createdBy = "MediaViewer";
            IsPrivate = false;
            OutputDirectory = "MyFiles";
            encoding = "UTF-8";
        }

        public void createTorrent()
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
            info["pieces"] = new BString(buildPiecesHash(media));            
            info["piece length"] = new BInteger(pieceLength);
            info["private"] = new BInteger(isPrivate ? 1 : 0);

            if (media.Count == 1)
            {
                singleFileTorrent(info, new FileInfo(media[0].Location));

            }
            else
            {

                List<FileInfo> fileInfo = new List<FileInfo>();

                foreach (MediaFileItem item in media)
                {

                    fileInfo.Add(new FileInfo(item.Location));
                }

                multiFileTorrent(info, pathRoot, fileInfo);
            }

            BDictionary metaInfo = new BDictionary();

            metaInfo["info"] = info;
            metaInfo["announce"] = new BString(announce.ToString());           
            metaInfo["creation date"] = new BInteger((long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds);
            metaInfo["created by"] = new BString(createdBy);

            if (!String.IsNullOrEmpty(comment) && !String.IsNullOrWhiteSpace(comment))
            {
                metaInfo["comment"] = new BString(comment);
            }
           
            if (!String.IsNullOrEmpty(encoding) && !String.IsNullOrWhiteSpace(encoding))
            {
                metaInfo["encoding"] = new BString(encoding);
            }
         
            FileStream outputTorrent = new FileStream("d://test.torrent", FileMode.Create);

            BinaryWriter bw = new BinaryWriter(outputTorrent);

            foreach (char c in metaInfo.ToBencodedString())
            {
                bw.Write((byte)c);
            }

            outputTorrent.Close();
        }

        void singleFileTorrent(BDictionary info, FileInfo file)
        {
            info["name"] = new BString(file.Name);
            info["length"] = new BInteger(file.Length);
        }

        void multiFileTorrent(BDictionary info, String pathRoot, List<FileInfo> files)
        {
            BList filesList = new BList();

            foreach (FileInfo file in files)
            {
                BDictionary fileDictionary = new BDictionary();
                fileDictionary["length"] = new BInteger(file.Length);

                BList pathList = new BList();

                String relativePath = file.FullName.Remove(0, pathRoot.Length);
                foreach (String elem in relativePath.Split(new char[]{'/'}))
                {
                    pathList.Add(elem);
                }

                fileDictionary["path"] = pathList;

                filesList.Add(fileDictionary);
            }

            info["name"] = new BString(outputDirectory);
            info["files"] = filesList;

        }

        byte[] buildPiecesHash(List<MediaFileItem> items)
        {
            byte[] piece = new byte[pieceLength];
            byte[] result;
            MemoryStream hashStream = new MemoryStream();        
            SHA1 sha = new SHA1CryptoServiceProvider();
            int bytesRead = 0;
            int bytesToRead = pieceLength;
            int offset = 0;

            for(int i = 0; i < items.Count; i++)
            {
                FileStream fileStream = new FileStream(items[i].Location, FileMode.Open);
                fileStream.Seek(0, SeekOrigin.Begin);

                do
                {
                    bytesToRead = pieceLength - offset;

                    bytesRead = fileStream.Read(piece, offset, bytesToRead);

                    offset = (offset + bytesRead) % pieceLength;

                    if (bytesToRead == bytesRead)
                    {                                                
                        result = sha.ComputeHash(piece);

                        hashStream.Write(result, 0, 20);

                        Array.Clear(piece, 0, pieceLength);
                                              
                    }

                } while (bytesRead == bytesToRead);

                fileStream.Close();
            }

            if (bytesToRead != bytesRead)
            {                           
                result = sha.ComputeHash(piece, 0, offset);

                hashStream.Write(result, 0, 20);
            }
                                 
            byte[] hashArray = hashStream.ToArray();

            hashStream.Close();

            return(hashArray);
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        Uri announce;

        public Uri Announce
        {
            get { return announce; }
            set { announce = value;
            NotifyPropertyChanged();
            }
        }
        String comment;

        public String Comment
        {
            get { return comment; }
            set { comment = value;
            NotifyPropertyChanged();
            }
        }

        String createdBy;
        String encoding;       
        int pieceLength;

        String outputDirectory;

        public String OutputDirectory
        {
            get { return outputDirectory; }
            set { outputDirectory = value;
            NotifyPropertyChanged();
            }
        }
        String pathRoot;

        public String PathRoot
        {
            get { return pathRoot; }
            set { pathRoot = value; }
        }
        List<MediaFileItem> media;

        public List<MediaFileItem> Media
        {
            get { return media; }
            set { media = value; }
        }
        bool isPrivate;

        public bool IsPrivate
        {
            get { return isPrivate; }
            set { isPrivate = value;
            NotifyPropertyChanged();
            }
        }
       

    }
}
