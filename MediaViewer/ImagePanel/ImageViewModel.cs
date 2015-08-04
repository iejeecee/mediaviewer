using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MediaViewer.Model.Media.File;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows;
using System.Threading;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.UserControls.Pager;
using System.Collections.Specialized;
using MediaViewer.MediaDatabase;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using MediaViewer.MetaData;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Utils;
using Microsoft.Practices.Prism.PubSubEvents;
using MediaViewer.Model.Global.Events;
using System.IO;
using Microsoft.Practices.Prism.Commands;
using MediaViewer.Model.Mvvm;
using MediaViewer.MediaFileBrowser;
using MediaViewer.Infrastructure.Logging;
using MediaViewer.Infrastructure.Global.Events;
using MediaViewer.Model.Media.Base;
using MediaViewer.Infrastructure.Utils;
using System.Windows.Data;
using MediaViewer.Model.Utils.Windows;


namespace MediaViewer.ImagePanel
{

    public class ImageViewModel : BindableBase
    {        
        IEventAggregator EventAggregator { get; set; }
        List<String> scaleModes = new List<string>(){"Unscaled","Auto","Fit Width","Fith Height"};
        List<String> rotationModes = new List<string>() { "None", "90°", "180°", "270°" };

        public Command OpenLocationCommand { get; set; }

        public ImageViewModel(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
        
            isLoading = false;
  
            SelectedScaleMode = UserControls.ImagePanel.ScaleMode.UNSCALED;

            ScaleModes = new ListCollectionView(scaleModes);
            ScaleModes.CurrentChanged += ScaleModes_CurrentChanged;

            RotationModes = new ListCollectionView(rotationModes);
            RotationModes.CurrentChanged += RotationModes_CurrentChanged;

            OpenLocationCommand = new Command(() =>
            {
                Microsoft.Win32.OpenFileDialog dialog = FileDialog.createOpenMediaFileDialog(FileDialog.MediaDialogType.IMAGE);
                bool? success = dialog.ShowDialog();
                if (success == true)
                {
                    Location = dialog.FileName;
                }
            });
        }

        void RotationModes_CurrentChanged(object sender, EventArgs e)
        {
            switch ((string)RotationModes.CurrentItem)
            {
                case "None":
                    {
                        RotationDegrees = 0;
                        break;
                    }
                case "90°":
                    {
                        RotationDegrees = 90;
                        break;
                    }
                case "180°":
                    {
                        RotationDegrees = 180;
                        break;
                    }
                case "270°":
                    {
                        RotationDegrees = 270;
                        break;
                    }
            }
        }

        void ScaleModes_CurrentChanged(object sender, EventArgs e)
        {
            switch ((string)ScaleModes.CurrentItem)
            {
                case "Unscaled":
                    {
                        SelectedScaleMode = UserControls.ImagePanel.ScaleMode.UNSCALED;
                        break;
                    }
                case "Auto":
                    {
                        SelectedScaleMode = UserControls.ImagePanel.ScaleMode.FIT_HEIGHT_AND_WIDTH;
                        break;
                    }
                case "Fit Width":
                    {
                        SelectedScaleMode = UserControls.ImagePanel.ScaleMode.FIT_WIDTH;
                        break;
                    }
                case "Fith Height":
                    {
                        SelectedScaleMode = UserControls.ImagePanel.ScaleMode.FIT_HEIGHT;
                        break;
                    }
            }
        }

        public ListCollectionView ScaleModes { get; set; }
        public ListCollectionView RotationModes { get; set; }

        bool flipX;

        public bool FlipX
        {
            get { return flipX; }
            set
            {              
                SetProperty(ref flipX, value);
             
            }
        }

        bool flipY;

        public bool FlipY
        {
            get { return flipY; }
            set
            {            
                SetProperty(ref flipY, value);          
            }
        }

        bool isLoading;

        public bool IsLoading
        {
            get { return isLoading; }
            set
            {               
                SetProperty(ref isLoading, value);
            }
        }

        double rotationDegrees;

        public double RotationDegrees
        {
            get { return rotationDegrees; }
            set
            {
                SetProperty(ref rotationDegrees, value);           
            }
        }

        double scale;

        public double Scale
        {
            get { return scale; }
            set
            {               
                SetProperty(ref scale, value);              
            }
        }

        MediaItem currentItem;

        public MediaItem CurrentItem
        {
            get { return currentItem; }
            set {

                if (value != null)
                {
                    Location = value.Location;
                }

                SetProperty(ref currentItem, value); 
            }
        }

        String location;

        public String Location
        {
            get { return location; }
            private set {  
                SetProperty(ref location, value);
            }
        }

        MediaViewer.UserControls.ImagePanel.ScaleMode selectedScaleMode;

        public MediaViewer.UserControls.ImagePanel.ScaleMode SelectedScaleMode
        {
            get { return selectedScaleMode; }
            set
            {                                                
                SetProperty(ref selectedScaleMode, value);
            }
        }

        public void OnNavigatedTo(Microsoft.Practices.Prism.Regions.NavigationContext navigationContext)
        {
            MediaItem item = (MediaItem)navigationContext.Parameters["item"];
                          
            CurrentItem = item;
            
            EventAggregator.GetEvent<TitleChangedEvent>().Publish(CurrentItem != null ? CurrentItem.Name : null);       

            EventAggregator.GetEvent<MediaSelectionEvent>().Subscribe(imageView_MediaSelectionEvent, ThreadOption.UIThread);
        }

        public void OnNavigatedFrom(Microsoft.Practices.Prism.Regions.NavigationContext navigationContext)
        {
            EventAggregator.GetEvent<MediaSelectionEvent>().Unsubscribe(imageView_MediaSelectionEvent);
        }

        private void imageView_MediaSelectionEvent(MediaSelectionPayload selection)
        {
            if (selection.Items.Count() == 0) return;

            MediaItem first = selection.Items.ElementAt(0);

            if (String.Equals(Location, first.Location)) return;

            CurrentItem = first;

            EventAggregator.GetEvent<TitleChangedEvent>().Publish(first.Name);
        }
    }

}
