using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.MetaData
{
    class FileMetaDataFactory
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static FileMetaData read(string location, CancellationToken token)
        {
            FileMetaData metaData = null;

            if(token.IsCancellationRequested) return(metaData);

            try
            {
                metaData = new FileMetaData();

                metaData.loadFromDisk(location);
            }
            catch (Exception e)
            {
                log.Warn("Error reading metadata for: " + location, e);

                if (metaData != null)
                {
                    metaData.closeFile();
                }

                return (null);
            }

            return (metaData);
        }

        public static async Task<FileMetaData> readAsync(string location, CancellationToken token)
        {

            return await Task<FileMetaData>.Run(() => read(location, token), token);

        }
    }
}
