using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Utils
{
    public class FileUtilsEventArgs : System.EventArgs
    {

        private string filePath;
        private bool isDirectory;

        public FileUtilsEventArgs(string filePath, bool isDirectory)
        {

            this.filePath = filePath;
            this.isDirectory = isDirectory;
        }

        public string FilePath
        {

            get { return (filePath); }
            set { filePath = value; }
        }

        public bool IsDirectory
        {

            get { return (isDirectory); }
            set { isDirectory = value; }
        }

    }
}
