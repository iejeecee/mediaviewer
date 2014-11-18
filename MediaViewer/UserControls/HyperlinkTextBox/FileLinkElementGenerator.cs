using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.HyperlinkTextBox
{
    class FileLinkElementGenerator : LinkElementGenerator
    {
  
        static Regex fileLinkRegex = new Regex(@"\bfile://[\w\d\._/\-~%@()+:?&=#!,']*[\w\d/]");

        static Regex fileLinkRegex2 = new Regex(@"\bfile://[^" + Regex.Escape(new String(System.IO.Path.GetInvalidPathChars())) + @"]*[\w\d/]");

        public FileLinkElementGenerator() :
            base(fileLinkRegex2)
        {
          
        }

        protected override VisualLineElement ConstructElementFromMatch(Match m)
        {
            Uri uri = GetUriFromMatch(m);
            if (uri == null)
                return null;
            VisualLineLinkText linkText = new VisualLineLinkText(CurrentContext.VisualLine, m.Length);    
            linkText.NavigateUri = uri;
            linkText.RequireControlModifierForClick = this.RequireControlModifierForClick;
           
            return linkText;
        }
    
        protected override Uri GetUriFromMatch(Match match)
        {
            string targetUrl = match.Value;

            Uri targetUri;

            bool valid = Uri.TryCreate(targetUrl, UriKind.Absolute, out targetUri);

            if (valid)
            {
                return (targetUri);
            }
            else
            {
                return null;
            }
        }
    }
}
