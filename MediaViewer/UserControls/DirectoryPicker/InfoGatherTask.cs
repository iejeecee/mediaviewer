using MediaViewer.MediaDatabase.DbCommands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.DirectoryPicker
{
    public class InfoGatherTask
    {
        CancellationToken token;
        BlockingCollection<Location> locationQueue;

        public InfoGatherTask(CancellationToken token)
        {
            this.token = token;
            locationQueue = new BlockingCollection<Location>();

            Task.Factory.StartNew(infoGatherLoop, token);
           
        }
        
        public void addLocation(Location location)
        {
            locationQueue.Add(location);
        }

        void infoGatherLoop()
        {
            while (token.IsCancellationRequested == false)
            {
                Location location = locationQueue.Take(token);

                using (MediaDbCommands mediaCommand = new MediaDbCommands())
                {
                    location.NrImported = mediaCommand.getNrMediaInLocation(location.FullName);
                }
            }
          
        }

    }
}
