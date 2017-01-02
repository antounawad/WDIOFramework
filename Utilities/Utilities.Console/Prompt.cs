using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Eulg.Utilities.Console
{
    public static class Prompt
    {
        private static void LeadIn()
        {
        }

        private static void LeadOut()
        {
            System.Console.WriteLine();
        }

        public static T Show<T>(string prompt, Func<string, Tuple<T, bool>> parser, bool inline = false)
        {
            LeadIn();

            Tuple<T, bool> result;
            do
            {
                System.Console.Write(prompt);
                if (inline)
                {
                    System.Console.Write(' ');
                }
                else
                {
                    System.Console.WriteLine();
                    System.Console.Write("> ");
                }

                try
                {
                    result = parser(System.Console.ReadLine().Trim());
                }
                catch
                {
                    result = Tuple.Create(default(T), false);
                }
            }
            while (!result.Item2);

            LeadOut();
            return result.Item1;
        }

        public static T Show<T>(string prompt, Func<string, Tuple<T, bool>> parser, T defaultValue, bool inline = false)
        {
            return Show(prompt, s => string.IsNullOrWhiteSpace(s) ? Tuple.Create(defaultValue, true) : parser(s), inline);
        }

        /// <summary>
        /// Prompts for string value
        /// </summary>
        /// <param name="prompt">Prompt message</param>
        /// <param name="blank">default value for "blank" answer</param>
        /// <param name="inline">if all should be printed in one line</param>
        /// <returns></returns>
        public static string String(string prompt, string blank = null, bool inline = false)
        {
            return blank == null
                       ? Show(prompt, s => Tuple.Create(s, !string.IsNullOrWhiteSpace(s)), inline)
                       : Show(prompt, s => Tuple.Create(s, !string.IsNullOrWhiteSpace(s)), blank, inline);
        }

        public static string Regex(string prompt, string pattern, string blank = null, RegexOptions options = RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.IgnoreCase, bool inline = false)
        {
            return blank == null
                       ? Show(prompt, s => Tuple.Create(s, System.Text.RegularExpressions.Regex.IsMatch(s, pattern, options)), inline)
                       : Show(prompt, s => Tuple.Create(s, System.Text.RegularExpressions.Regex.IsMatch(s, pattern, options)), blank, inline);
        }

        public static string File(string prompt, bool mustExist = false, bool directoryMustExist = true, bool inline = false)
        {
            return Show(prompt, s =>
            {
                var fn = Path.GetFullPath(s.Trim());
                return Tuple.Create(fn, !string.IsNullOrWhiteSpace(fn)
                                        && (!mustExist || System.IO.File.Exists(fn))
                                        && (!directoryMustExist || Directory.Exists(Path.GetDirectoryName(fn)))
                );
            }, inline);
        }

        public static string FileName(string prompt, string defaultName = null, string forceExtension = null)
        {
            var illegalChars = new List<char> { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            illegalChars.AddRange(Path.GetInvalidFileNameChars());

            while(true)
            {
                LeadIn();
                System.Console.Write(prompt);
                System.Console.Write(" ");

                var input = FilteredReadString(null, c => !illegalChars.Contains(c));
                if (string.IsNullOrEmpty(input))
                {
                    if (string.IsNullOrEmpty(defaultName))
                    {
                        continue;
                    }

                    input = defaultName;
                }

                if (forceExtension != null)
                {
                    input = Path.ChangeExtension(input, forceExtension);
                }

                return input;
            }
        }

        public static int Integer(string prompt, int? blank = null, int min = int.MinValue, int max = int.MaxValue, NumberStyles style = NumberStyles.Integer, IFormatProvider culture = null)
        {
            Func<string, Tuple<int, bool>> parser = delegate(string s)
            {
                var i = int.Parse(s, style, culture);
                return Tuple.Create(i, min <= i && i <= max);
            };
            return blank == null
                       ? Show(prompt, parser, true)
                       : Show(prompt, parser, blank.Value, true);
        }

        public static T Enum<T>(string prompt, T? blank = null, bool inline = false, bool suppressDescription = false) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T");
            }

            var type = typeof(T);
            var values = System.Enum.GetValues(type).Cast<T>().ToArray();
            var displayValues = new Dictionary<T, KeyValuePair<string, string>>();
            foreach (var value in values)
            {
                var name = value.ToString();
                var desc = ((DescriptionAttribute)type.GetField(name).GetCustomAttribute(typeof(DescriptionAttribute)))?.Description;
                displayValues.Add(value, new KeyValuePair<string, string>(name, desc == null || suppressDescription ? string.Empty : desc));
            }

            return Select(prompt, values, x => displayValues[x], blank ?? default(T), inline, blank != null);
        }

        public static T Select<T>(string prompt, T[] items, T blank = default(T), bool inline = false, bool allowBlank = false)
        {
            return Select(prompt, items, x => x.ToString(), blank, inline, allowBlank);
        }

        public static T Select<T>(string prompt, T[] items, Func<T, string> formatter, T blank = default(T), bool inline = false, bool allowBlank = false)
        {
            return Select(prompt, items, x => new KeyValuePair<string, string>(formatter(x), string.Empty), blank, inline, allowBlank);
        }

        public static T Select<T>(string prompt, T[] items, Func<T, KeyValuePair<string, string>> formatter, T blank = default(T), bool inline = false, bool allowBlank = false)
        {
            if (inline)
            {
                System.Console.WriteLine("{0} ({1})", prompt,
                                  string.Join(" ", items.Select((i, n) => $"{n+1}/{formatter(i).Key}")));
            }
            else
            {
                System.Console.WriteLine(prompt);

                var digits = (int)Math.Ceiling(Math.Log10(items.Length));
                var maxKeyLength = items.Max(x => formatter(x).Key.Length);

                var indentation = (digits + 3) + (maxKeyLength + 2);
                var maxDescLineLength = System.Console.WindowWidth - indentation;

                var n = 0;
                foreach (var item in items)
                {
                    var formatted = formatter(item);
                    System.Console.Write("[{0}] {1}  ", (++n).ToString().PadLeft(digits), formatted.Key.PadRight(maxKeyLength));

                    var lines = new List<string>();
                    var desc = formatted.Value ?? string.Empty;
                    while (desc.Length > 0)
                    {
                        if (desc.Length <= maxDescLineLength)
                        {
                            lines.Add(desc);
                            break;
                        }

                        var lineBreak = desc.Substring(0, Math.Min(desc.Length, maxDescLineLength)).IndexOf(Environment.NewLine, StringComparison.Ordinal);
                        var whitespace = desc.Substring(0, Math.Min(desc.Length, maxDescLineLength)).Select((c, i) => new KeyValuePair<int, char>(i + 1, c)).LastOrDefault(x => char.IsWhiteSpace(x.Value)).Key - 1;

                        if (lineBreak >= 0)
                        {
                            lines.Add(desc.Substring(0, lineBreak));
                            desc = desc.Substring(lineBreak + Environment.NewLine.Length);
                        }
                        else if (whitespace >= 0)
                        {
                            lines.Add(desc.Substring(0, whitespace));
                            desc = desc.Substring(whitespace);
                            while (char.IsWhiteSpace(desc[0]))
                            {
                                desc = desc.Substring(1);
                            }
                        }
                    }

                    if (lines.Count > 0)
                    {
                        System.Console.WriteLine(lines[0]);
                        foreach (var line in lines.Skip(1))
                        {
                            System.Console.WriteLine(string.Empty.PadRight(indentation) + line);
                        }
                    }
                    else
                    {
                        System.Console.WriteLine();
                    }
                }
            }

            Func<string, Tuple<int, bool>> parser = input =>
            {
                var idx = Enumerable.Range(0, items.Length).Cast<int?>().SingleOrDefault(i => formatter(items[i.Value]).Key.Equals(input, StringComparison.OrdinalIgnoreCase)) ?? (int.Parse(input) - 1);
                return Tuple.Create(idx, idx >= 0 && idx < items.Length);
            };

            var index = !allowBlank
                ? Show(">", parser, true)
                : Show(">", parser, -1, true);

            return index == -1 ? blank : items[index];
        }

        public static T Keystroke<T>(string prompt, Func<char, Tuple<T, bool>> parser)
        {
            LeadIn();

            System.Console.Write(prompt);
            System.Console.Write(' ');

            Tuple<T, bool> result;
            char lastChar;
            do
            {
                try
                {
                    result = parser(lastChar = System.Console.ReadKey(true).KeyChar);
                }
                catch
                {
                    lastChar = default(char);
                    result = Tuple.Create(default(T), false);
                }
            }
            while (!result.Item2);

            System.Console.WriteLine(lastChar);
            LeadOut();

            return result.Item1;
        }

        public static bool YesNo(string prompt)
        {
            return Choice(prompt, new KeyValuePair<char, bool>('y', true), new KeyValuePair<char, bool>('n', false));
        }

        public static T Choice<T>(string prompt, params KeyValuePair<char, T>[] choices)
        {
            var choicesString = " (" + string.Join("/", choices.Select(x => x.Key.ToString())) + ")";
            return Keystroke(prompt + choicesString, x => Tuple.Create(choices.Single(c => char.ToLowerInvariant(c.Key) == x).Value, true));
        }

        public static void Wait(string prompt = "Press any key to continue")
        {
            Keystroke(prompt, _ => Tuple.Create<object, bool>(null, true));
        }

        public static string Password(string prompt = "Password: ")
        {
            LeadIn();
            System.Console.Write(prompt);
            return FilteredReadString('*', _ => true);
        }

        private static string FilteredReadString(char? mask, Func<char, bool> filter)
        {
            var chars = new List<char>();
            while(true)
            {
                var ck = System.Console.ReadKey(true);
                switch(ck.Key)
                {
                    case ConsoleKey.Enter:
                        System.Console.WriteLine();
                        LeadOut();
                        return new string(chars.ToArray());
                    case ConsoleKey.Backspace:
                        if(chars.Count > 0)
                        {
                            System.Console.CursorLeft -= 1;
                            System.Console.Write(' ');
                            System.Console.CursorLeft -= 1;
                            chars.RemoveAt(chars.Count - 1);
                        }
                        break;
                    default:
                        if (filter(ck.KeyChar))
                        {
                            System.Console.Write(mask ?? ck.KeyChar);
                            chars.Add(ck.KeyChar);
                        }
                        else
                        {
                            System.Console.Beep();
                        }
                        break;
                }
            }
        }

        public static string SqlServerConnectionString(string defaultDataSource = null, string defaultCatalog = null)
        {
            var dataSource = defaultDataSource == null
                ? String("Database instance")
                : String($"Database instance (default=\"{defaultDataSource}\")", defaultDataSource);
            var username = String("Username (blank=IntSec)", "");
            var password = string.IsNullOrEmpty(username) ? null : Password();
            var catalog = defaultCatalog == null
                ? String("Catalog")
                : String($"Catalog (default=\"{defaultCatalog}\")", defaultCatalog);

            var sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = dataSource,
                InitialCatalog = catalog,
                ConnectTimeout = 20
            };

            if(string.IsNullOrEmpty(username))
            {
                sqlBuilder.IntegratedSecurity = true;
            }
            else
            {
                sqlBuilder.IntegratedSecurity = false;
                sqlBuilder.Password = password;
                sqlBuilder.UserID = username;
            }

            return sqlBuilder.ToString();
        }
    }
}
