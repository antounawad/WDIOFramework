using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Eulg.Setup
{
    public static class WindowTools
    {
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        public static void EnableBorderlessWindowDragging(Window window)
        {
            MouseButtonEventHandler mouseEvent = delegate
            {
                ReleaseCapture();
                SendMessage(new WindowInteropHelper(window).Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            };

            window.MouseLeftButtonDown += mouseEvent;
        }
    }
}
