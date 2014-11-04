using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Global.Events
{   
    public class TitleChangedEvent : PubSubEvent<String> { }
    public class ViewMediaEvent : PubSubEvent<String> { }
    public class ToggleFullScreenEvent : PubSubEvent<bool> { }
   
   
}
