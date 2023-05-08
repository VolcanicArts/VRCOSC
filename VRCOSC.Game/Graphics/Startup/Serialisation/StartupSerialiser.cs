// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Graphics.Startup.Serialisation;

public class StartupSerialiser : Serialiser<StartupManager, List<StartupModel>>
{
    protected override string FileName => @"startup.json";

    public StartupSerialiser(Storage storage, NotificationContainer notification, StartupManager startupManager)
        : base(storage, notification, startupManager)
    {
    }

    protected override object GetSerialisableData(StartupManager startupManager)
    {
        var data = new List<StartupModel>();
        startupManager.FilePaths.ForEach(path => data.Add(new StartupModel(path.Value)));
        return data;
    }
}
