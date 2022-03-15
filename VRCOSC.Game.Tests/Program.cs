// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework;
using osu.Framework.Platform;

namespace VRCOSC.Game.Tests;

public static class Program
{
    public static void Main()
    {
        using GameHost host = Host.GetSuitableDesktopHost("visual-tests");
        using var game = new VRCOSCTestBrowser();
        host.Run(game);
    }
}
