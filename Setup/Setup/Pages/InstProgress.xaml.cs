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

        public bool OnPrev() => true;

        public bool OnClose() => false;

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) => SetupHelper.CancelRequested = true;

        public void UpdateProgress(int percent, string message, string header)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                if (percent < 0)
                {
                    ProgressBar.IsIndeterminate = true;
                }
                else
                {
                    ProgressBar.IsIndeterminate = false;
                    ProgressBar.Value = percent;
                }
                LabelFileCount.Text = message;
                if (!string.IsNullOrEmpty(header)) LabelProgress.Text = header;
            }));
        }

    }
}
