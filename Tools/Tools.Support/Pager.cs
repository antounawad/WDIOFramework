using System.Windows.Controls;

namespace Eulg.Client.SupportTool
{
    public static class Pager
    {
        public static MainWindow ContainerWindow { get; set; }

        public static void NavigateTo(UserControl view)
        {
            ContainerWindow.NavigateTo(view);
        }
    }
}
