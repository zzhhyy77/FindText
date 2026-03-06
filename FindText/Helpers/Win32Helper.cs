using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FindText.Helpers
{
    internal class Win32Helper
    {

        [DllImport("USER32.DLL")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        internal static extern int ShowWindow(IntPtr hwnd, int nCmdShow);


        internal static void OpenFileWithShell(string filePath, string arg = "")
        {
            Process.Start(new ProcessStartInfo(filePath) { Arguments = arg, UseShellExecute = true });
        }
    }
}
