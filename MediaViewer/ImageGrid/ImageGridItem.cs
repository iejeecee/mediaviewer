using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.MediaPreview;
using MediaViewer.MediaFileModel;
using MvvmFoundation.Wpf;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace MediaViewer.ImageGrid
{
    public enum ImageGridItemState
    {
        EMPTY,
        LOADING,
        LOADED,
        CANCELLED,
        ERROR
    }

    public class ImageGridItem : ObservableObject
    {
       
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);      

        ImageGridItemState state;

        public ImageGridItemState ItemState
        {
            get { return state; }
            set { state = value;
            NotifyPropertyChanged();           
            }
        }

        public ImageGridItem(String location)
        {

            Location = location;
            IsSelected = false;
            Media = null;
            ItemState = ImageGridItemState.EMPTY;

        }

        string location;

        public string Location
        {
            get { return location; }
            set
            {
                location = value;
                NotifyPropertyChanged();
            }
        }

        bool isSelected;

        public bool IsSelected
        {
            get
            {
                return (isSelected);
            }
            set
            {
                isSelected = value;
                NotifyPropertyChanged();

            }
        }

        public void toggleSelected()
        {
            if (IsSelected == true)
            {
                IsSelected = false;
            }
            else
            {
                IsSelected = true;
            }
        }

        MediaFile media;

        public MediaFile Media
        {
            get { return media; }
            set
            {
                media = value;
                NotifyPropertyChanged();
            }
        }

    
        public async Task loadMediaFileAsync(CancellationToken token)
        {
        
            ItemState = ImageGridItemState.LOADING;

            MediaFile media = null;
            ImageGridItemState result = ImageGridItemState.LOADED;

            try
            {
                media = await MediaFileFactory.openAsync(Location, MediaFile.MetaDataMode.LOAD_FROM_DISK, token).ConfigureAwait(false);

                media.close();

                if (media.OpenError != null)
                {
                    result = ImageGridItemState.ERROR;
                }
            }
            catch (TaskCanceledException)
            {
                result = ImageGridItemState.CANCELLED;
            }
            catch (Exception e)
            {
                result = ImageGridItemState.ERROR;
                log.Info("Error loading image grid item:" + Location, e);
                
            }

            // assign the results on the UI thread           
            DispatcherOperation task = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                ItemState = result;
                Media = media;
            }));

        }
    }
}
