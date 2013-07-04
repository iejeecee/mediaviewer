using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MediaViewer.MetaData;
using MediaViewer.Utils;

namespace MediaViewer.MediaFileModel
{
    class ImageFile : MediaFile
{

	private const int THUMBNAIL_DATA = 0x501B;
	private const int IMAGE_WIDTH = 0xA002;
	private const int IMAGE_HEIGHT = 0x0101;

	private int width;
	private int height;


	public override void readMetaData() {

	
		
/*
		for(int i = 0; i < imageMetaData.PropertyIdList.Length; i++) {

			if(imageMetaData.PropertyIdList[i] == IMAGE_WIDTH)
			{
				PropertyItem p = imageMetaData.GetPropertyItem(IMAGE_WIDTH);
				if(p.Value != null) {

					if(p.Len == 2) continue;

					IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(p.Value, 0);
					
					cli.array<int> bla = new cli.array<int>(p.Len / 4);

					Marshal.Copy(ptr, bla, 0, p.Len / 4);

					width = bla[0];
				}
	
			}

			if(imageMetaData.PropertyIdList[i] == IMAGE_HEIGHT)
			{
				PropertyItem p = imageMetaData.GetPropertyItem(IMAGE_HEIGHT);
				if(p.Value != null) {

					IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(p.Value, 0);
					
					cli.array<short> bla = new cli.array<short>(p.Len / 2);

					Marshal.Copy(ptr, bla, 0, p.Len / 2);

					height = bla[0];
				}
	
			}

		}

*/


        base.readMetaData();
		
	}

    public ImageFile(string location, string mimeType, Stream data, MediaFile.MetaDataMode mode) 
		: base(location, mimeType, data, mode) 
	{
        width = 0;
        height = 0;
		sizeBytes = data.Length;
	}

    public override void generateThumbnails(int nrThumbnails)
    {

        BitmapDecoder bitmap = BitmapDecoder.Create(Data,
            BitmapCreateOptions.DelayCreation,
            BitmapCacheOption.OnDemand);
        BitmapFrame frame = bitmap.Frames[0];

        BitmapSource thumb = frame.Thumbnail;

        if (thumb == null)
        {
            data.Position = 0;

            var tempImage = new BitmapImage();
            tempImage.BeginInit();
            tempImage.CacheOption = BitmapCacheOption.OnLoad;
            tempImage.StreamSource = Data;
            tempImage.DecodePixelWidth = MAX_THUMBNAIL_WIDTH;
            tempImage.EndInit();

            thumb = tempImage;
        }

        Thumbnail = thumb;

        base.generateThumbnails(nrThumbnails);
		
	}

    public override string DefaultCaption
    {
        get
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Path.GetFileName(Location));
            sb.AppendLine();

            if (MetaData != null)
            {

                if (!string.IsNullOrEmpty(MetaData.Description))
                {

                    sb.AppendLine("Description:");

                    //string temp = System.Text.RegularExpressions.Regex.Replace(MetaData.Description,"(.{50}\\s)","$1`n");
                    sb.AppendLine(MetaData.Description);
                    sb.AppendLine();
                }

                if (!string.IsNullOrEmpty(MetaData.Creator))
                {

                    sb.AppendLine("Creator:");
                    sb.AppendLine(MetaData.Creator);
                    sb.AppendLine();

                }

                if (MetaData.CreationDate != DateTime.MinValue)
                {

                    sb.AppendLine("Creation date:");
                    sb.Append(MetaData.CreationDate);
                    sb.AppendLine();
                }
            }

            return (sb.ToString());
        }
	}

    public override string DefaultFormatCaption
    {
        get
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Path.GetFileName(Location));
            sb.AppendLine();

            sb.AppendLine("Mime type:");
            sb.Append(MimeType.Replace("//","/"));
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine("Resolution:");
            sb.Append(width);
            sb.Append("x");
            sb.Append(height);
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine("Size:");
            sb.Append(Misc.formatSizeBytes(sizeBytes));

            return (sb.ToString());
        }
	}

    public override MediaType MediaFormat
	{
		get {

			return(MediaType.IMAGE);
		}
	}

    public int Width
    {

		get {

			return(width);
		}
	}

    public int Height
    {

		get {

			return(height);
		}
	}

    public override void close()
    {


        base.close();
	}
}


}
