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
    public class ImageGridItem : ObservableObject
    {
       

        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public enum State
        {
            EMPTY,
            LOADING,
            LOADED,
            ERROR
        }

        State state;

        public State ItemState
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
            ItemState = State.EMPTY;

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
        
            ItemState = State.LOADING;

            MediaFile media = null;
            State result = State.LOADED;

            try
            {
                media = await MediaFileFactory.openAsync(Location, MediaFile.MetaDataMode.LOAD_FROM_DISK, token).ConfigureAwait(false);

                media.close();

                if (media.OpenError != null)
                {
                    result = State.ERROR;
                }
            }
            catch (TaskCanceledException)
            {
                
            }
            catch (Exception e)
            {
                log.Info("Error loading image grid item:" + Location, e);
                
            }

            // assign the results on the UI thread   
            Dispatcher dispatcher = Application.Current.Dispatcher;

            DispatcherOperation task = dispatcher.BeginInvoke(new Action(() =>
            {
                ItemState = result;
                Media = media;
            }));

        }
    }
}
