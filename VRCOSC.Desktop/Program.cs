// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;
using osu.Framework;
using osu.Framework.Platform;
using Squirrel;

namespace VRCOSC.Desktop;

public static class Program
{
    public static void Main()
    {
        initSquirrel();

        using GameHost host = Host.GetSuitableDesktopHost(@"VRCOSC");
        using osu.Framework.Game game = new VRCOSCGameDesktop();
        host.Run(game);
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
