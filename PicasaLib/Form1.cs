using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;

namespace PicasaLib
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            PicasaService service = new PicasaService(null);

            entry meta = new entry();
            meta.title = "hogfarm woo hoo";
            meta.summary = "boowoo";

            meta.category = new entryCategory();
            meta.category.scheme = "http://schemas.google.com/g/2005#kind";
            meta.category.term = "http://schemas.google.com/photos/2007#photo";

            XmlSerializer s = new XmlSerializer(typeof(entry));
            StringWriter textWriter = new StringWriter();

            s.Serialize(textWriter, meta);

            String output = textWriter.ToString();

            //textWriter.

            int i = 0;
           // service.test();
        }
    }
}
