using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaPreview
{
    public class MediaPreviewAsyncState
    {
        public enum InfoIconModes
        {
            SHOW_ALL_ICONS,
            DEFAULT_ICONS_ONLY,
            CUSTOM_ICONS_ONLY,
            DISABLE_ICONS
        };

        string mediaLocation;
        string caption;
        bool isSelected;
        //ContextMenuStrip contextMenu;
        List<InfoIcon> infoIcon;

        InfoIconModes infoIconMode;

        public MediaPreviewAsyncState()
        {

            this.mediaLocation = "";
            caption = "";

            //contextMenu = null;
            infoIcon = new List<InfoIcon>();

            infoIconMode = InfoIconModes.CUSTOM_ICONS_ONLY;
        }

        public MediaPreviewAsyncState(string mediaLocation)
        {

            this.mediaLocation = mediaLocation;
            caption = "";

            //contextMenu = null;
            infoIcon = new List<InfoIcon>();

            infoIconMode = InfoIconModes.CUSTOM_ICONS_ONLY;
        }

        public string MediaLocation
        {

            get
            {

                return (mediaLocation);
            }

            set
            {

                this.mediaLocation = value;

            }

        }

        public string Caption
        {

            get
            {

                return (caption);
            }

            set
            {

                this.caption = value;

            }

        }

        public bool IsSelected
        {

            get
            {
                return (isSelected);
            }

            set
            {

                this.isSelected = value;

            }

        }

        public bool IsEmpty
        {

            get
            {
                return (string.IsNullOrEmpty(MediaLocation));
            }
           
        }

        public InfoIconModes InfoIconMode
        {

            get
            {
                return (infoIconMode);
            }

            set
            {

                this.infoIconMode = value;

            }

        }
        /*
                public ContextMenuStrip ContextMenu
                {

                    get
                    {

                        return (contextMenu);
                    }

                    set
                    {

                        this.contextMenu = value;
                    }


                }
        */
        public List<InfoIcon> InfoIcon
        {

            get
            {
                return (infoIcon);
            }

            set
            {
                this.infoIcon = value;
            }

        }

        public static readonly MediaPreviewAsyncState Empty = new MediaPreviewAsyncState();
       

    }
}
