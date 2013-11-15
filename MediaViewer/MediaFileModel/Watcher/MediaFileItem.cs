using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MediaViewer.MediaFileModel.Watcher
{
    public class MediaFileItem : ObservableObject, IEquatable<MediaFileItem>
    {

        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        MediaFileItemState state;

        public MediaFileItemState ItemState
        {
            get { return state; }
            set
            {
                state = value;
                NotifyPropertyChanged();
            }
        }

        public MediaFileItem(String location)
        {

            Location = location;
            IsSelected = false;
            Media = null;
            ItemState = MediaFileItemState.EMPTY;

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

        public void loadMetaData(MediaFile.MetaDataLoadOptions options, CancellationToken token)
        {
            ItemState = MediaFileItemState.LOADING;

            MediaFile media = null;
            MediaFileItemState result = MediaFileItemState.LOADED;

            try
            {
                media = MediaFileFactory.open(Location, options, token);

                media.close();

                if (media.OpenError != null)
                {
                    result = MediaFileItemState.ERROR;
                }
            }
            catch (TaskCanceledException)
            {
                result = MediaFileItemState.CANCELLED;
            }
            catch (Exception e)
            {
                result = MediaFileItemState.ERROR;
                log.Info("Error loading image grid item:" + Location, e);

            }

            ItemState = result;
            Media = media;
        }

        public async Task loadMetaDataAsync(MediaFile.MetaDataLoadOptions options, CancellationToken token)
        {

            ItemState = MediaFileItemState.LOADING;

            MediaFile media = null;
            MediaFileItemState result = MediaFileItemState.LOADED;

            try
            {
                media = await MediaFileFactory.openAsync(Location, options, token).ConfigureAwait(false);

                media.close();

                if (media.OpenError != null)
                {
                    result = MediaFileItemState.ERROR;
                }
            }
            catch (TaskCanceledException)
            {
                result = MediaFileItemState.CANCELLED;
            }
            catch (Exception e)
            {
                result = MediaFileItemState.ERROR;
                log.Info("Error loading image grid item:" + Location, e);

            }

            // assign the results on the UI thread           
            DispatcherOperation task = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                ItemState = result;
                Media = media;
            }));

        }

   
        public bool Equals(MediaFileItem other)
        {
            if (other == null) return (false);

            return (Location.Equals(other.Location));
        }
    }
}
