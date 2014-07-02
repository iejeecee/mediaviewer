using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.Progress
{
    public interface ICancellableOperationProgress : INonCancellableOperationProgress
    { 
       String ItemInfo { get; set; }
       int ItemProgress { get; set; }
       int ItemProgressMax { get; set; }
       ObservableCollection<String> InfoMessages { get; set; }
       Command OkCommand { get; set; }
       Command CancelCommand { get; set; }
       CancellationToken CancellationToken { get; set; }

    }
}
