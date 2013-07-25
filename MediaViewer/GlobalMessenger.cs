using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer
{
    class GlobalMessenger
    {
        static Messenger globalMessenger;

        static GlobalMessenger() {

            globalMessenger = new Messenger();
        }

        public static Messenger Instance
        {
            get
            {
                return (globalMessenger);
            }
        }
    }
}
