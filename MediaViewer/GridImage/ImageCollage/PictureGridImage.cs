using MediaViewer.MediaDatabase;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MediaViewer.GridImage.ImageCollage
{
    class PictureGridImage : GridImageBase
    {
        static List<BitmapSource> getImages(List<MediaFileItem> items, bool useThumbs)
        {
            List<BitmapSource> images = new List<BitmapSource>();

            foreach(MediaFileItem item in items) {

                if (MediaFormatConvert.isImageFile(item.Location) && !useThumbs)
                {
                    images.Add(new BitmapImage(new Uri(item.Location)));
                }
                else 
                {
                    if (item.Media != null && item.Media.Thumbnail != null)
                    {
                        images.Add(item.Media.Thumbnail.Image);
                    }
                }
            }

            return (images);
        }

        public PictureGridImage(ImageCollageViewModel vm, List<MediaFileItem> items, bool useThumbs = false) :
            base(vm.MaxWidth, (int)Math.Ceiling(vm.Media.Count / (double)vm.NrColumns), 
                vm.NrColumns, getImages(items, useThumbs), Stretch.Uniform)
        {                                 
            Items = items;
            Vm = vm;           
        }

        ImageCollageViewModel Vm { get; set; }
        List<MediaFileItem> Items { get; set; }

        protected override void createHeader(Grid mainGrid, string fontFamily, int margin)
        {
            if (Vm.IsAddHeader == false) return;

            Grid headerGrid = new Grid();
            headerGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            headerGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            headerGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            headerGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            Grid.SetRow(headerGrid, 0);
            mainGrid.Children.Add(headerGrid);

            TextBlock name = new TextBlock();
            name.TextWrapping = TextWrapping.Wrap;
            name.Margin = new Thickness(margin);
            name.FontSize = Vm.FontSize;
            name.Text = Vm.Filename;

            Grid.SetRow(name, 0);
            headerGrid.Children.Add(name);

            TextBlock header = new TextBlock();
            header.TextTrimming = TextTrimming.CharacterEllipsis;      
            header.Margin = new Thickness(margin);
            header.FontSize = Vm.FontSize;

            long sizeBytes = 0;

            foreach (MediaFileItem item in Items)
            {
                if (item.Media != null)
                {
                    sizeBytes += item.Media.SizeBytes;
                }
            }

            header.Text = Items.Count + " Items, Size: " + MiscUtils.formatSizeBytes(sizeBytes);

            Grid.SetRow(header, 1);
            headerGrid.Children.Add(header);

            if (Vm.IsCommentEnabled && !String.IsNullOrEmpty(Vm.Comment))
            {
                TextBlock comment = new TextBlock();
                comment.TextWrapping = TextWrapping.Wrap;
                //comment.FontFamily = new FontFamily(fontFamily);
                comment.Margin = new Thickness(margin);
                comment.FontSize = Vm.FontSize;

                comment.Text = Vm.Comment;

                Grid.SetRow(comment, 2);
                headerGrid.Children.Add(comment);
            }

            Separator seperator = new Separator();
            Grid.SetRow(seperator, 3);
            headerGrid.Children.Add(seperator);

        }

        protected override void addImageInfo(int imageNr, Grid cell, string fontFamily, int margin)
        {
            if (Vm.IsAddInfo == false) return;

            cell.Margin = new Thickness(margin);

            MediaFileItem item = Items[imageNr];

            TextBlock name = new TextBlock();
            name.TextTrimming = TextTrimming.CharacterEllipsis;
            name.HorizontalAlignment = HorizontalAlignment.Center;
            name.VerticalAlignment = VerticalAlignment.Bottom;

            name.Text = Path.GetFileName(item.Location);

            Grid.SetRow(name, 0);

            cell.Children.Add(name);

            if (item.Media == null) return;

            VideoMedia videoInfo = item.Media as VideoMedia;
            ImageMedia imageInfo = item.Media as ImageMedia;

            TextBlock info = new TextBlock();
            info.TextTrimming = TextTrimming.CharacterEllipsis;
            info.HorizontalAlignment = HorizontalAlignment.Center;
            info.VerticalAlignment = VerticalAlignment.Top;

            String infoText = "";

            if (videoInfo != null)
            {
                infoText += videoInfo.Width + "x" + videoInfo.Height;
            }
            else
            {
                infoText += imageInfo.Width + "x" + imageInfo.Height;
            }

            infoText += ", " + MiscUtils.formatSizeBytes(item.Media.SizeBytes);

            info.Text = infoText;

            Grid.SetRow(info, 2);

            cell.Children.Add(info);
        
        }
    }
}
