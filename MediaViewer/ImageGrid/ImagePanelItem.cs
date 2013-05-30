using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.MediaPreview;

namespace MediaViewer.ImageGrid
{
    public class ImagePanelItem : INotifyPropertyChanged
    {
        public ImagePanelItem() {

            asyncState = new MediaPreviewAsyncState();
            isSelected = false;
        }

        MediaPreviewAsyncState asyncState;

        public MediaPreviewAsyncState AsyncState
        {
            get { return asyncState; }
            set
            {
                asyncState = value;

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

                this.isSelected = value;

                NotifyPropertyChanged();
            }

        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
