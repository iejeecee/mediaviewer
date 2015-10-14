using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Utils.Windows;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.VideoPanel
{
    public class VideoOpenLocationViewModel : CloseableBindableBase
    {
        public Command VideoLocationCommand { get; set; }
        public Command AudioLocationCommand { get; set; }
        public Command CloseCommand { get; set; }
        public Command OpenCommand { get; set; }

        string videoLocation;
        public String VideoLocation
        {
            get
            {
                return (videoLocation);
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    OpenCommand.IsExecutable = false;
                }
                else
                {
                    OpenCommand.IsExecutable = true;
                }

                SetProperty(ref videoLocation,value);                
            }
        }

        string audioLocation;
        public String AudioLocation
        {
            get
            {
                return (audioLocation);
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    OpenCommand.IsExecutable = false;
                }
                else
                {
                    OpenCommand.IsExecutable = true;
                }

                SetProperty(ref audioLocation, value);
            }
        }

        public VideoOpenLocationViewModel()
        {
            VideoLocationCommand = new Command(() =>
            {
                Microsoft.Win32.OpenFileDialog dialog = FileDialog.createOpenMediaFileDialog(FileDialog.MediaDialogType.VIDEO);
                bool? success = dialog.ShowDialog();
                if (success == true)
                {
                    VideoLocation = dialog.FileName;                   
                }

            });

            AudioLocationCommand = new Command(() =>
            {
                Microsoft.Win32.OpenFileDialog dialog = FileDialog.createOpenMediaFileDialog(FileDialog.MediaDialogType.AUDIO);
                bool? success = dialog.ShowDialog();
                if (success == true)
                {
                    AudioLocation = dialog.FileName;
                }

            });

            CloseCommand = new Command(() =>
                {
                    OnClosingRequest(new DialogEventArgs(DialogMode.CANCEL));
                });

            OpenCommand = new Command(() =>
                {
                    OnClosingRequest(new DialogEventArgs(DialogMode.SUBMIT));
                },false);
        }
    }
}
