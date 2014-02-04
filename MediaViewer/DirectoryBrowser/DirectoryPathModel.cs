using MediaViewer.Import;
using MediaViewer.MediaDatabase.DbCommands;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.DirectoryBrowser
{
    class DirectoryPathModel : PathModel
    {
       
        public DirectoryPathModel(PathModel parent, DirectoryInfo info) : base(info.Name)         
        {
            Parent = parent;       
          
            MediaDbCommands mediaCommand = new MediaDbCommands();

            NrImportedFiles = mediaCommand.getNrMediaInLocation(getFullPath());

            if (NrImportedFiles > 0)
            {
                ImageUrl = "pack://application:,,,/Resources/Icons/mediafolder.ico";
            }
            else
            {
                ImageUrl = "pack://application:,,,/Resources/Icons/Folder_Open.ico";
            }
        }

        protected override void importStateChanged(object sender, NotifyCollectionChangedEventArgs e) 
        {
            base.importStateChanged(sender, e);

            if (NrImportedFiles > 0)
            {
                ImageUrl = "pack://application:,,,/Resources/Icons/mediafolder.ico";
            }
            else
            {
                ImageUrl = "pack://application:,,,/Resources/Icons/Folder_Open.ico";
            }
        }
    }
}
