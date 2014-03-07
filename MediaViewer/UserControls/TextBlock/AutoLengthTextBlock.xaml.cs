using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaViewer.UserControls.TextBlock
{
    /// <summary>
    /// Interaction logic for AutoLengthTextBlock.xaml
    /// </summary>
    public partial class AutoLengthTextBlock : UserControl
    {
        public AutoLengthTextBlock()
        {
            InitializeComponent();
            fullText = null;
        }

        private void root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            setText();
        }

        String fullText;

        String FullText
        {
          get { return fullText; }
          set { fullText = value; }
        }

        public String Text
          {
            get { return (String)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(String), typeof(AutoLengthTextBlock), new PropertyMetadata(null,new PropertyChangedCallback(textChangedCallback)));

        private static void textChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
 	        AutoLengthTextBlock t = (AutoLengthTextBlock)d;
            t.FullText = (String)e.NewValue;
            t.setText();            

        }

        void setText() {

            if(FullText == null) {

                textBlock.Text = "";
                return;
            }

            Size size = MeasureString(FullText);

            if(size.Width + AutoPadding <= textBlock.ActualWidth) {
               
               textBlock.Padding = new Thickness(AutoPadding, 0, 0, 0);
               textBlock.Text = FullText;

            } else if(size.Width + 5 <= textBlock.ActualWidth) {

               textBlock.Padding = new Thickness(textBlock.ActualWidth - (size.Width + 5), 0, 0, 0);
               textBlock.Text = FullText;

            } else {

               textBlock.Padding = new Thickness(0, 0, 0, 0);

               String shortText = FullText;
               while(shortText.Length > 0) {

                   shortText = shortText.Substring(0, shortText.Length - 1);

                   Size shortSize = MeasureString(shortText + "...");

                   if(shortSize.Width <= textBlock.ActualWidth) {

                       textBlock.Text = shortText + "...";
                       return;
                   }
               }

               textBlock.Text = shortText + "...";
              
            }

        }


        Size MeasureString(string candidate)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(this.textBlock.FontFamily, this.textBlock.FontStyle, this.textBlock.FontWeight, this.textBlock.FontStretch),
                this.textBlock.FontSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(AutoLengthTextBlock), new PropertyMetadata(TextAlignment.Left, new PropertyChangedCallback(TextAlignmentChanged)));

        private static void TextAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoLengthTextBlock t = (AutoLengthTextBlock)d;
            t.textBlock.TextAlignment = (TextAlignment)e.NewValue;
        }

        public double AutoPadding
        {
            get { return (double)GetValue(AutoPaddingProperty); }
            set { SetValue(AutoPaddingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AutoPadding.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoPaddingProperty =
            DependencyProperty.Register("AutoPadding", typeof(double), typeof(AutoLengthTextBlock), new PropertyMetadata(0.0));               
        
    }
}
