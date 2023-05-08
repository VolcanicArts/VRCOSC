// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Modules.Serialisation.V1.Models;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Modules.Serialisation.V1;

public class ModuleSerialiser : Serialiser<ModuleManager, SerialisableModuleManager>
{
    protected override string FileName => "modules.json";

    public ModuleSerialiser(Storage storage, NotificationContainer notification, ModuleManager reference)
        : base(storage, notification, reference)
    {
    }

    protected override object GetSerialisableData(ModuleManager moduleManager) => new SerialisableModuleManager(moduleManager.Modules);
}
