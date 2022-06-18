// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;
using osu.Framework;
using osu.Framework.Platform;
using Squirrel;
using VRCOSC.Game;

namespace VRCOSC.Desktop;

public static class Program
{
    public static void Main()
    {
        initSquirrel();

        using GameHost host = Host.GetSuitableDesktopHost(@"VRCOSC");
        using osu.Framework.Game game = new VRCOSCGame();
        host.Run(game);
    }

    [SupportedOSPlatform("windows")]
    private static void initSquirrel()
    {
        SquirrelAwareApp.HandleEvents(onInitialInstall: OnAppInstall, onAppUninstall: OnAppUninstall, onEveryRun: OnAppRun);
    }

    private static void OnAppInstall(SemanticVersion version, IAppTools tools)
    {
        tools.CreateShortcutForThisExe();
    }

    private static void OnAppUninstall(SemanticVersion version, IAppTools tools)
    {
        tools.RemoveShortcutForThisExe();
    }

    private static void OnAppRun(SemanticVersion version, IAppTools tools, bool firstRun)
    {
        tools.SetProcessAppUserModelId();
    }
}
