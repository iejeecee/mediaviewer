using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Mvvm
{
    public abstract class CloseableBindableBase : BindableBase
    {
        public event EventHandler<DialogEventArgs> ClosingRequest;

        public enum DialogMode
        {
            SUBMIT,
            CANCEL
        }

        public class DialogEventArgs : EventArgs
        {
            DialogMode dialogMode;

            public DialogMode DialogMode
            {
                get { return dialogMode; }
                set { dialogMode = value; }
            }

            public DialogEventArgs(DialogMode dialogMode)
            {
                DialogMode = dialogMode;
            }
        }

        protected void OnClosingRequest()
        {
            if (this.ClosingRequest != null)
            {

                this.ClosingRequest(this, new DialogEventArgs(DialogMode.SUBMIT));
            }
        }

        protected void OnClosingRequest(DialogEventArgs e)
        {
            if (this.ClosingRequest != null)
            {

                this.ClosingRequest(this, e);
            }
        }
    }
}
