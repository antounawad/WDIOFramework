using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Eulg.Client.SupportTool.Validators
{
    class HostOrIpValidator : ValidationRule
    {
        private static readonly Regex _domainNameRegex = new Regex(@"^[0-9a-z]([0-9a-z-_]*[0-9a-z])?(\.[0-9a-z]([0-9a-z-_]*[0-9a-z])?)*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var input = value as string;

            if(String.IsNullOrEmpty(input))
            {
                return new ValidationResult(true, String.Empty);
            }

            if(_domainNameRegex.IsMatch(input))
            {
                return new ValidationResult(true, String.Empty);
            }

            IPAddress result;
            if(IPAddress.TryParse(input, out result) && (input.Count(x => x == '.') == 3 || result.AddressFamily == AddressFamily.InterNetworkV6))
            {
                return new ValidationResult(true, String.Empty);
            }

            return new ValidationResult(false, "Bei dem angegebenen Wert muss es sich um einen Hostname oder eine IP-Adresse handeln!");              
        }
    }
}
