using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace Tools.BlzImport
{
    internal struct SortingCode
    {
        public string Code;
        public int Flags;
        public string Name;
        public string ZipCode;
        public string City;
        public string ShortName;
        public string Bic;
        public string Pan;
        public string Pbm;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnClickImportCsv(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DereferenceLinks = true,
                Multiselect = false,
                Filter = "Comma-separated values|*.csv",
                FilterIndex = 0,
                ShowReadOnly = false,
                Title = "Select data source",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (ofd.ShowDialog(this) != true) return;

            var filename = ofd.FileName;
            var dispatcher = Dispatcher;

            Task.Run(() =>
            {
                try
                {
                    dispatcher.Invoke(() =>
                    {
                        ((UIElement)sender).IsEnabled = false;
                        OutputBox.Text = "";
                    });

                    using (var buffer = new StringWriter())
                    {
                        BlzImport(filename, buffer);
                        dispatcher.Invoke(() => { OutputBox.Text = buffer.ToString(); });
                    }
                }
                finally
                {
                    dispatcher.Invoke(() => { ((UIElement) sender).IsEnabled = true; });
                }
            });
        }

        private void OnClickSaveToFile(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                DereferenceLinks = true,
                AddExtension = true,
                CreatePrompt = false,
                Filter = "T-SQL Script|*.sql",
                FilterIndex = 0,
                Title = "Save script as",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (sfd.ShowDialog(this) != true) return;

            try
            {
                File.WriteAllText(sfd.FileName, OutputBox.Text, Encoding.GetEncoding(1252));

                MessageBox.Show("Script successfully written to " + sfd.FileName, "OK");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while writing: " + ex, "Save failed");
            }
        }

        private static void BlzImport(string filename, TextWriter output)
        {
            try
            {
                using (var input = new StreamReader(filename))
                {
                    input.ReadLine();

                    var codes = new List<SortingCode>(20000);

                    string line;
                    while ((line = input.ReadLine()) != null)
                    {
                        var ptr = 0;
                        codes.Add(new SortingCode
                        {
                            Code = GetToken(line, ref ptr),
                            Flags = int.Parse(GetToken(line, ref ptr)),
                            Name = GetToken(line, ref ptr),
                            ZipCode = GetToken(line, ref ptr),
                            City = GetToken(line, ref ptr),
                            ShortName = GetToken(line, ref ptr),
                            Pan = GetToken(line, ref ptr),
                            Bic = GetToken(line, ref ptr),
                            Pbm = GetToken(line, ref ptr)
                        });
                    }

                    string Esc(string s) => s.Replace("'", "''");

                    output.WriteLine("TRUNCATE TABLE [dbo].[BLZ];");
                    output.WriteLine("GO");
                    output.WriteLine();

                    var index = 0;
                    foreach (var code in codes)
                    {
                        if (index == 0)
                        {
                            output.WriteLine("INSERT INTO [dbo].[BLZ] ([blz], [mark], [name], [plz], [place], [shortName], [pan], [bic], [pbm]) VALUES");
                        }

                        output.Write("\t('{0}', {1}, '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')", Esc(code.Code), code.Flags, Esc(code.Name), Esc(code.ZipCode), Esc(code.City), Esc(code.ShortName), Esc(code.Pan), Esc(code.Bic), Esc(code.Pbm));
                        if (++index < 800)
                        {
                            output.WriteLine(",");
                            continue;
                        }

                        output.WriteLine(";");
                        output.WriteLine();
                        index = 0;
                    }

                    if (index > 0)
                    {
                        output.WriteLine(";");
                        output.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                output.WriteLine();
                output.WriteLine();
                output.WriteLine();
                output.WriteLine();
                output.WriteLine(ex.ToString());

                MessageBox.Show("An error occurred during import: " + ex.Message, "Import failed");
            }
        }

        private static string GetToken(string line, ref int startIndex)
        {
            var q = line.IndexOf('"', startIndex);
            var c = line.IndexOf(',', startIndex);

            if (q >= 0 && q < c)
            {
                ++q;
                var e = line.IndexOf('"', q);
                startIndex = e + 2;
                return e == q ? string.Empty : line.Substring(q, e - q);
            }

            var token = c == startIndex ? string.Empty : line.Substring(startIndex, c - startIndex);
            startIndex = c + 1;
            return token;
        }
    }
}
