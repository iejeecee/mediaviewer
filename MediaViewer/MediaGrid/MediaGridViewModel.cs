using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.Model.Media.File;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Data;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using System.Collections.Specialized;
using MediaViewer.Model.Media.File.Watcher;
using System.ComponentModel;
using MediaViewer.MediaDatabase;
using Microsoft.Practices.Prism.Regions;
using System.Windows.Input;
using MediaViewer.UserControls.Pager;
using MediaViewer.MetaData;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.MediaFileBrowser;
using Microsoft.Practices.Prism.PubSubEvents;
using MediaViewer.Model.Global.Events;


namespace MediaViewer.MediaGrid
{

    public class MediaGridViewModel : MediaStateCollectionViewModel, IMediaFileBrowserContentViewModel
    {
        IEventAggregator EventAggregator { get; set; }
        
        public MediaGridViewModel(IMediaState mediaState, IEventAggregator eventAggregator) : base(mediaState)
        {
            EventAggregator = eventAggregator;
            NrGridColumns = 4;

        }
        
        int nrGridColumns;

        public int NrGridColumns
        {
            get { return nrGridColumns; }
            set
            {               
                SetProperty(ref nrGridColumns, value);
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            EventAggregator.GetEvent<MediaBatchSelectionEvent>().Publish(MediaStateCollectionView.getSelectedItems());
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
           
        }
    }                                           
}
