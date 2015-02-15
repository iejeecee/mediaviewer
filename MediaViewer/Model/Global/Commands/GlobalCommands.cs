using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Infrastructure.Global.Commands
{
    public static class GlobalCommands
    {
        public static CompositeCommand MetaDataUpdateCommand = new CompositeCommand();
        public static CompositeCommand SaveGlobalSettingsCommand = new CompositeCommand();
    }
}
