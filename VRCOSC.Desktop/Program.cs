using osu.Framework;
using osu.Framework.Platform;
using VRCOSC.Game;

namespace VRCOSC.Desktop;

public static class Program
{
    public static void Main()
    {
        using GameHost host = Host.GetSuitableDesktopHost(@"VRCOSC");
        using osu.Framework.Game game = new VRCOSCGame();
        host.Run(game);
    }
}
