using System;
using System.Globalization;
using System.Windows.Controls;

namespace Eulg.Client.SupportTool.Validators
{
    public class UInt16Validator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            UInt16 result;

            if (value != null && !String.IsNullOrEmpty(value.ToString()))
            {
                if (UInt16.TryParse(value.ToString(), out result))
                {
                    return new ValidationResult(true, String.Empty);
                }

                return new ValidationResult(false, "Bei dem angegebenen Wert muss es sich um eine Ganzzahl handeln!");
            }

            return new ValidationResult(true, String.Empty);
        }
    }
}
