// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Platform;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Remote;

namespace VRCOSC.Game;

public class AppManager
{
    public ModuleManager ModuleManager { get; private set; } = null!;
    public RemoteModuleSourceManager RemoteModuleSourceManager { get; private set; } = null!;

    public void Initialise(Storage storage)
    {
        ModuleManager = new ModuleManager(storage);
        RemoteModuleSourceManager = new RemoteModuleSourceManager(storage);
    }
}
