using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
    }
}
