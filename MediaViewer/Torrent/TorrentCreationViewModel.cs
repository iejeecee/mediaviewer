using MediaViewer.DirectoryPicker;
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
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel.Composition;
using MediaViewer.Settings;
using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Commands;

namespace MediaViewer.Torrent
{
    public class TorrentCreationViewModel : CloseableBindableBase
    {
   
        AppSettings Settings
        {
            get;
            set;
        }

        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TorrentCreationViewModel(AppSettings settings)
        {
            Settings = settings;

            IsPrivate = false;                     
            IsCommentEnabled = false;

            OutputPathHistory = new ObservableCollection<string>();
            AnnounceURLHistory = settings.TorrentAnnounceHistory;
            if (AnnounceURLHistory.Count > 0)
            {
                AnnounceURL = AnnounceURLHistory[0];
            }

            DirectoryPickerCommand = new Command(new Action(() =>
            {
                DirectoryPickerView directoryPicker = new DirectoryPickerView();
                DirectoryPickerViewModel vm = (DirectoryPickerViewModel)directoryPicker.DataContext;
                vm.MovePath = OutputPath;
                vm.MovePathHistory = OutputPathHistory;

                if (directoryPicker.ShowDialog() == true)
                {
                    OutputPath = vm.MovePath;
                }

            }));

            CancelCommand = new Command(() =>
            {
                OnClosingRequest();
            });

            OkCommand = new Command(async () =>
                {
                    CancellableOperationProgressView progress = new CancellableOperationProgressView();
                    TorrentCreationProgressViewModel vm = new TorrentCreationProgressViewModel(Settings);
                    progress.DataContext = vm;
                    Task task = vm.createTorrentAsync(this);
                    progress.Show();                    
                    OnClosingRequest();
                    await task;
                                                                     
                });
        }

        
   
        String announceURL;

        public String AnnounceURL
        {
            get { return announceURL; }
            set { 
                SetProperty(ref announceURL, value);           
            }
        }

        ObservableCollection<String> announceURLHistory;

        public ObservableCollection<String> AnnounceURLHistory
        {
            get { return announceURLHistory; }
            set { announceURLHistory = value; }
        }

        bool isCommentEnabled;

        public bool IsCommentEnabled
        {
            get { return isCommentEnabled; }
            set {
                SetProperty(ref isCommentEnabled, value);       
            }
        }

        String comment;

        public String Comment
        {
            get { return comment; }
            set { 
            SetProperty(ref comment, value);
            }
        }
      
        String outputPath;

        public String OutputPath
        {
            get { return outputPath; }
            set { 
            SetProperty(ref outputPath, value);
            }
        }

      
        String pathRoot;

        public String PathRoot
        {
            get { return pathRoot; }
            set
            {
                pathRoot = value;
                
            }
        }
        ICollection<MediaFileItem> media;

        public ICollection<MediaFileItem> Media
        {
            get { return media; }
            set { media = value;
            getPathRoot();
            }
        }
        bool isPrivate;

        public bool IsPrivate
        {
            get { return isPrivate; }
            set { 
            SetProperty(ref isPrivate, value);
            }
        }

        Command cancelCommand;

        public Command CancelCommand
        {
            get { return cancelCommand; }
            set { cancelCommand = value; }
        }
        Command okCommand;

        public Command OkCommand
        {
            get { return okCommand; }
            set { okCommand = value; }
        }

        Command directoryPickerCommand;

        public Command DirectoryPickerCommand
        {
            get { return directoryPickerCommand; }
            set { directoryPickerCommand = value; }
        }

        ObservableCollection<String> outputPathHistory;

        public ObservableCollection<String> OutputPathHistory
        {
            get { return outputPathHistory; }
            set { outputPathHistory = value; }
        }

        String torrentName;
        public string TorrentName {
            get { return torrentName; }
            set
            {
                SetProperty(ref torrentName, value);
            }
        }

        void getPathRoot()
        {
            if (media == null || media.Count == 0)
            {
                pathRoot = "";
                return;
            }

            pathRoot = MediaViewer.Model.Utils.FileUtils.getPathWithoutFileName(Media.ElementAt(0).Location);

            for (int i = 1; i < Media.Count; i++)
            {
                String newPathRoot = "";

                for (int j = 0; j < Math.Min(Media.ElementAt(i).Location.Length, pathRoot.Length); j++)
                {

                    if (pathRoot[j] == Media.ElementAt(i).Location[j])
                    {
                        newPathRoot += Media.ElementAt(i).Location[j];
                    }
                    else
                    {
                        break;
                    }
                }

                if (String.IsNullOrEmpty(newPathRoot))
                {
                    throw new Exception("When adding multiple files to a torrent, they need to share the same root drive");
                }

                pathRoot = newPathRoot;
            }

            OutputPath = pathRoot = pathRoot.TrimEnd(new char[]{'\\','/'});

            if (Media.Count == 1)
            {
                TorrentName = Path.GetFileNameWithoutExtension(Media.ElementAt(0).Location);
            }
            else
            {
                string[] rootDirs = pathRoot.Split(new char[] { '\\' });

                TorrentName = rootDirs[rootDirs.Length - 1].Contains(':') ? "files" : rootDirs[rootDirs.Length - 1];
            }
        }
    }
}
