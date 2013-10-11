using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.Utils
{
    class FileUtilsProgressViewModel : CloseableObservableObject
    {
        CancellationTokenSource tokenSource;

        public FileUtilsProgressViewModel()
        {
           tokenSource = new CancellationTokenSource();
           cancellationToken = tokenSource.Token;

           cancelCommand = new Command(new Action(() =>
           {
               tokenSource.Cancel();
           }));

           okCommand = new Command(new Action(() =>
           {
               OnClosingRequest();
           }));

           infoMessages = new ObservableCollection<string>();
        }

        Command okCommand;

        public Command OkCommand
        {
            get { return okCommand; }
            set
            {
                okCommand = value;
                NotifyPropertyChanged();
            }
        }     

        Command cancelCommand;

        public Command CancelCommand
        {
            get { return cancelCommand; }
            set { cancelCommand = value;
            NotifyPropertyChanged();
            }
        }     

        int totalFiles;

        public int TotalFiles
        {
            get { return totalFiles; }
            set { totalFiles = value;
            NotifyPropertyChanged();
            }
        }
        int currentFile;

        public int CurrentFile
        {
            get { return currentFile; }
            set { currentFile = value;
            NotifyPropertyChanged();
            }
        }

        int currentFileProgress;

        public int CurrentFileProgress
        {
            get { return currentFileProgress; }
            set { currentFileProgress = value;
            NotifyPropertyChanged();
            }
        }

        String itemInfo;

        public String ItemInfo
        {
            get { return itemInfo; }
            set { itemInfo = value;
            NotifyPropertyChanged();
            }
        }

        ObservableCollection<String> infoMessages;

        public ObservableCollection<String> InfoMessages
        {
            get { return infoMessages; }
            set { infoMessages = value;
            NotifyPropertyChanged();
            }
        }
              
        CancellationToken cancellationToken;

        public CancellationToken CancellationToken
        {
            get { return cancellationToken; }
            set { cancellationToken = value; }
        }
    }
}
