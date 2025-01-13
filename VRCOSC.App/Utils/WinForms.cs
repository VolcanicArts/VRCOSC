// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell.Common;
using WinRT.Interop;

namespace VRCOSC.App.Utils;

public static class WinForms
{
    public static async Task<string?> PickFileAsync(string filter)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Downloads,
            FileTypeFilter = { filter }
        };

        InitializeWithWindow.Initialize(picker, PInvoke.GetActiveWindow());

        return (await picker.PickSingleFileAsync())?.Path;
    }

    public static unsafe void PresentFile(string filePath)
    {
        ITEMIDLIST* pidlFolder = null;
        ITEMIDLIST* pidlFile = null;

        filePath = filePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        fixed (char* folderPathArray = Path.GetDirectoryName(filePath))
        {
            fixed (char* filePathArray = filePath)
            {
                try
                {
                    var hrFolder = PInvoke.SHParseDisplayName(new PCWSTR(folderPathArray), null, &pidlFolder, 0, null);
                    if (hrFolder != 0) throw new COMException("Failed to parse folder path", hrFolder);

                    var hrFile = PInvoke.SHParseDisplayName(new PCWSTR(filePathArray), null, &pidlFile, 0, null);
                    if (hrFile != 0) throw new COMException("Failed to parse file path", hrFile);

                    var hrSelect = PInvoke.SHOpenFolderAndSelectItems(pidlFolder, 1, &pidlFile, 0);
                    if (hrSelect != 0) throw new COMException("Failed to open folder and select item", hrSelect);
                }
                finally
                {
                    if (pidlFolder != null) PInvoke.ILFree(pidlFolder);
                    if (pidlFile != null) PInvoke.ILFree(pidlFile);
                }
            }
        }
    }
}