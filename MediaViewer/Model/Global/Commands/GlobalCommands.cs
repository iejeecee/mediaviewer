using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Global.Commands
{
    public static class GlobalCommands
    {
        public static CompositeCommand MetaDataUpdateCommand = new CompositeCommand();
        public static CompositeCommand SaveGlobalSettingsCommand = new CompositeCommand();

        //http://briannoyes.net/2009/09/05/supporting-graceful-shutdown-and-saving-on-close-from-a-wpf-prism-app/
        public static CompositeCommand ShutdownCommand = new CompositeCommand();
    }
}
