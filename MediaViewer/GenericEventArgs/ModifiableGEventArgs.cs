using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.GenericEventArgs
{
    class ModifiableGEventArgs<Type> : EventArgs
    {
        private Type value;

        public ModifiableGEventArgs(Type value)
        {
            this.value = value;
        }

        public Type Value
        {
            set
            {
                this.value = value;
            }

            get
            {
                return value;
            }
        }
    }
}
