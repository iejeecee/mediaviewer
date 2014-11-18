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

namespace MediaViewer.UserControls.TextBlockOld
{
    /// <summary>
    /// Interaction logic for AutoLengthTextBlock.xaml
    /// </summary>
    public partial class AutoLengthTextBlock : UserControl
    {
        const double errorMargin = 3;

        public AutoLengthTextBlock()
        {
            InitializeComponent();
            FullText = null;
            
           
        }

        private void root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            setText();
        }

        Size FullTextSize { get; set; }

        string fullText;

        String FullText {

            get
            {
                return (fullText);
            }

            set
            {               
                fullText = value;

                // measure length of the full string whenever it is changed
                if (value == null || String.IsNullOrEmpty(value))
                {
                    FullTextSize = new Size(0, 0);
                }
                else
                {
                    FullTextSize = MeasureString(value);
                }

            }
        
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

            if (FullTextSize.Width + AutoPadding <= textBlock.ActualWidth)
            {
               
               textBlock.Padding = new Thickness(AutoPadding, 0, 0, 0);
               textBlock.Text = FullText;

            }
            else if (FullTextSize.Width + 5 <= textBlock.ActualWidth)
            {

               textBlock.Padding = new Thickness(textBlock.ActualWidth - (FullTextSize.Width + 5), 0, 0, 0);
               textBlock.Text = FullText;

            } else {

               textBlock.Padding = new Thickness(0, 0, 0, 0);

               //calculate average length of characters in the string
               double avgCharLength = FullTextSize.Width / FullText.Length;
               // length of added dots with a little extra buffer
               double dotsLength = avgCharLength * 3;

                // calculate how many characters need to be dropped to fit the string into the available space
               int nrDroppedChars = (int)Math.Ceiling(((FullTextSize.Width + dotsLength) - textBlock.ActualWidth) / avgCharLength);

               String text = "";

               do
               {
                   int newLength = FullText.Length - nrDroppedChars;

                   if (newLength <= 0)
                   {
                       text = "";
                       break;
                   }

                   text = FullText.Substring(0, newLength) + "...";

                   nrDroppedChars++;

               } while (MeasureString(text).Width >= textBlock.ActualWidth - errorMargin);

               textBlock.Text = text;
              

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
