using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MediaViewer.ValidationRules
{
    class FloatValidationRule : ValidationRule
    {
        int min;
        int max;

        public int Min
        {
            get { return min; }
            set { min = value; }
        }

        public int Max
        {
            get { return max; }
            set { max = value; }
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            float floatValue = 0;

            if (value == null)
            {
                return new ValidationResult(true, null);
            }

            try
            {
                if (((string)value).Length > 0)
                    floatValue = float.Parse((String)value);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            if ((floatValue < Min) || (floatValue > Max))
            {
                return new ValidationResult(false,
                  "Please enter an value in the range: " + Min + " - " + Max + ".");
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
