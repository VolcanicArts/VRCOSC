// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Desktop.Updater;
using VRCOSC.Game;
using VRCOSC.Game.Graphics.Updater;

namespace VRCOSC.Desktop;

public partial class VRCOSCGameDesktop : VRCOSCGame
{
    protected override VRCOSCUpdateManager CreateUpdateManager() => new SquirrelUpdateManager();

    public VRCOSCGameDesktop(bool enableModuleDebugMode)
        : base(enableModuleDebugMode)
    {
    }
}
