using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PN.Timesheet.Component
{
    public class NumberButton : System.Windows.Controls.Button
    {
        public bool ActiveNumber {
            get { return (bool)GetValue(ActiveNumberProperty); }
            set
            {
                SetValue(ActiveNumberProperty, value);
            }
        }
        public int Number
        {
            get { return (int)GetValue(NumberProperty); }
            set
            {
                SetValue(NumberProperty, value);
            }
        }

        public static DependencyProperty NumberProperty = DependencyProperty.Register("Number", typeof(Int32), typeof(NumberButton), new FrameworkPropertyMetadata(0));
        public static DependencyProperty ActiveNumberProperty = DependencyProperty.Register("ActiveNumber", typeof(Boolean), typeof(NumberButton), new FrameworkPropertyMetadata(false));
    }
}
