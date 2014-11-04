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
    }
}
