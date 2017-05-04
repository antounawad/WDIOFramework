using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace Eulg.Tools.QueryPad.Controls
{
    public partial class EditorControl : UserControl
    {
        public ResultsControl Results { get; set; }

        public EditorControl()
        {
            InitializeComponent();
            InitEditor();
            ComboBoxDatabases.ItemsSource = DbHelper.Databases;
            ComboBoxDatabases.DisplayMemberPath = "Name";
            if (ComboBoxDatabases.Items.Count > 0)
                ComboBoxDatabases.SelectedIndex = 0;
        }

        public void Run()
        {
            var db = ComboBoxDatabases.SelectedValue as Database;
            if (db == null)
                return;
            try
            {
                using (var c = db.GetConnection())
                {
                    c.Open();
                    var r = new SQLiteDataAdapter(TextEditor.Text, c);
                    var dataSet = new DataSet();
                    r.Fill(dataSet);
                    if(dataSet.Tables.Count > 0)
                    {
                        Results.SetData(dataSet);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, db.Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitEditor()
        {
            var foldingManager = FoldingManager.Install(TextEditor.TextArea);
            var foldingStrategy = new XmlFoldingStrategy();
            foldingStrategy.UpdateFoldings(foldingManager, TextEditor.Document);
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Eulg.Tools.QueryPad.SqlSyntaxDefinition.xshd"))
            {
                using (var reader = new XmlTextReader(s))
                {
                    TextEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }


            TextEditor.TextArea.AllowCaretOutsideSelection();
            //TextEditor.TextArea.IndentationStrategy
            TextEditor.TextArea.Options.AllowScrollBelowDocument = true;
            TextEditor.TextArea.Options.EnableRectangularSelection = true;
            TextEditor.TextArea.Options.EnableVirtualSpace = true;
            TextEditor.TextArea.Options.HideCursorWhileTyping = true;
            TextEditor.TextArea.Options.HighlightCurrentLine = true;
            //TextEditor.TextArea.Options

            TextEditor.TextArea.TextEntered += TextAreaOnTextEntered;
        }
        private void TextAreaOnTextEntered(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            if (textCompositionEventArgs.Text == ".")
            {
                var completionWindow = new CompletionWindow(TextEditor.TextArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                data.Add(new MyCompletionData("Item1"));
                data.Add(new MyCompletionData("Item2"));
                data.Add(new MyCompletionData("Item3"));
                completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
        }
        private void TextEditor_OnGotFocus(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("TextEditor_OnGotFocus");
        }
        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("UserControl_GotFocus");
        }


        public class MyCompletionData : ICompletionData
        {
            public string Text { get; private set; }
            public MyCompletionData(string text)
            {
                this.Text = text;
            }
            
            public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
            {
                textArea.Document.Replace(completionSegment, this.Text);
            }
            public ImageSource Image { get { return null; } }
            public object Content { get { return Text; } }
            public object Description { get { return "tja"; } }
            public double Priority { get { return 0; } }
        }


    }
}
