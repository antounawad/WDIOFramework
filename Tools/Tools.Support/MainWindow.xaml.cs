using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Eulg.Client.SupportTool.Views;

namespace Eulg.Client.SupportTool
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowTools.EnableBorderlessWindowDragging(this);
            Pager.ContainerWindow = this;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Support.Init();
            Pager.NavigateTo(new MainMenu());
        }

        public void NavigateTo(UserControl view)
        {
            Dispatcher.Invoke(() => ViewContainer.Content = view);
        }

        private void CloseIcon_OnMouseLeftButtonDown(object sender, EventArgs e)
        {
            Close();
        }

        private void MainWindow_OnSourceInitialized(object sender, EventArgs e)
        {
            //Window Shadow
            var hwnd = new WindowInteropHelper(this).Handle;
            var attrValue = 2;

            var margin = new App.Margins
            {
                BottomHeight = -1,
                LeftWidth = -1,
                RightWidth = -1,
                TopHeight = -1
            };

            try
            {
                if (App.DwmSetWindowAttribute(hwnd, 2, ref attrValue, 4) == 0)
                {
                    App.DwmExtendFrameIntoClientArea(hwnd, ref margin);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("DWM-Api not available: Unsupported OS");
            }
        }
    }
}
