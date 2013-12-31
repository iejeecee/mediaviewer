using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

// a combobox that supports custom contextmenu's
namespace MediaViewer.MetaData
{
    class ExtendedComboBox : ComboBox
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Use Snoop to find the name of the TextBox part  
            // http://wpfmentor.blogspot.com/2008/11/understand-bubbling-and-tunnelling-in-5.html  
            TextBox textBox = (TextBox)Template.FindName("PART_EditableTextBox", this);

            // Create a template-binding in code  
            Binding binding = new Binding("ContextMenu");
            binding.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
            BindingOperations.SetBinding(textBox,
                FrameworkElement.ContextMenuProperty, binding);

        }
    }  
}
