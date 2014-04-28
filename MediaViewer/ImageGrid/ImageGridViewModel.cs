using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.MediaFileModel;
using MvvmFoundation.Wpf;
using System.Windows.Data;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using System.Collections.Specialized;
using MediaViewer.MediaFileModel.Watcher;

namespace MediaViewer.ImageGrid
{
    class ImageGridViewModel : ObservableObject
    {

        IMediaState mediaState;

        public IMediaState MediaState
        {
            get
            {
                return (mediaState);
            }
        }

        CancellationTokenSource loadItemsCTS;

       
    
        public ImageGridViewModel(IMediaState mediaState)
        {

            this.mediaState = mediaState;
          
            loadItemsCTS = new CancellationTokenSource();
            
        }
             
             
        public void loadItemRangeAsync(int start, int nrItems)
        {

            // cancel any previously loading items           
            loadItemsCTS.Cancel();          
            // create new cts for the items that need to be loaded
            loadItemsCTS = new CancellationTokenSource();

            MediaState.readMetadataRangeAsync(start, nrItems, loadItemsCTS.Token);
                    
        }
      
    }
}
