using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.Base.Item;
using MediaViewer.UserControls.MediaGridItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MediaViewer.Model.Media.File
{
    class MediaFileInfoIconsCache : InfoIconsCache
    {
        public MediaFileInfoIconsCache(List<BitmapImage> icons) :
            base(icons)
        {

        }

        protected override String getKey(MediaItem item)
        {
            String key = "";

            if (item.Metadata != null)
            {
                if (item.Metadata.IsImported)
                {
                    key += '0';
                }

                if (!item.Metadata.SupportsXMPMetadata)
                {
                    key += '1';
                }
            }

            if (item.HasTags)
            {
                key += '2';
            }

            if (item.HasGeoTag)
            {
                key += '3';
            }

            if (item.IsReadOnly)
            {
                key += '4';
            }

            return (key);
        }

        public override string getToolTip(int iconNr, MediaItem item)
        {
            String key = getKey(item);

            if (String.IsNullOrEmpty(key)) return (null);

            char icon = key[iconNr];

            String toolTip = "";

            switch (icon)
            {
                case '0':
                    {
                        toolTip = "Imported";
                        break;
                    }
                case '1':
                    {
                        toolTip = "Metadata Not Supported";
                        break;
                    }
                case '2':
                    {
                        if (item.Metadata.Tags.Count == 0) break;
                                             
                        toolTip = item.Metadata.Tags.ElementAt(0).Name;
                        int lineLength = toolTip.Length;
                        int maxLineLength = 40;

                        for (int i = 1; i < item.Metadata.Tags.Count; i++)
                        {
                            String name = item.Metadata.Tags.ElementAt(i).Name;
                            String paddedName = ", " + name;

                            if (lineLength + paddedName.Length > maxLineLength)
                            {
                                toolTip += "\n" + name;
                                lineLength = name.Length;                             
                            }
                            else
                            {
                                toolTip += paddedName;
                                lineLength += paddedName.Length;
                            }                           
                        }
                        

                        break;
                    }
                case '3':
                    {
                        toolTip = "Geotagged";
                        break;
                    }
                case '4':
                    {
                        toolTip = "Readonly";
                        break;
                    }
                default:
                    {
                        break;
                    }

            }

            return (toolTip);
        }
    }
}
