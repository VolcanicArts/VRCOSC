// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework;
using osu.Framework.Platform;
using Velopack;

namespace VRCOSC.Desktop;

public static class Program
{
#if DEBUG
    private const string base_game_name = @"VRCOSC-V2-Dev";
#else
    private const string base_game_name = @"VRCOSC-V2";
#endif

    [STAThread]
    public static void Main()
    {
        VelopackApp.Build().Run();

        using GameHost host = Host.GetSuitableDesktopHost(base_game_name);
        using osu.Framework.Game game = new VRCOSCGameDesktop();
        host.Run(game);
    }
}
