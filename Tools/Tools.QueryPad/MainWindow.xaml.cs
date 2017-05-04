using System;
using System.Linq;
using System.Windows;
using Eulg.Tools.QueryPad.Controls;
using Telerik.Windows.Controls;

namespace Eulg.Tools.QueryPad
{
    public partial class MainWindow : RadRibbonWindow
    {
        static MainWindow()
        {
            //IsWindowsThemeEnabled = true;
            VisualStudio2013Palette.LoadPreset(VisualStudio2013Palette.ColorVariation.Blue);
        }
        public MainWindow()
        {
            InitializeComponent();

            foreach (var colorVariation in Enum.GetValues(typeof(VisualStudio2013Palette.ColorVariation)))
            {
                var menuItem = new RadMenuItem { Header = colorVariation, Tag = colorVariation };
                menuItem.Click += (sender, args) =>
                {
                    var s = sender as RadMenuItem;
                    if (s == null) { return; }
                    var c = s.Tag as VisualStudio2013Palette.ColorVariation?;
                    if (c == null) { return; }
                    ChangeTheme(c.Value);
                };
                MenuChangeTheme.Items.Add(menuItem);
            }

            FillTreeView();

            NewEditor();
        }

        public EditorControl GetSelectedEditor()
        {
            var selectedPane = PaneGroupEditors.SelectedItem as RadPane;
            if (selectedPane == null)
            {
                return null;
            }
            return selectedPane.Content as EditorControl;
        }
        public ResultsControl GetSelectedResult()
        {
            var selectedPane = PaneGroupResults.SelectedItem as RadPane;
            if (selectedPane == null)
            {
                return null;
            }
            return selectedPane.Content as ResultsControl;
        }

        public void ChangeTheme(VisualStudio2013Palette.ColorVariation colorVariation)
        {
            VisualStudio2013Palette.LoadPreset(colorVariation);
        }

        private void ButtonAusfuehren_Click(object sender, RoutedEventArgs e)
        {
            var editor = GetSelectedEditor();
            if (editor == null)
            {
                return;
            }
            editor.Run();
        }
        private void ButtonNeuerEditor_Click(object sender, RoutedEventArgs e)
        {
            NewEditor();
        }

        private void FillTreeView()
        {
            EulgHelper.ReadEulgDatabasesFromRegistry(TreeViewBrowser);
            var treeItemManualRoot = new RadTreeViewItem { Header = "Andere Datenbanken" };
            TreeViewBrowser.Items.Add(treeItemManualRoot);
        }
        private void NewEditor()
        {
            var editorControl = new EditorControl();
            var resultsControl = new ResultsControl();
            editorControl.Results = resultsControl;
            resultsControl.Editor = editorControl;
            var paneEditor = new RadPane { Header = String.Format("Editor {0}", (PaneGroupEditors.Items.Count + 1)), Content = editorControl };
            paneEditor.Activated += PaneEditorOnActivated;
            PaneGroupEditors.Items.Add(paneEditor);
            paneEditor.IsSelected = true;
            var paneResult = new RadPane { Header = String.Format("Result {0}", (PaneGroupResults.Items.Count + 1)), Content = resultsControl };
            paneResult.Activated += PaneResultOnActivated;
            PaneGroupResults.Items.Add(paneResult);
            RibbonView.Title = paneEditor.Header as string;
        }
        private void PaneEditorOnActivated(object sender, EventArgs eventArgs)
        {
            var paneEditor = sender as RadPane;
            if (paneEditor == null)
            {
                return;
            }
            var editorControl = paneEditor.Content as EditorControl;
            if (editorControl == null)
            {
                return;
            }
            var x = PaneGroupResults.EnumeratePanes().SingleOrDefault(s => s.Content.Equals(editorControl.Results));
            if (x == null)
            {
                return;
            }
            x.IsSelected = true;
            RibbonView.Title = paneEditor.Header as string;
        }
        private void PaneResultOnActivated(object sender, EventArgs eventArgs)
        {
            var paneResults = sender as RadPane;
            if (paneResults == null)
            {
                return;
            }
            var resultsControl = paneResults.Content as ResultsControl;
            if (resultsControl == null)
            {
                return;
            }
            var x = PaneGroupEditors.EnumeratePanes().SingleOrDefault(s => s.Content == resultsControl.Editor);
            if (x == null)
            {
                return;
            }
            x.IsSelected = true;
        }
    }
}
