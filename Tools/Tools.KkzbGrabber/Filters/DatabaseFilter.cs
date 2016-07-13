using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Eulg.Utilities.Console;

namespace Tools.KkzbGrabber.Filters
{
    [Description("Match provider names against database")]
    class DatabaseFilter : IFilter
    {
        private readonly IList<Provider> _existing = new List<Provider>();

        private int _newProviders;
        private int _unchangedData;
        private int _rateUpdates;
        
        public DatabaseFilter()
        {
            const string QUERY = "SELECT [Name], [Rate] FROM dbo.HealthInsuranceProvider p JOIN dbo.HealthInsuranceSurcharge s ON s.Provider_ID = p.ID " +
                                 "WHERE s.ApplicableAfter = (SELECT MAX(si.ApplicableAfter) FROM dbo.HealthInsuranceSurcharge si WHERE si.Provider_ID = p.ID)";

            var connectionString = Prompt.SqlServerConnectionString("(LocalDb)\\v11.0", "eulg");

            using(var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using(var command = new SqlCommand(QUERY, connection))
                {
                    using(var reader = command.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            _existing.Add(new Provider(reader.GetString(0), GetRateFromDecimal(reader.GetDecimal(1))));
                        }
                    }
                }
            }
        }

        public bool Filter(ref Provider item)
        {
            var local = item;
            var existing = _existing.SingleOrDefault(p => RelaxedNamesEqual(p.Name, local.Name));

            // Item not found in database; add as new
            if (existing == null)
            {
                item.IsNew = true;
                ++_newProviders;
                return true;
            }

            // Once a match is confirmed, keep using the name from the database
            item = new Provider(existing.Name, item.Rate);

            // Item found and value hasn't changed; remove
            if (existing.Rate == item.Rate)
            {
                ++_unchangedData;
                return false;
            }

            // Updated value
            ++_rateUpdates;
            item.IsNew = false;
            return true;
        }

        public void ShowAndResetCounters(TextWriter output)
        {
            output.WriteLine("{0} records acquired from store", _existing.Count);
            output.WriteLine("{0} new providers, {1} updated rates, {2} orphans", _newProviders, _rateUpdates, _existing.Count - (_rateUpdates + _unchangedData));
            _newProviders = 0;
            _rateUpdates = 0;
            _unchangedData = 0;
        }

        private static Rate GetRateFromDecimal(decimal value)
        {
            var exponent = 0;
            while (Math.Truncate(value) != value)
            {
                value *= 10;
                --exponent;
            }

            return new Rate((int)Math.Truncate(value), exponent);
        }

        private static bool RelaxedNamesEqual(string name1, string name2)
        {
            var trimmed1 = name1;
            var trimmed2 = name2;
            var isBkk1 = RemoveBkkArtifact(ref trimmed1);
            var isBkk2 = RemoveBkkArtifact(ref trimmed2);

            if (string.Equals(NormalizeForComparison(trimmed1), NormalizeForComparison(trimmed2), StringComparison.OrdinalIgnoreCase) && (isBkk1 == isBkk2))
            {
                return true;
            }

            if (trimmed1.StartsWith(trimmed2, StringComparison.OrdinalIgnoreCase) || trimmed2.StartsWith(trimmed1, StringComparison.OrdinalIgnoreCase))
            {
                if ((trimmed1.Length < trimmed2.Length && !char.IsLetterOrDigit(trimmed2[trimmed1.Length]))
                    || trimmed2.Length < trimmed1.Length && !char.IsLetterOrDigit(trimmed1[trimmed2.Length]))
                {
                    if (Prompt.YesNo($"The names '{name1}' and '{name2}' seem similar. Do they refer to the same insurance provider?"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool RemoveBkkArtifact(ref string name)
        {
            var artifacts = new [] {"bkk", "betriebskrankenkasse" };

            foreach (var artifact in artifacts)
            {
                var index = name.IndexOf(artifact, StringComparison.OrdinalIgnoreCase);
                if(index >= 0)
                {
                    name = name.Substring(0, index) + name.Substring(index + artifact.Length);
                    return true;
                }
            }

            return false;
        }

        private static string NormalizeForComparison(string name)
        {
            return new string(name.Where(c => !char.IsWhiteSpace(c) && !char.IsPunctuation(c)).ToArray());
        }
    }
}
