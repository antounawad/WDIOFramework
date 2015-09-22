using System;
using System.Windows;
using System.Windows.Controls;

namespace Eulg.Setup.Pages
{
    public partial class InstProgress : UserControl, ISetupPageBase
    {
        public InstProgress()
        {
            InitializeComponent();
        }

        public string PageTitle { get; set; }
        public Type PrevPage { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrev { get; set; }
        public string NextButtonText { get; set; }
        public bool NextEnabled { get; set; }
        public bool PrevEnabled { get; set; }
        public void OnLoad()
        {
        }
        public void OnLoadComplete() { }
        public void OnNext()
        {
            throw new NotImplementedException();
        }

        public bool OnPrev()
        {
            return true;
        }

        public bool OnClose()
        {
            return false;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            SetupHelper.CancelRequested = true;
        }

    }
}
