using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Eulg.Utilities.Console;
using xbAV.Utilities.Kkzb;

namespace Tools.KkzbGrabber
{
    class Program
    {
        static void Main(string[] args)
        {
            var grabbers = GetImplementations(typeof(IGrabber));
            var filters = GetImplementations(typeof(IFilter));
            var formatters = GetImplementations(typeof(IFormatter));

            var selectedGrabberType = Prompt.Select("Select source to grab data from", grabbers, g => g.Key).Value;
            var grabber = (IGrabber)Activator.CreateInstance(selectedGrabberType);

            var beitraege = grabber.GetBeitraege().ToList();
            
            Console.WriteLine();
            Console.WriteLine("Grabbed {0} values from the selected sources.", beitraege.Count);
            Console.WriteLine();

            var first = true;
            while (Prompt.YesNo($"Apply {(first ? "a" : "another")} filter to the extracted data?"))
            {
                first = false;

                var selectedFilterType = Prompt.Select("Select filter to apply", filters, f => f.Key).Value;
                var filter = (IFilter)Activator.CreateInstance(selectedFilterType);

                if(!filter.Initialize()) continue;

                var filtered = new List<Provider>();

                foreach(var item in beitraege)
                {
                    var local = item;

                    if(filter.Filter(ref local))
                    {
                        filtered.Add(local);
                    }
                }

                beitraege = filtered;

                filter.ShowAndResetCounters(Console.Out);
                Console.WriteLine();
                Console.WriteLine();
            }

            var selectedFormatterType = Prompt.Select("Select output format", formatters, f => f.Key).Value;
            var formatter = (IFormatter)Activator.CreateInstance(selectedFormatterType);

            FormatAndShowData(beitraege, formatter);

            Prompt.Wait();
        }

        private static void FormatAndShowData(IEnumerable<Provider> data, IFormatter formatter)
        {
            var filename = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".txt"));
            using(var file = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                using (var writer = new StreamWriter(file, Encoding.UTF8))
                {
                    formatter.Write(data, writer);
                }
            }

            using(Process.Start(filename)) { }
        }

        private static KeyValuePair<string, Type>[] GetImplementations(Type interfaceType)
        {
            if(!interfaceType.IsInterface) throw new ArgumentException();

            var types = interfaceType.Assembly.GetExportedTypes().Concat(Assembly.GetExecutingAssembly().GetTypes());
            return types
                .Where(t => !t.IsInterface && !t.IsAbstract && interfaceType.IsAssignableFrom(t))
                .Select(t => new KeyValuePair<DescriptionAttribute, Type>(t.GetCustomAttribute(typeof(DescriptionAttribute), false) as DescriptionAttribute, t))
                .Where(t => t.Key != null)
                .Select(t => new KeyValuePair<string, Type>(t.Key.Description, t.Value))
                .ToArray();
        } 
    }
}
