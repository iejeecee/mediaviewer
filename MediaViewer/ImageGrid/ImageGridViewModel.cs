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


namespace MediaViewer.ImageGrid
{
            
    public class ImageGridViewModel : ObservableObject, IPageable
    {

        IMediaState mediaState;

        public IMediaState MediaState
        {
            get
            {
                return (mediaState);
            }
        }
       
                    
        public ImageGridViewModel(IMediaState mediaState)
        {
            if (mediaState == null)
            {
                throw new ArgumentNullException("mediaState cannot be null");
            }

            this.mediaState = mediaState;
                               
            NrGridColumns = 4;
                                                  
            NextPageCommand = new Command(() => { });
            PrevPageCommand = new Command(() => { });
            FirstPageCommand = new Command(() => { });
            LastPageCommand = new Command(() => { });

            IsPagingEnabled = false;
        }
     
        String imageGridInfo;

        public String ImageGridInfo
        {
            get { return imageGridInfo; }
            set { imageGridInfo = value;
            NotifyPropertyChanged();
            }
        }

        DateTime imageGridInfoDateTime;

        public DateTime ImageGridInfoDateTime
        {
            get { return imageGridInfoDateTime; }
            set { imageGridInfoDateTime = value; }
        }
                 
             
        int nrGridColumns;

        public int NrGridColumns
        {
            get { return nrGridColumns; }
            set { nrGridColumns = value;
            NotifyPropertyChanged();
            }
        }
        
        public void selectAll()
        {
          
        }

        public void deselectAll()
        {
            
        }
                                
       public int NrPages
       {
           get
           {
               return (0);
           }
           set
           {              
           }
       }

       public int CurrentPage
       {
           get
           {
               return (0);
           }
           set
           {
      
           }
       }

       bool isPagingEnabled;

       public bool IsPagingEnabled
       {
           get
           {
               return (isPagingEnabled);
           }
           set
           {
               isPagingEnabled = value;
               NotifyPropertyChanged();
           }
       }

       Command nextPageCommand;

       public Command NextPageCommand
       {
           get
           {
               return (nextPageCommand);
           }
           set
           {
               nextPageCommand = value;
           }
       }

       Command prevPageCommand;

       public Command PrevPageCommand
       {
           get
           {
               return (prevPageCommand);
           }
           set
           {
               prevPageCommand = value;
           }
       }

       Command firstPageCommand;

       public Command FirstPageCommand
       {
           get
           {
               return (firstPageCommand);
           }
           set
           {
               firstPageCommand = value;
           }
       }

       Command lastPageCommand;

       public Command LastPageCommand
       {
           get
           {
               return (lastPageCommand);
           }
           set
           {
               lastPageCommand = value;
           }
       }
    
    }
}
