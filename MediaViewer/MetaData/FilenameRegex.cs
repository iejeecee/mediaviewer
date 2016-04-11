using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MediaViewer.MetaData
{
    [Serializable]
    public class FilenameRegex : BindableBase 
    {
        // xml serialization doesn't preserve whitespace only strings
        [XmlAttribute("space", Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string Space = "preserve";

        public FilenameRegex()
        {
            Regex = "";
            Replace = "";
        }

        String regex;
     
        public String Regex
        {
            get { return regex; }
            set
            {
                String newValue = value;

                if (newValue != null)
                {
                    int newLine = value.IndexOf('\n');
                    if (newLine != -1)
                    {
                        newValue = newValue.Substring(0, newLine);
                    }

                }

                SetProperty(ref regex, value);                       
            }
        }

        String replace;

        public String Replace
        {
            get { return replace; }
            set
            {
                String newValue = value;

                if (newValue != null)
                {
                    int newLine = value.IndexOf('\n');
                    if (newLine != -1)
                    {
                        newValue = newValue.Substring(0, newLine);
                    }
                }

                SetProperty(ref replace, newValue);                           
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (String.IsNullOrEmpty(Regex));
            }
        }
    }
}
