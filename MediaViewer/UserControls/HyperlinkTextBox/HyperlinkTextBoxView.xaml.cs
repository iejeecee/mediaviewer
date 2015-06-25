using MediaViewer.Infrastructure.Logging;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaViewer.UserControls.HyperlinkTextBox
{
    /// <summary>
    /// Interaction logic for HyperlinkTextBoxView.xaml
    /// </summary>
    public partial class HyperlinkTextBoxView : UserControl
    {
        static HyperlinkTextBoxView()
        {
            BackgroundProperty.OverrideMetadata(typeof(HyperlinkTextBoxView), new FrameworkPropertyMetadata(Brushes.White,backgroundChanged));
        }

        public HyperlinkTextBoxView()
        {
            InitializeComponent();

            textEditor.Document = new ICSharpCode.AvalonEdit.Document.TextDocument();

            textEditor.TextChanged += textEditor_TextChanged;

            FileLinkElementGenerator fileLinkElementGenerator = new FileLinkElementGenerator();

            textEditor.TextArea.TextView.ElementGenerators.Add(fileLinkElementGenerator);
            textEditor.TextArea.TextView.AddHandler(Hyperlink.RequestNavigateEvent, (RoutedEventHandler)requestNavigateEvent);

            textEditor.WordWrap = false;           
            textEditor.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            textEditor.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }

        private void requestNavigateEvent(object sender, RoutedEventArgs e)
        {
            RequestNavigateEventArgs args = e as RequestNavigateEventArgs;

            Uri uri = args.Uri;

            if (uri.IsFile)          
            {
                String location = null;
               
                location = HttpUtility.UrlDecode(uri.AbsolutePath);

                if (MediaFormatConvert.isVideoFile(location))
                {
                    NameValueCollection values = HttpUtility.ParseQueryString(uri.Query);

                    String[] formats = { @"h'h'm'm's's'", @"m'm's's'", @"s's'"};

                    String timeOffsetString = values["t"];
                    TimeSpan time = new TimeSpan();

                    if (timeOffsetString != null)
                    {
                        try
                        {
                            bool success = TimeSpan.TryParseExact(timeOffsetString, formats, null, out time);
                        }
                        catch(Exception ex)
                        {
                            Logger.Log.Error("Error parsing timestring: " + timeOffsetString + " for " + location, ex);
                        }
                    }

                    MediaItem item = MediaItemFactory.create(location);
                    Shell.ShellViewModel.navigateToVideoView(item, (int)time.TotalSeconds);
                }

                e.Handled = true;
            }
           
        }

        private void textEditor_TextChanged(object sender, EventArgs e)
        {                      
            if (!String.Equals(Text, textEditor.Document.Text))
            {
                Text = textEditor.Document.Text;
            }          
            
        }     

        public String Text
        {
            get { return (String)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(String), typeof(HyperlinkTextBoxView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, textChanged));

        private static void textChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HyperlinkTextBoxView view = (HyperlinkTextBoxView)d;

            String newText = (String)e.NewValue;

            if (newText == null)
            {
                view.textEditor.Document.Text = "";
                return;
            }
            else
            {
                if (!String.Equals(newText, view.textEditor.Document.Text))
                {
                    view.textEditor.Document.Text = newText;
                }
            }

        }

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
            set { SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VerticalScrollBarVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
            DependencyProperty.Register("VerticalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(HyperlinkTextBoxView), new PropertyMetadata(ScrollBarVisibility.Hidden, verticalScrollbarVisibilityChanged));

        private static void verticalScrollbarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HyperlinkTextBoxView view = (HyperlinkTextBoxView)d;
            view.textEditor.VerticalScrollBarVisibility = (ScrollBarVisibility)e.NewValue;
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(HyperlinkTextBoxView), new PropertyMetadata(false, isReadOnlyChanged));

        private static void isReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HyperlinkTextBoxView view = (HyperlinkTextBoxView)d;
            view.textEditor.IsReadOnly = (bool)e.NewValue;
        }

        private static void backgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HyperlinkTextBoxView view = (HyperlinkTextBoxView)d;
            view.textEditor.Background = (Brush)e.NewValue;
        }

        public bool WordWrap
        {
            get { return (bool)GetValue(WordWrapProperty); }
            set { SetValue(WordWrapProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WordWrap.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WordWrapProperty =
            DependencyProperty.Register("WordWrap", typeof(bool), typeof(HyperlinkTextBoxView), new PropertyMetadata(false, wordWrapChanged));

        private static void wordWrapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HyperlinkTextBoxView view = (HyperlinkTextBoxView)d;
            view.textEditor.WordWrap = (bool)e.NewValue;
        }


        

    }
}
