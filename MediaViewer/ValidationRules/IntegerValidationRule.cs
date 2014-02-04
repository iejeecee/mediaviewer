using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MediaViewer.ValidationRules
{
    class IntegerValidationRule : ValidationRule
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
            int intValue = 0;

            if (value == null)
            {
                return new ValidationResult(true, null);
            }

            try
            {
                if (((string)value).Length > 0)
                    intValue = Int32.Parse((String)value);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            if ((intValue < Min) || (intValue > Max))
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
