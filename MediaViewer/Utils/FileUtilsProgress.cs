using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.Utils
{
    class FileUtilsProgress : CloseableObservableObject
    {
        CancellationTokenSource tokenSource;

        public FileUtilsProgress()
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

        String messages;

        public String Messages
        {
            get { return messages; }
            set { messages = value;
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
