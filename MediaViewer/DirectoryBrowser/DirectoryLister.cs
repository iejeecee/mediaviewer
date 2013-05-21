using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using MediaViewer.Utils;

namespace MediaViewer.DirectoryBrowser
{

    class DirectoryLister
    {
        
        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public List<DriveObject> Drives
        {
            get { return updateDrives(); }           
        }

        private static DriveInfo[] getDrives()
        {
            DriveInfo[] drivesInfo = new DriveInfo[0];
            try
            {
                drivesInfo = DriveInfo.GetDrives();
            }
            catch (Exception e)
            {
                log.Warn("Cannot read system drives: ", e);
            }

            return(drivesInfo);
        }

        private List<DriveObject> updateDrives()
        {

            List<DriveObject> drives = new List<DriveObject>();

            DriveInfo[] drivesArray = DriveInfo.GetDrives();

            foreach (DriveInfo driveInfo in drivesArray)
            {
                DriveObject drive = new DriveObject(driveInfo);

                LoadingObject dummy = new LoadingObject(drive);
                drive.Directories.Add(dummy);

                drives.Add(drive);
            }

            return (drives);
        }

        public static DirectoryInfo[] getSubDirectories(string fullPath)
        {

            DirectoryInfo dirInfo = new DirectoryInfo(fullPath);

            DirectoryInfo[] subDirsInfo = new DirectoryInfo[0];
            try
            {                     
                subDirsInfo = dirInfo.GetDirectories();
            }
            catch (Exception e)
            {
                log.Warn("Cannot access directory: " + dirInfo.FullName, e);
            }

            return (subDirsInfo);
        }

       
    }
}
