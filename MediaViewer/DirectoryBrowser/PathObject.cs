using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.DirectoryBrowser
{
    public class PathObject 
    {
        private ObservableCollection<PathObject> directories;
        public ObservableCollection<PathObject> Directories
        {
            get { return directories; }
            set { directories = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private PathObject parent;
        public PathObject Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        private string imageUrl;

        public string ImageUrl
        {
            get { return imageUrl; }
            set { imageUrl = value; }
        }

        private bool isExpanded;

        public bool IsExpanded
        {
            get { return isExpanded; }
            set { isExpanded = value; }
        }

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; }
        }

        public PathObject()
        {
            directories = new ObservableCollection<PathObject>();
            parent = null;
            IsExpanded = false;
            IsSelected = false;
        }        

        public string getFullPath() {

            string fullPath = Name;
            PathObject parent = Parent;

            while (parent != null)
            {
                fullPath = parent.Name + "\\" + fullPath;
                parent = parent.Parent;
            }

            return (fullPath);
        }

    
    }
}
