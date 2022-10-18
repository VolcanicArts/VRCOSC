// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using osu.Framework;
using osu.Framework.Platform;
using Squirrel;

namespace VRCOSC.Desktop;

public static class Program
{
    [DllImport("User32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr h, string m, string c, int type);

    public static void Main()
    {
        ensureSingleInstance();
        initSquirrel();

        using GameHost host = Host.GetSuitableDesktopHost(@"VRCOSC");
        using osu.Framework.Game game = new VRCOSCGameDesktop();
        host.Run(game);
    }

    private static void ensureSingleInstance()
    {
        var singleInstance = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length <= 1;

        if (!singleInstance)
        {
            _ = MessageBox((IntPtr)0, "A VRCOSC instance is already open", "VRCOSC", 0);
            Environment.Exit(0);
        }
    }

    [SupportedOSPlatform("windows")]
    private static void initSquirrel()
    {
        SquirrelAwareApp.HandleEvents(onInitialInstall: (_, tools) =>
        {
            tools.CreateShortcutForThisExe();
            tools.CreateUninstallerRegistryEntry();
        }, onAppUpdate: (_, tools) =>
        {
            tools.CreateUninstallerRegistryEntry();
        }, onAppUninstall: (_, tools) =>
        {
            tools.RemoveShortcutForThisExe();
            tools.RemoveUninstallerRegistryEntry();
        }, onEveryRun: (_, tools, _) =>
        {
            tools.SetProcessAppUserModelId();
        });
    }
}
