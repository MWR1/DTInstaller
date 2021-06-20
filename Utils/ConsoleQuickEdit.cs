using System;
using System.Runtime.InteropServices;

using static DTInstaller.Utils.Logger;

namespace DTInstaller.Utils
{
    // On Windows, if the user doesn't press any key while in the console, the console will not update.
    // This is caused by a feature known as "Quick Edit".
    // Source: https://stackoverflow.com/questions/13656846/how-to-programmatic-disable-c-sharp-console-applications-quick-edit-mode
    static class ConsoleQuickEdit
    {
        const uint ENABLE_QUICK_EDIT = 0x0040;

        // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
        const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        internal static void Disable()
        {
            try
            {
                IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

                // Get current console mode.
                uint consoleMode;
                GetConsoleMode(consoleHandle, out consoleMode);

                // Clear the quick edit bit in the mode flags.
                consoleMode &= ~ENABLE_QUICK_EDIT;

                // Set the new mode.
                SetConsoleMode(consoleHandle, consoleMode);
            }
            catch (Exception error)
            {
                DebugLog(LogVariant.Error, $"Could not disable quick edit. Reason: {error}");
            }
        }
    }

}
