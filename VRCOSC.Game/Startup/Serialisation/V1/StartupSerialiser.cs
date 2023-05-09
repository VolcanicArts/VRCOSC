// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Serialisation;
using VRCOSC.Game.Startup.Serialisation.V1.Models;

namespace VRCOSC.Game.Startup.Serialisation.V1;

public class StartupSerialiser : Serialiser<StartupManager, SerialisableStartupManager>
{
    protected override string FileName => @"startup.json";

    public StartupSerialiser(Storage storage, NotificationContainer notification, StartupManager startupManager)
        : base(storage, notification, startupManager)
    {
    }

    protected override SerialisableStartupManager GetSerialisableData(StartupManager startupManager) => new(startupManager);

    protected override void ExecuteAfterDeserialisation(StartupManager startupManager, SerialisableStartupManager data)
    {
        startupManager.FilePaths.Clear();
        startupManager.FilePaths.AddRange(data.FilePaths.Select(path => new Bindable<string>(path)));
    }
}
