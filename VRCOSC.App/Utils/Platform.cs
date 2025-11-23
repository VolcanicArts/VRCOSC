// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Windows.Storage.Pickers;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Shell.Common;
using WinRT.Interop;

namespace VRCOSC.App.Utils;

public static class Platform
{
    public static async Task<string?> PickFileAsync(string filter)
    {
        try
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.Downloads,
                FileTypeFilter = { filter }
            };

            var mainWindow = Application.Current.MainWindow;
            if (mainWindow is null) return string.Empty;

            var mainWindowHandle = new WindowInteropHelper(mainWindow).EnsureHandle();

            InitializeWithWindow.Initialize(picker, mainWindowHandle);

            return (await picker.PickSingleFileAsync())?.Path;
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{nameof(PickFileAsync)} has experienced an issue");
            return string.Empty;
        }
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
                    PInvoke.CoTaskMemFree(pidlFolder);
                    PInvoke.CoTaskMemFree(pidlFile);
                }
            }
        }
    }

    public static void ApplyDefaultStyling(this Window window)
    {
        setTitleBarColour(new HWND(new WindowInteropHelper(window).Handle));
    }

    private static unsafe void setTitleBarColour(HWND hwnd)
    {
        var useImmersiveDarkMode = 1u;
        var result = PInvoke.DwmSetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, &useImmersiveDarkMode, sizeof(uint));

        if (result < 0) Logger.Log("Failed to set immersive dark mode");
    }

    public static void SetPositionFrom(this Window window, DependencyObject? parent, HorizontalPosition horizontal = HorizontalPosition.Center, VerticalPosition vertical = VerticalPosition.Center)
    {
        SetPositionFrom(window, parent is not null ? Window.GetWindow(parent) : null, horizontal, vertical);
    }

    public static void SetPositionFrom(this Window window, Window? parent, HorizontalPosition horizontal = HorizontalPosition.Center, VerticalPosition vertical = VerticalPosition.Center)
    {
        if (parent is not null)
        {
            // when a parent is available, position the child relative to the parent
            var parentDpiScale = PInvoke.GetDpiForSystem() / 96f;
            var parentLeft = parent.Left * parentDpiScale;
            var parentTop = parent.Top * parentDpiScale;
            var parentWidth = parent.Width * parentDpiScale;
            var parentHeight = parent.Height * parentDpiScale;

            var dpiScale = PInvoke.GetDpiForSystem() / 96f;

            var x = horizontal switch
            {
                HorizontalPosition.Left => parentLeft / dpiScale,
                HorizontalPosition.Center => (parentLeft + (parentWidth - window.Width * dpiScale) / 2) / dpiScale,
                HorizontalPosition.Right => (parentLeft + parentWidth - window.Width * dpiScale) / dpiScale,
                _ => throw new ArgumentOutOfRangeException(nameof(horizontal), "Invalid horizontal position")
            };

            var y = vertical switch
            {
                VerticalPosition.Top => parentTop / dpiScale,
                VerticalPosition.Center => (parentTop + (parentHeight - window.Height * dpiScale) / 2) / dpiScale,
                VerticalPosition.Bottom => (parentTop + parentHeight - window.Height * dpiScale) / dpiScale,
                _ => throw new ArgumentOutOfRangeException(nameof(vertical), "Invalid vertical position")
            };

            window.Left = x;
            window.Top = y;
        }
        else
        {
            var monitor = PInvoke.MonitorFromWindow(new HWND(IntPtr.Zero), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY);
            var workingArea = getWorkingArea(monitor);

            var dpiScale = PInvoke.GetDpiForSystem() / 96f;

            var x = horizontal switch
            {
                HorizontalPosition.Left => workingArea.left / dpiScale,
                HorizontalPosition.Center => (workingArea.left + (workingArea.right - workingArea.left - window.Width * dpiScale) / 2) / dpiScale,
                HorizontalPosition.Right => (workingArea.right - window.Width * dpiScale) / dpiScale,
                _ => throw new ArgumentOutOfRangeException(nameof(horizontal), "Invalid horizontal position")
            };

            var y = vertical switch
            {
                VerticalPosition.Top => workingArea.top / dpiScale,
                VerticalPosition.Center => (workingArea.top + (workingArea.bottom - workingArea.top - window.Height * dpiScale) / 2) / dpiScale,
                VerticalPosition.Bottom => (workingArea.bottom - window.Height * dpiScale) / dpiScale,
                _ => throw new ArgumentOutOfRangeException(nameof(vertical), "Invalid vertical position")
            };

            window.Left = x;
            window.Top = y;
        }
    }

    private static RECT getWorkingArea(HMONITOR monitor)
    {
        var monitorInfo = new MONITORINFO
        {
            cbSize = (uint)Marshal.SizeOf<MONITORINFO>()
        };

        if (!PInvoke.GetMonitorInfo(monitor, ref monitorInfo)) throw new InvalidOperationException("Failed to get monitor info");

        return monitorInfo.rcWork;
    }
}

public enum HorizontalPosition
{
    Left,
    Center,
    Right
}

public enum VerticalPosition
{
    Top,
    Center,
    Bottom
}