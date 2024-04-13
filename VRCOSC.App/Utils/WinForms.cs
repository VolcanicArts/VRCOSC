// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace VRCOSC.App.Utils;

public static class WinForms
{
    [DllImport("shell32.dll", SetLastError = true)]
    private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, uint dwFlags);

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern void SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext, [Out] out IntPtr pidl, uint sfgaoIn, [Out] out uint psfgaoOut);

    [STAThread]
    public static void OpenFile(string filter, Action<string> filePathCallback)
    {
        var t = new Thread(() =>
        {
            var dlg = new OpenFileDialog
            {
                Multiselect = false,
                Filter = filter
            };

            if (dlg.ShowDialog() == false) return;

            filePathCallback.Invoke(dlg.FileName);
        });

        t.SetApartmentState(ApartmentState.STA);
        t.Start();
    }

    public static void PresentFile(string filePath)
    {
        Task.Run(() =>
        {
            var filePtr = IntPtr.Zero;
            var folderPtr = IntPtr.Zero;

            try
            {
                filePath = filePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

                var folderPath = Path.GetDirectoryName(filePath);

                if (folderPath == null)
                {
                    Logger.Log($"Failed to get directory for {filePath}", level: LogLevel.Debug);
                    return;
                }

                SHParseDisplayName(folderPath, IntPtr.Zero, out folderPtr, 0, out _);

                if (folderPtr == IntPtr.Zero)
                {
                    Logger.Log($"Cannot find folder for '{folderPath}'", level: LogLevel.Error);
                    return;
                }

                SHParseDisplayName(filePath, IntPtr.Zero, out filePtr, 0, out _);

                IntPtr[] fileArray;

                if (filePtr != IntPtr.Zero)
                {
                    fileArray = [filePtr];
                }
                else
                {
                    Logger.Log($"Cannot find file for '{filePath}'", level: LogLevel.Debug);
                    fileArray = [folderPtr];
                }

                SHOpenFolderAndSelectItems(folderPtr, (uint)fileArray.Length, fileArray, 0);
            }
            finally
            {
                if (folderPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(folderPtr);

                if (filePtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(filePtr);
            }
        });
    }
}
