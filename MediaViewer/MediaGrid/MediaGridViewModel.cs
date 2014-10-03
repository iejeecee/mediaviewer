using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.Model.Media.File;
using MvvmFoundation.Wpf;
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
using MediaViewer.Pager;
using MediaViewer.MetaData;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Media.State.CollectionView;


namespace MediaViewer.MediaGrid
{

    public class MediaGridViewModel : MediaStateCollectionViewModel
    {
        
        public MediaGridViewModel(IMediaState mediaState) : base(mediaState)
        {
           
            NrGridColumns = 4;

        }
        
        int nrGridColumns;

        public int NrGridColumns
        {
            get { return nrGridColumns; }
            set
            {
                nrGridColumns = value;
                NotifyPropertyChanged();
            }
        }
    }                                           
}
