using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet
{
    public class EventosPorProcesso
    {
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOW = 5;

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("coredll.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern bool EnableWindow(IntPtr hwnd, bool enabled);

        public static void ExibirProcesso(IntPtr hWnd)
        {
            SetForegroundWindow(hWnd);
            ShowWindow(hWnd, SW_SHOWNORMAL);
            ShowWindow(hWnd, SW_SHOW);

        }
    }
}
