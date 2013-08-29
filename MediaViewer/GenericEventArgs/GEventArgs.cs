using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.GenericEventArgs
{

    class GEventArgs<Type> : EventArgs
    {
        private Type value;

        public GEventArgs(Type value)
        {
            this.value = value;
        }

        public Type Value
        {
            get
            {

                return value;
            }
        }
    }

}
