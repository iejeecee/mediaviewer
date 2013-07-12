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


        public delegate void CompletedCallback(ImageGridItem item);

        public ImageGridItem(String location)
        {

            Location = location;
            IsSelected = false;
            Media = null;
            IsOpening = false;

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

        bool isOpening;

        public bool IsOpening
        {
            get { return isOpening; }
            set
            {
                isOpening = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsLoaded
        {
            get
            {
                return (Media == null ? false : true);
            }
        }


        public async Task loadMediaFileAsync(CancellationToken token)
        {
            /*
                        if (openMediaAsyncTask != null && !openMediaAsyncTask.IsCompleted)
                        {
                            openMediaAsyncTask.Wait();
                        }

                        IsOpening = true;
                        openMediaAsyncTask = MediaFileFactory.openAsync(Location, MediaFile.MetaDataMode.LOAD_FROM_DISK, token);
 
                        openMediaAsyncTask.ContinueWith(completedTask =>
                        {
                            // assign the results on the UI thread
                            Dispatcher dispatcher = Application.Current.Dispatcher;

                            MediaFile media = completedTask.Result;
                            if (media != null)
                            {
                                media.close();
                            }

                            // run the completed callback on the worker thread
                            if (completedCallback != null)
                            {
                                completedCallback(this);
                            }
                
                            dispatcher.BeginInvoke(new Action(() =>
                            {
                  
                                IsOpening = false;

                                if (completedTask.IsCanceled)
                                {
                                    Media = null;
                                }
                                else
                                {
                                    Media = completedTask.Result;
                                }

                            }));               
                
                        });
             */

            IsOpening = true;

            MediaFile media = null;

            try
            {
                media = await MediaFileFactory.openAsync(Location, MediaFile.MetaDataMode.LOAD_FROM_DISK, token).ConfigureAwait(false);

                media.close();

            }
            catch (Exception)
            {

            }

            // assign the results on the UI thread   
            Dispatcher dispatcher = Application.Current.Dispatcher;

            DispatcherOperation task = dispatcher.BeginInvoke(new Action(() =>
            {
                IsOpening = false;
                Media = media;
            }));

        }
    }
}
