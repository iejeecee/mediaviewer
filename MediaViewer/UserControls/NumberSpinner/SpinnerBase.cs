using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaViewer.UserControls.NumberSpinner
{
    public abstract class SpinnerBase<T> : UserControl where T : struct, System.IComparable<T> 
    {
        static VideoPlayerControl.Timers.DefaultTimer timer;
        const int initialRepeatDelayMS = 800;
        const int repeatDelayMS = 50;

        static SpinnerBase()
        {
            timer = new VideoPlayerControl.Timers.DefaultTimer();
            timer.Tick += timer_Tick;
            timer.AutoReset = true;
        }

        Button upButtonBase;        
        Button downButtonBase;
        TextBox valueTextBoxBase;
     
        protected SpinnerBase()
        {
                                         
        }

        protected void initializeElems(Button upButton, Button downButton, TextBox valueTextBox)
        {
            upButtonBase = upButton;
            downButtonBase = downButton;
            valueTextBoxBase = valueTextBox;

            var descriptor = DependencyPropertyDescriptor.FromProperty(Button.IsPressedProperty, typeof(Button));
            descriptor.AddValueChanged(upButtonBase, new EventHandler(button_IsPressedChanged));

            descriptor.AddValueChanged(downButtonBase, new EventHandler(button_IsPressedChanged));
        }

        private void button_IsPressedChanged(object sender, EventArgs e)
        {
            if (downButtonBase.IsPressed == true)
            {
                subtractValue();
                timer.Interval = initialRepeatDelayMS;
                timer.Tag = this;
                timer.start();
            } 
            else if(upButtonBase.IsPressed == true)
            {
                addValue();
                timer.Interval = initialRepeatDelayMS;
                timer.Tag = this;
                timer.start();
            }
            else
            {
                timer.stop();
            }
        }

        static void timer_Tick(Object sender, EventArgs e)
        {
           SpinnerBase<T> spinner = (SpinnerBase<T>)(sender as VideoPlayerControl.Timers.DefaultTimer).Tag;

           spinner.Dispatcher.BeginInvoke(new Action(() =>
           {
               if (spinner.downButtonBase.IsPressed == true)
               {
                   spinner.subtractValue();
               }
               else if (spinner.upButtonBase.IsPressed == true)
               {
                   spinner.addValue();
               }
           }));

           timer.Interval = repeatDelayMS;
        }
        
        public Nullable<T> Value
        {
            get { return (Nullable<T>)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(Nullable<T>), typeof(SpinnerBase<T>),
            new FrameworkPropertyMetadata()
            {
                DefaultValue = null,
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = new PropertyChangedCallback(valueChangedCallback),
                CoerceValueCallback = new CoerceValueCallback(coerceValueCallback),
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged                      
            });

        private static Nullable<T> coerceValue(Nullable<T> val, Nullable<T> max, Nullable<T> min)
        {
            if (val == null)
            {
                return (val);
            }

            if (max != null)
            {
                if (val.Value.CompareTo(max.Value) > 0)
                {
                    val = max;
                }               
            }

            if (min != null)
            {
                if (val.Value.CompareTo(min.Value) < 0)
                {
                    val = min;
                }                
            }

            return (val);
        }

        private static object coerceValueCallback(DependencyObject d, object baseValue)
        {
            Nullable<T> max = (Nullable<T>)d.GetValue(MaxProperty);
            Nullable<T> min = (Nullable<T>)d.GetValue(MinProperty);

            Nullable<T> val = (Nullable<T>)baseValue;
            
            Nullable<T> result = coerceValue(val, max, min);

            if(!result.Equals(val)) {
                // Microsoft have decided that coercing a value happens AFTER updating the binding                
                // So in order to not fuck up our datamodel we have to explicitly set our binding
                // again, to it's correct value in case it's was coerced.
                d.SetValue(ValueProperty, result);
            }

            return (result);
        }

        protected virtual void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SpinnerBase<T> control = (SpinnerBase<T>)d;

            Nullable<T> value = (Nullable<T>)e.NewValue;

            control.valueTextBoxBase.Text = value == null ? "" : value.Value.ToString();   
        }

        private static void valueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SpinnerBase<T> control = (SpinnerBase<T>)d;

            control.OnValueChanged(d, e);
        }


        public Nullable<T> Max
        {
            get { return (Nullable<T>)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Max.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register("Max", typeof(Nullable<T>), typeof(SpinnerBase<T>), new PropertyMetadata(null, new PropertyChangedCallback(maxChangedCallback)));

        private static void maxChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SpinnerBase<T> control = (SpinnerBase<T>)d;
            Nullable<T> max = (Nullable<T>)e.NewValue;

            control.Value = coerceValue(control.Value, max, control.Min);
            
        }

        public Nullable<T> Min
        {
            get { return (Nullable<T>)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Min.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register("Min", typeof(Nullable<T>), typeof(SpinnerBase<T>), new PropertyMetadata(null, new PropertyChangedCallback(minChangedCallback)));

        private static void minChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SpinnerBase<T> control = (SpinnerBase<T>)d;
            Nullable<T> min = (Nullable<T>)e.NewValue;

            control.Value = coerceValue(control.Value, control.Max, min);
        }

        protected abstract void addValue();


        protected abstract void subtractValue();


        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(SpinnerBase<T>), new PropertyMetadata(TextAlignment.Right, textAlignment_PropertyChangedCallback));

        private static void textAlignment_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SpinnerBase<T> control = (SpinnerBase<T>)d;
            control.valueTextBoxBase.TextAlignment = (TextAlignment)e.NewValue;           
        }

    }
}
