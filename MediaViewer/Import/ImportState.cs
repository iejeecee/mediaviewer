using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Progress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MediaViewer.Import
{
   
    class ImportState
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static ImportState instance;
        public event EventHandler<ImportStateEventArgs> ImportStateChanged;

        public static ImportState Instance
        {
            get { return ImportState.instance; }           
        }

        static ImportState()
        {
            instance = new ImportState();
        }

        protected ImportState()
        {

        }

        public void import(MediaFileItem item, MediaDatabaseContext ctx = null)
        {
            MediaDbCommands mediaCommands = new MediaDbCommands(ctx);

            Media media = new Media(item);

            mediaCommands.createMedia(media);

            if (ImportStateChanged != null)
            {
                ImportStateChanged(this, new ImportStateEventArgs(Mode.ADDED, item));
            }
        }

       
    }

    public enum Mode
    {
        ADDED,
        REMOVED
    }

    public class ImportStateEventArgs : EventArgs
    {
        Mode mode;

        public Mode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        MediaFileItem item;

        public MediaFileItem Item
        {
            get { return item; }
            set { item = value; }
        }

        public ImportStateEventArgs(Mode mode, MediaFileItem item)
        {
            this.mode = mode;
            this.item = item;
        }
    }
}
