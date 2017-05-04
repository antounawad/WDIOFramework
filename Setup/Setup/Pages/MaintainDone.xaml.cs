using System;
using System.Windows.Controls;
using Eulg.Setup.Shared;
using Path = System.IO.Path;

namespace Eulg.Setup.Pages
{
    public partial class MaintainDone : UserControl, ISetupPageBase
    {
        public MaintainDone()
        {
            InitializeComponent();

            PageTitle = "Deinstallation abgeschlossen";
            HasPrev = false;
            HasNext = true;
        }

        public string PageTitle { get; set; }
        public Type PrevPage { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrev { get; set; }
        public string NextButtonText { get; set; }

        public void OnLoad() { }

        public void OnLoadComplete()
        {
            if (SetupHelper.OfflineInstall) OnNext();
        }
        public void OnNext()
        {
            SetupHelper.DelTree(Path.Combine(Path.GetTempPath(), Temp.WebInstTempFolder));
            SetupHelper.DelTree(Path.Combine(Path.GetTempPath(), Temp.SetupTempFolder));

            MainWindow.Instance.Close();
        }
        public bool OnPrev()
        {
            return true;
        }
        public bool OnClose()
        {
            return true;
        }

    }
}
