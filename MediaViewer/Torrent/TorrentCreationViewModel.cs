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
using MediaViewer.Model.Settings;
using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Commands;
using MediaViewer.Model.Utils;
using MediaViewer.Infrastructure.Logging;
using MediaViewer.Properties;

namespace MediaViewer.Torrent
{
    public class TorrentCreationViewModel : CloseableBindableBase
    {
   
        public TorrentCreationViewModel()
        {
            
            IsPrivate = false;                     
            IsCommentEnabled = false;

            OutputPathHistory = new ObservableCollection<string>();
            InputPathHistory = new ObservableCollection<string>();

            AnnounceURLHistory = Settings.Default.TorrentAnnounceHistory;
            if (AnnounceURLHistory.Count > 0)
            {
                AnnounceURL = AnnounceURLHistory[0];
            }

            InputDirectoryPickerCommand = new Command(new Action(async () =>
            {
                DirectoryPickerView directoryPicker = new DirectoryPickerView();
                DirectoryPickerViewModel vm = (DirectoryPickerViewModel)directoryPicker.DataContext;
                vm.SelectedPath = String.IsNullOrEmpty(InputPath) ? PathRoot : InputPath;
                vm.PathHistory = InputPathHistory;

                if (directoryPicker.ShowDialog() == true)
                {                    
                    InputPath = vm.SelectedPath;
                    TorrentName = Path.GetFileName(InputPath);

                    ScanFilesViewModel scanFilesViewModel = new ScanFilesViewModel();
                    NonCancellableOperationProgressView progressView = new NonCancellableOperationProgressView();
                    progressView.DataContext = scanFilesViewModel;

                    ObservableCollection<MediaFileItem> items = new ObservableCollection<MediaFileItem>();

                    await Task.Factory.StartNew(() =>
                    {
                        App.Current.Dispatcher.BeginInvoke(new Action(() => {

                            progressView.ShowDialog();

                        }));

                        try
                        {
                            items = scanFilesViewModel.getInputMedia(InputPath);
                        }
                        catch (Exception e)
                        {
                            Logger.Log.Error("Error reading: " + inputPath, e);
                            MessageBox.Show("Error reading: " + inputPath + "\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        finally
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                progressView.Close();
                            }));
                        }
                    });                                              

                    Media = items;                                      
                }

            }));

            OutputDirectoryPickerCommand = new Command(new Action(() =>
            {
                DirectoryPickerView directoryPicker = new DirectoryPickerView();
                DirectoryPickerViewModel vm = (DirectoryPickerViewModel)directoryPicker.DataContext;
                vm.SelectedPath = OutputPath;
                vm.PathHistory = OutputPathHistory;

                if (directoryPicker.ShowDialog() == true)
                {
                    OutputPath = vm.SelectedPath;
                }

            }));

            CancelCommand = new Command(() =>
            {
                OnClosingRequest();
            });

            OkCommand = new Command(async () =>
                {
                    CancellableOperationProgressView progress = new CancellableOperationProgressView();
                    TorrentCreationProgressViewModel vm = new TorrentCreationProgressViewModel();
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

        String inputPath;

        public String InputPath
        {
            get { return inputPath; }
            set { SetProperty(ref inputPath, value); }
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
        ObservableCollection<MediaFileItem> media;

        public ObservableCollection<MediaFileItem> Media
        {
            get { return media; }
            set { SetProperty(ref media, value);
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

        public Command CancelCommand { get; set; }
        public Command OkCommand { get; set; }
        public Command InputDirectoryPickerCommand { get; set; }
        public Command OutputDirectoryPickerCommand { get; set; }
       
        ObservableCollection<String> outputPathHistory;

        public ObservableCollection<String> OutputPathHistory
        {
            get { return outputPathHistory; }
            set { SetProperty(ref outputPathHistory, value); }
        }

        ObservableCollection<String> inputPathHistory;

        public ObservableCollection<String> InputPathHistory
        {
            get { return inputPathHistory; }
            set { SetProperty(ref inputPathHistory, value); }
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
                OutputPath = PathRoot;
                OkCommand.IsExecutable = false;
                return;
            }
            else
            {
                OkCommand.IsExecutable = true;
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

            InputPath = OutputPath = pathRoot = pathRoot.TrimEnd(new char[]{'\\','/'});

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
