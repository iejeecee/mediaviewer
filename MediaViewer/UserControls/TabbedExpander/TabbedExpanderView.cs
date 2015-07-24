using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaViewer.UserControls.TabbedExpander
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:MediaViewer.UserControls.TabbedExpanderView"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:MediaViewer.UserControls.TabbedExpanderView;assembly=MediaViewer.UserControls.TabbedExpanderView"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:TabbedExpanderView/>
    ///
    /// </summary>
    [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)] // cannot be read & localized as string    
    public class TabbedExpanderView : ItemsControl
    {
        //-------------------------------------------------------------------
        //
        //  Constructors
        //
        //-------------------------------------------------------------------
 
        #region Constructors

        static TabbedExpanderView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TabbedExpanderView), new FrameworkPropertyMetadata(typeof(TabbedExpanderView)));
            //_dType = DependencyObjectType.FromSystemTypeInternal(typeof(TabbedExpanderView));

            IsTabStopProperty.OverrideMetadata(typeof(TabbedExpanderView), new FrameworkPropertyMetadata(false));

            //IsMouseOverPropertyKey.OverrideMetadata(typeof(TabbedExpanderView), new UIPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));
            //IsEnabledProperty.OverrideMetadata(typeof(TabbedExpanderView), new UIPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));
        }

        public TabbedExpanderView()
        {
            Guid = System.Guid.NewGuid().ToString();
        }


        #endregion
 
        //-------------------------------------------------------------------
        //
        //  Public Properties
        //
        //-------------------------------------------------------------------
 
        #region Public Properties
 
        /// <summary>
        /// ExpandDirection specifies to which direction the content will expand
        /// </summary>
        [Bindable(true), Category("Behavior")]
        public ExpandDirection ExpandDirection
        {
            get { return (ExpandDirection) GetValue(ExpandDirectionProperty); }
            set { SetValue(ExpandDirectionProperty, value); }
        }
 
        /// <summary>
        /// The DependencyProperty for the ExpandDirection property.
        /// Default Value: ExpandDirection.Down
        /// </summary>
        public static readonly DependencyProperty ExpandDirectionProperty =
                DependencyProperty.Register(
                        "ExpandDirection",
                        typeof(ExpandDirection),
                        typeof(TabbedExpanderView),
                        new FrameworkPropertyMetadata(
                                ExpandDirection.Down /* default value */,
                                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault),
                        new ValidateValueCallback(IsValidExpandDirection));
 
        private static bool IsValidExpandDirection(object o)
        {
            ExpandDirection value = (ExpandDirection)o;
 
            return (value == ExpandDirection.Down ||
                    value == ExpandDirection.Left ||
                    value == ExpandDirection.Right ||
                    value == ExpandDirection.Up);
        }
 
        /// <summary>
        ///     The DependencyProperty for the IsExpanded property.
        ///     Default Value: false
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty =
                DependencyProperty.Register(
                        "IsExpanded",
                        typeof(bool),
                        typeof(TabbedExpanderView),
                        new FrameworkPropertyMetadata(
                                false /* default value */,
                                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                                new PropertyChangedCallback(OnIsExpandedChanged)));
 
        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TabbedExpanderView ep = (TabbedExpanderView)d;
            
            bool newValue = (bool) e.NewValue;
 
            // Fire accessibility event
            /*ExpanderAutomationPeer peer = UIElementAutomationPeer.FromElement(ep) as ExpanderAutomationPeer;
            if(peer != null)
            {
                peer.RaiseExpandCollapseAutomationEvent(!newValue, newValue);
            }*/
 
            if (newValue)
            {
                ep.OnExpanded();
            }
            else
            {
                ep.OnCollapsed();
            }
                    
            //ep.UpdateVisualState();
        }
 
        /// <summary>
        /// IsExpanded indicates whether the expander is currently expanded.
        /// </summary>
        [Bindable(true), Category("Appearance")]
        public bool IsExpanded
        {
            get { return (bool) GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public string Guid
        {
            get { return (string)GetValue(GuidProperty); }
            set { SetValue(GuidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Guid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GuidProperty =
            DependencyProperty.Register("Guid", typeof(string), typeof(TabbedExpanderView), new PropertyMetadata(null));

        
        /// <summary>
        /// Expanded event.
        /// </summary>
        public static readonly RoutedEvent ExpandedEvent =
            EventManager.RegisterRoutedEvent("Expanded",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(TabbedExpanderView)
            );
 
        /// <summary>
        /// Expanded event. It is fired when IsExpanded changed from false to true.
        /// </summary>
        public event RoutedEventHandler Expanded
        {
            add { AddHandler(ExpandedEvent, value); }
            remove { RemoveHandler(ExpandedEvent, value); }
        }
 
        /// <summary>
        /// Collapsed event.
        /// </summary>
        public static readonly RoutedEvent CollapsedEvent =
            EventManager.RegisterRoutedEvent("Collapsed",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(TabbedExpanderView)
            );
 
        /// <summary>
        /// Collapsed event. It is fired when IsExpanded changed from true to false.
        /// </summary>
        public event RoutedEventHandler Collapsed
        {
            add { AddHandler(CollapsedEvent, value); }
            remove { RemoveHandler(CollapsedEvent, value); }
        }
 
        #endregion
 
        //-------------------------------------------------------------------
        //
        //  Protected Methods
        //
        //-------------------------------------------------------------------
 
        #region Protected Methods
 
        /*internal override void ChangeVisualState(bool useTransitions)
        {
            // Handle the Common states
            if (!IsEnabled)
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateDisabled, VisualStates.StateNormal);
            }
            else if (IsMouseOver)
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateMouseOver, VisualStates.StateNormal);
            }
            else
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateNormal);
            }
 
            // Handle the Focused states
            if (IsKeyboardFocused)
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateFocused, VisualStates.StateUnfocused);
            }
            else
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateUnfocused);
            }
        
            if (IsExpanded)
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateExpanded);
            }
            else
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateCollapsed);
            }
 
            switch (ExpandDirection)
            {
                case ExpandDirection.Down:
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateExpandDown); 
                    break;
                
                case ExpandDirection.Up:
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateExpandUp); 
                    break;
 
                case ExpandDirection.Left:
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateExpandLeft); 
                    break;
                
                default:
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateExpandRight); 
                    break;
            }
            
            base.ChangeVisualState(useTransitions);
        }*/
 
        /// <summary>
        /// A virtual function that is called when the IsExpanded property is changed to true. 
        /// Default behavior is to raise an ExpandedEvent.
        /// </summary>
        protected virtual void OnExpanded()
        {                      
            RaiseEvent(new RoutedEventArgs(TabbedExpanderView.ExpandedEvent, this));         
        }
 
        /// <summary>
        /// A virtual function that is called when the IsExpanded property is changed to false. 
        /// Default behavior is to raise a CollapsedEvent.
        /// </summary>
        protected virtual void OnCollapsed()
        {
            RaiseEvent(new RoutedEventArgs(TabbedExpanderView.CollapsedEvent, this));
        }
 
        #endregion
        
        #region Accessibility
 
        /// <summary>
        /// Creates AutomationPeer (<see cref="UIElement.OnCreateAutomationPeer"/>)
        /// </summary>
        /*protected override AutomationPeer OnCreateAutomationPeer() 
        {
            return new ExpanderAutomationPeer(this);
        }*/
 
        #endregion
 
 
        //-------------------------------------------------------------------
        //
        //  Private Fields
        //
        //-------------------------------------------------------------------
        
        #region Private Fields
 
        #endregion
 
        #region DTypeThemeStyleKey
 
        // Returns the DependencyObjectType for the registered ThemeStyleKey's default 
        // value. Controls will override this method to return approriate types.
        /*public override DependencyObjectType DTypeThemeStyleKey
        {
            get { return _dType; }
        }
 
        private static DependencyObjectType _dType;
 
         */ 
        #endregion DTypeThemeStyleKey
    }
}
