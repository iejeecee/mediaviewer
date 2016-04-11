using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.Progress
{
    public class CancellableOperationProgressBase : NonCancellableOperationProgressBase, IDisposable
    {
        protected CancellationTokenSource CancellationTokenSource { get; set; }

        public CancellableOperationProgressBase()
        {
            InfoMessages = new ObservableCollection<string>();
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationTokenSource.Token;

            OkCommand = new Command(() =>
            {
                OnClosingRequest();
            }, false);

            CancelCommand = new Command(() =>
            {
                if (CancellationTokenSource != null)
                {
                    CancellationTokenSource.Cancel();
                }
            }, true);
            
            ItemProgress = 0;
            ItemProgressMax = 1;

            TotalProgress = 0;
            TotalProgressMax = 1;
           
        }

        String itemInfo;

        public String ItemInfo
        {
            get { return itemInfo; }
            set { SetProperty(ref itemInfo, value); }
        }

        int itemProgress;

        public int ItemProgress
        {
            get { return itemProgress; }
            set { SetProperty(ref itemProgress, value); }
        }

        int itemProgressMax;

        public int ItemProgressMax
        {
            get { return itemProgressMax; }
            set { SetProperty(ref itemProgressMax, value); }
        }       

        public ObservableCollection<String> InfoMessages { get; set; }
        public Command OkCommand { get; set; }
        public Command CancelCommand { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool cleanupManaged)
        {
            if (cleanupManaged)
            {
                if (CancellationTokenSource != null)
                {
                    CancellationTokenSource.Dispose();
                    CancellationTokenSource = null;
                }
            }

        }
    }
}
