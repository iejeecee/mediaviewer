using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.MetaData;
using MediaViewer.Utils;

namespace MediaViewer.MediaFile
{
    class ImageFile : MediaFileBase
{

	private const int THUMBNAIL_DATA = 0x501B;
	private const int IMAGE_WIDTH = 0xA002;
	private const int IMAGE_HEIGHT = 0x0101;

	private int width;
	private int height;

	private long sizeBytes;

	Image imageMetaData;


	protected override void readMetaData() {

		imageMetaData = Image.FromStream(Data, false, false);
		
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

    public ImageFile(string location, string mimeType, Stream data, MediaFileBase.MetaDataMode mode) 
		: base(location, mimeType, data, mode) 
	{

		sizeBytes = data.Length;
	}

    public override List<MetaDataThumb> generateThumbnails()
    {

		List<MetaDataThumb > thumbs = new List<MetaDataThumb >();

		Image tempImage = null;

		try {

			// GDI+ throws an error if we try to read a  when the imageMetaData
			// doesn't have that . Check to make sure the thumbnail 
			// item exists.
			bool propertyFound = false;

			for(int i = 0; i < imageMetaData.PropertyIdList.Length; i++) {

				if(imageMetaData.PropertyIdList[i] == THUMBNAIL_DATA)
				{
					propertyFound = true;
					break;
				}

			}

			if(propertyFound) {

				PropertyItem p = imageMetaData.GetPropertyItem(THUMBNAIL_DATA);

				// The imageMetaData data is in the form of a byte array. Write all 
				// the bytes to a stream and create a new imageMetaData from that stream
				if(p.Value != null) {

					byte[] imageBytes = p.Value;

					MemoryStream stream = new MemoryStream(imageBytes.Length);
					stream.Write(imageBytes, 0, imageBytes.Length);

					tempImage = Image.FromStream(stream);

				} else {

					tempImage = Image.FromStream(Data, MAX_THUMBNAIL_WIDTH, MAX_THUMBNAIL_HEIGHT);

				}

			} else {

				tempImage = Image.FromStream(Data, MAX_THUMBNAIL_WIDTH, MAX_THUMBNAIL_HEIGHT);
			}


			// scale thumbnail to the right size
			int thumbWidth;
			int thumbHeight;

			ImageUtils.resizeRectangle(tempImage.Width, tempImage.Height, 
				MAX_THUMBNAIL_WIDTH, MAX_THUMBNAIL_HEIGHT, out thumbWidth, out thumbHeight);

			Image thumbImage = null;

			thumbImage = new Bitmap(tempImage, thumbWidth, thumbHeight);

			thumbs.Add(new MetaDataThumb(thumbImage));

			return(thumbs);

		} finally {

			if(tempImage != null) {

				tempImage.Dispose();
			}

		}
	}

    public override string getDefaultCaption()
    {

		StringBuilder sb = new StringBuilder();

		sb.AppendLine(Path.GetFileName(Location));
		sb.AppendLine();

		if(MetaData != null) {

			if(!string.IsNullOrEmpty(MetaData.Description)) {

				sb.AppendLine("Description:");

				//string temp = System.Text.RegularExpressions.Regex.Replace(MetaData.Description,"(.{50}\\s)","$1`n");
				sb.AppendLine(MetaData.Description);
				sb.AppendLine();
			}

			if(!string.IsNullOrEmpty(MetaData.Creator)) {

				sb.AppendLine("Creator:");
				sb.AppendLine(MetaData.Creator);
				sb.AppendLine();

			}

			if(MetaData.CreationDate != DateTime.MinValue) {

				sb.AppendLine("Creation date:");
				sb.Append(MetaData.CreationDate);
				sb.AppendLine();
			}
		}

		return(sb.ToString());
	}

    public override string getDefaultFormatCaption()
    {

		StringBuilder sb = new StringBuilder();

		sb.AppendLine(Path.GetFileName(Location));
		sb.AppendLine();
	
		sb.AppendLine("Mime type:");
		sb.Append(MimeType);
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
	
		return(sb.ToString());
	}

    public MediaType MediaFormat
	{
		virtual override get  {

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

		if(imageMetaData != null) {

			imageMetaData.Dispose();
			imageMetaData = null;
		}

		base.close();
	}
}


}
